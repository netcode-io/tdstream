namespace Tdstream.Server
{
    /// <summary>
    /// Class TdsRequest.
    /// </summary>
    public class TdsRequest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TdsRequest"/> class.
        /// </summary>
        /// <param name="query">The query.</param>
        public TdsRequest(string query)
        {
            Query = query;
        }

        /// <summary>
        /// Gets the query.
        /// </summary>
        /// <value>The query.</value>
        public string Query { get; }

        /// <summary>
        /// Parses the query.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>TdsRequest.</returns>
        public static TdsRequest Parse(string query)
        {
            return new TdsRequest(query);
        }
    }
}
