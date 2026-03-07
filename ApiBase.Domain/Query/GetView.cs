namespace ApiBase.Domain.Query
{
    /// <summary>
    /// Represents the result of a paginated query.
    /// Contains the total record count (before pagination) and the current page content.
    /// </summary>
    public class GetView
    {
        /// <summary>
        /// Total number of records matching the query, before pagination is applied.
        /// </summary>
        public int Total { get; set; }

        /// <summary>
        /// The records for the current page, as a list of objects.
        /// Each item may be a typed view, a dynamic projection, or a dictionary depending on the query.
        /// </summary>
        public IList<object> Content { get; set; }
    }
}
