using Microsoft.Data.SqlClient;
using NFluent;
using NSubstitute.Extensions;
using NUnit.Framework;
using System;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Tdstream.Server
{
    public class TdsServerTest
    {
        [Test]
        public async Task Should_call_handler_on_request()
        {
            // given
            var port = TdsServer.FindFreeTcpPort();
            var server = BuildServer(port, "content");
            // when
            var task = Task.Run(() => server.Run());

            // then
            using (var conn = new SqlConnection($"Data Source=tcp:localhost,{port};Initial Catalog=Test;MultipleActiveResultSets=False;user=guest;pwd=sybase;Encrypt=false;trustservercertificate=false"))
            using (var com = new SqlCommand("request", conn))
            {
                conn.Open();
                var content = (await com.ExecuteScalarAsync()).ToString();
                Check.That(content).Contains("content");
            }
        }

        [Test]
        public async Task Should_stop_handling_requests_after_dispose()
        {
            // given
            var port = TdsServer.FindFreeTcpPort();
            var server = BuildServer(port, "content");
            // when
            var serverTask = Task.Run(() => server.Run());
            await Task.Delay(TimeSpan.FromMilliseconds(100));
            server.Dispose();

            // then
            using (var conn = new SqlConnection($"Data Source=tcp:localhost,{port};Initial Catalog=Test;MultipleActiveResultSets=False;user=guest;pwd=sybase;Encrypt=false;trustservercertificate=false"))
            using (var com = new SqlCommand("request", conn))
            {
                Check.ThatCode(() => conn.OpenAsync().Wait()).ThrowsAny();
            }
        }

        [Test]
        public void Should_throw_exception_running_a_disposed_server()
        {
            // given
            var port = TdsServer.FindFreeTcpPort();
            var server = BuildServer(port, "content");
            // when
            server.Dispose();

            // then
            var serverTask = Task.Run(() => server.Run());
            Check.ThatCode(() => server.Run()).Throws<InvalidOperationException>();
        }

        static TdsServer BuildServer(int port, string content) =>
            new TdsServer(port, ctx =>
            {
                var response = ctx.Response;
                response.NewTable(1)
                    .Column(0, "content", typeof(string), 30);
                response.NewRow()
                    .ColumnData(0, content);
                var info = response.Info;
                info.CurrentRow = Marshal.StringToHGlobalAnsi(content);
                info.Columns[0].ColumnData = info.CurrentRow;
                response.Done();
            });
    }
}
