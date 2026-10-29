using ApiBase.Domain.DTOs;
using ApiBase.Infra.Helpers;
using ApiBase.Infra.Query;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;

namespace ApiBase.Controller.Base
{
    /// <summary>
    /// Base controller providing standardized response helpers, dynamic query building
    /// and structured logging for all ApiBase controllers.
    /// Exceptions are handled by <c>ApiBaseExceptionHandler</c>, not by the controllers.
    /// </summary>
    public class BaseApiController<TApplication> : ControllerBase
    {
        protected TApplication Application { get; }
        protected ILogger Logger { get; }

        protected BaseApiController(TApplication application, ILogger logger)
        {
            Application = application;
            Logger = logger;
        }

        // -------------------------
        // Response helpers
        // -------------------------

        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult Respond<TResponse>(HttpStatusCode statusCode, TResponse response)
        {
            return new ObjectResult(response) { StatusCode = (int)statusCode };
        }

        /// <summary>
        /// Returns 400 with the given envelope. The message is sent to the client: never pass exception details.
        /// </summary>
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult RespondError(ApiErrorResponse error)
        {
            Logger.LogWarning("Request error: {Message} | Type: {ErrorType}", error.Message, error.ErrorType);
            return BadRequest(error);
        }

        /// <summary>
        /// Returns 400 with a message for the client. The message is sent as is: never pass exception details.
        /// </summary>
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult RespondError(string message = "", object? content = null, int? errorCode = null)
        {
            var errorResponse = new ApiErrorResponse
            {
                Message = message,
                Content = content,
                ErrorType = errorCode.HasValue ? $"AppError{errorCode}" : null
            };

            return RespondError(errorResponse);
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult RespondSuccess(ApiSuccessResponse success)
        {
            return Ok(success);
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult RespondSuccess(string message = "", object? content = null, int? requestCode = null)
        {
            var successResponse = new ApiSuccessResponse
            {
                Message = message,
                Content = content ?? new { },
                RequestCode = requestCode
            };

            return RespondSuccess(successResponse);
        }

        // -------------------------
        // Query builders
        // -------------------------

        /// <summary>
        /// Applies filters, ordering, field projection and pagination (limit capped by MaxPageSize)
        /// to a queryable of views/DTOs. All properties of <typeparamref name="T"/> are filterable,
        /// so use it only with types that are safe to expose.
        /// </summary>
        [ApiExplorerSettings(IgnoreApi = true)]
        public static ApiPaginatedResponse BuildPaginatedResponse<T>(QueryParams queryParams, IQueryable<T> query) where T : class
        {
            var result = new GuidQueryHelper().Page(query, queryParams);

            return new ApiPaginatedResponse
            {
                Content = result.Content,
                Total = result.Total
            };
        }
    }
}
