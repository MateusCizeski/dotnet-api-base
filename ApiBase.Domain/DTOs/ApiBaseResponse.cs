namespace ApiBase.Domain.DTOs
{
    /// <summary>
    /// Base response envelope returned by all ApiBase endpoints.
    /// </summary>
    public class ApiBaseResponse
    {
        /// <summary>The response payload. May be a typed object, list or dynamic projection.</summary>
        public object Content { get; set; }

        /// <summary>Human-readable message describing the result of the operation.</summary>
        public string Message { get; set; }

        /// <summary>Indicates whether the request completed successfully.</summary>
        public bool Success { get; set; }
    }

    /// <summary>
    /// Response envelope for failed requests.
    /// <see cref="ApiBaseResponse.Success"/> is always false.
    /// </summary>
    public class ApiErrorResponse : ApiBaseResponse
    {
        /// <summary>
        /// Optional error type identifier, e.g. "AppError404".
        /// Useful for client-side error handling without parsing the message.
        /// </summary>
        public string ErrorType { get; set; }

        public ApiErrorResponse()
        {
            Success = false;
        }
    }

    /// <summary>
    /// Response envelope for successful requests.
    /// <see cref="ApiBaseResponse.Success"/> is always true.
    /// </summary>
    public class ApiSuccessResponse : ApiBaseResponse
    {
        /// <summary>
        /// Optional application-level request code for client tracking or idempotency.
        /// </summary>
        public int? RequestCode { get; set; }

        public ApiSuccessResponse()
        {
            Success = true;
        }
    }

    /// <summary>
    /// Paginated response envelope returned by list endpoints.
    /// Contains the current page content and the total record count before pagination.
    /// </summary>
    public class ApiPaginatedResponse
    {
        /// <summary>
        /// The records for the current page.
        /// May be an IQueryable, materialized list or dynamic projection depending on the pipeline stage.
        /// </summary>
        public object Content { get; set; }

        /// <summary>Total number of records matching the query, before pagination.</summary>
        public int Total { get; set; }
    }

    /// <summary>
    /// Internal DTO representing a query result still in IQueryable form (not yet paginated or materialized).
    /// Used within the infrastructure layer before pagination is applied.
    /// Not intended for use in controller responses — use <see cref="ApiPaginatedResponse"/> instead.
    /// </summary>
    public class QueryExecutionDTO
    {
        /// <summary>The composed query, ready for pagination or further filtering.</summary>
        public IQueryable<object> Data { get; set; }

        /// <summary>Total record count before pagination.</summary>
        public int Total { get; set; }
    }
}
