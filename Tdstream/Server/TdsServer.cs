using System;
using System.IO;
using System.Net;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using FreeTdsContext = FreeTds.TdsContext;
using FreeTds;

namespace Tdstream.Server
{
    /// <summary>
    /// Class TdsServer.
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public class TdsServer : IDisposable
    {
        readonly int _port;
        readonly Action<TdsContext> _handler;
        volatile bool _running;
        volatile bool _disposed;
        X509Certificate _serverCertificate;
        FreeTdsContext _server;

        /// <summary>
        /// Initializes a new instance of the <see cref="TdsServer"/> class.
        /// </summary>
        /// <param name="port">The port.</param>
        /// <param name="handler">The handler.</param>
        public TdsServer(int port, Action<TdsContext> handler)
        {
            _port = port;
            _handler = handler;
        }

        /// <summary>
        /// Runs the specified certificate.
        /// </summary>
        /// <param name="certificate">The certificate.</param>
        /// <exception cref="InvalidOperationException">Cannot run on a disposed server</exception>
        public void Run(X509Certificate certificate = null)
        {
            if (_disposed)
                throw new InvalidOperationException("Cannot run on a disposed server");
            _serverCertificate = certificate;
            _server = new FreeTdsContext();
            _running = true;
            Task.Run(async () =>
            {
                while (_running)
                {
                    var client = _server.Listen(_port) ?? throw new Exception("Error Listening");
                    if (!_running)
                    {
                        client.Dispose();
                        return;
                    }
                    var cancelTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                    var tdsContext = await Task.Run(async () => await ProcessLoginAsync(client, cancelTokenSource), cancelTokenSource.Token);
                    if (tdsContext == null)
                    {
                        client.Dispose();
                        continue;
                    }
                    var task = Task.Run(async () => await ProcessClientAsync(tdsContext, cancelTokenSource), cancelTokenSource.Token);
                }
            });
        }

        Task<TdsContext> ProcessLoginAsync(TdsSocket client, CancellationTokenSource cancelTokenSource)
        {
            TdsContext tdsContext = null;
            try
            {
                var login = client.AllocReadLogin(0x702) ?? throw new Exception("Error reading login");
                if (login.UserName != "guest" && login.Password != "sybase")
                    return Task.FromResult(tdsContext);
                client.OutFlag = TDS_PACKET_TYPE.TDS_REPLY;
                //client.EnvChange(P.TDS_ENV_DATABASE, "master", "pubs2");
                //client.SendMsg(5701, 2, 10, "Changed database context to 'pubs2'.", "JDBC", "ZZZZZ", 1);
                if (!login.Value.suppress_language)
                {
                    //client.EnvChange(P.TDS_ENV_LANG, null, "us_english");
                    //client.SendMsg(5703, 1, 10, "Changed language setting to 'us_english'.", "JDBC", "ZZZZZ", 1);
                }
                //client.EnvChange(P.TDS_ENV_PACKSIZE, null, "512");
                client.SendLoginAck("Microsoft SQL Server", G.TDS_MS_VER(10, 0, 6000));
                if (G.IS_TDS50(client.Conn.Value))
                    client.SendCapabilitiesToken();
                client.SendDoneToken(0, 1);
                client.FlushPacket();
                return Task.FromResult(new TdsContext(client, cancelTokenSource.Token));
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception: {e.Message}");
                if (e.InnerException != null)
                    Console.WriteLine($"Inner exception: {e.InnerException.Message}");
                return Task.FromResult(tdsContext);
            }
        }

        Task ProcessClientAsync(TdsContext tdsContext, CancellationTokenSource cancelTokenSource)
        {
            try
            {
                var client = tdsContext.Client;
                while (!client.IsDead)
                {
                    var command = client.GetGenericQuery();
                    tdsContext.Request = TdsRequest.Parse(command);
                    using (tdsContext.Response = new TdsResponse(client))
                        _handler(tdsContext);
                    client.FlushPacket();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception: {e.Message}");
                if (e.InnerException != null)
                    Console.WriteLine($"Inner exception: {e.InnerException.Message}");
                Console.WriteLine("Connection closed.");
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _disposed = true;
            _running = false;
            _server?.Dispose();
        }

        /// <summary>
        /// Finds the certificate.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="findType">Type of the find.</param>
        /// <param name="storeName">Name of the store.</param>
        /// <param name="location">The location.</param>
        /// <returns>X509Certificate2.</returns>
        public static X509Certificate2 FindCertificate(object value, X509FindType findType = X509FindType.FindBySubjectName, string storeName = null, StoreLocation location = StoreLocation.LocalMachine)
        {
            if (value == null || (value is string valueAsString && valueAsString.Length == 0))
                return null;
            using (var store = new X509Store(storeName ?? "MY", location))
            {
                store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);
                return store.Certificates.Find(findType, value, false).Cast<X509Certificate2>()
                    .Where(x => x.NotBefore <= DateTime.Now)
                    .OrderBy(x => x.NotAfter)
                    .FirstOrDefault();
            }
        }

        /// <summary>
        /// Finds the free TCP port.
        /// </summary>
        /// <returns>System.Int32.</returns>
        public static int FindFreeTcpPort()
        {
            var l = new TcpListener(IPAddress.Loopback, 0);
            l.Start();
            var port = ((IPEndPoint)l.LocalEndpoint).Port;
            l.Stop();
            return port;
        }
    }
}
