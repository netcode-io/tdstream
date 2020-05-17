using FreeTds;
using System.Threading;

namespace Tdstream.Server
{
    /// <summary>
    /// Class TdsContext.
    /// </summary>
    public class TdsContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TdsContext"/> class.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="token">The token.</param>
        public TdsContext(TdsSocket client, CancellationToken token)
        {
            Client = client;
            Token = token;
        }

        /// <summary>
        /// Gets the client.
        /// </summary>
        /// <value>The client.</value>
        public TdsSocket Client { get; }

        /// <summary>
        /// Gets the TDS request.
        /// </summary>
        /// <value>The TDS request.</value>
        public TdsRequest Request { get; internal set; }

        /// <summary>
        /// Gets the TDS response.
        /// </summary>
        /// <value>The TDS response.</value>
        public TdsResponse Response { get; internal set; }

        /// <summary>
        /// Gets the token.
        /// </summary>
        /// <value>The token.</value>
        public CancellationToken Token { get; }
    }
}
