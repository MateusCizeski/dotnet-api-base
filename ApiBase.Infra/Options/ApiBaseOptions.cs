namespace ApiBase.Infra.Options
{
    /// <summary>
    /// Global options for the ApiBase query engine.
    /// Register with <c>services.AddApiBaseCore&lt;TContext&gt;(..., options =&gt; options.MaxPageSize = 50)</c>.
    /// </summary>
    public class ApiBaseOptions
    {
        /// <summary>Page size used when the request does not send <c>limit</c>. Default: 25.</summary>
        public int DefaultPageSize { get; set; } = 25;

        /// <summary>
        /// Upper bound for <c>limit</c>. Requests asking for more receive at most this many records,
        /// so a client cannot make the API load millions of rows (OWASP API4). Default: 100.
        /// </summary>
        public int MaxPageSize { get; set; } = 100;

        /// <summary>
        /// Returns the page size to use: the requested limit (or the default) clamped to [1, MaxPageSize].
        /// </summary>
        public int ResolvePageSize(int? requestedLimit)
        {
            int max = Math.Max(1, MaxPageSize);
            return Math.Clamp(requestedLimit ?? DefaultPageSize, 1, max);
        }
    }
}
