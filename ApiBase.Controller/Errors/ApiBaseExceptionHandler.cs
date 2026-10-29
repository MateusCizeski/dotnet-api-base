using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ApiBase.Controller.Errors
{
    /// <summary>
    /// Turns exceptions into RFC 7807 ProblemDetails responses.
    /// Mapped exceptions (see <see cref="ApiBaseExceptionOptions"/>) return their status code and message —
    /// those messages are written for the client. Anything else returns 500 with a generic message:
    /// the details (stack trace, SQL, table names) go only to the log.
    /// </summary>
    public class ApiBaseExceptionHandler : IExceptionHandler
    {
        private readonly IProblemDetailsService _problemDetailsService;
        private readonly ApiBaseExceptionOptions _options;
        private readonly ILogger<ApiBaseExceptionHandler> _logger;

        public ApiBaseExceptionHandler(IProblemDetailsService problemDetailsService, IOptions<ApiBaseExceptionOptions> options, ILogger<ApiBaseExceptionHandler> logger)
        {
            _problemDetailsService = problemDetailsService;
            _options = options.Value;
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            int? mappedStatus = _options.GetStatusCode(exception);
            int status = mappedStatus ?? StatusCodes.Status500InternalServerError;

            if (mappedStatus.HasValue)
            {
                _logger.LogWarning("Request failed with {Status}: {Message}", status, exception.Message);
            }
            else
            {
                _logger.LogError(exception, "Unhandled exception while processing {Method} {Path}.", httpContext.Request.Method, httpContext.Request.Path);
            }

            httpContext.Response.StatusCode = status;

            var problem = new ProblemDetails
            {
                Status = status,
                Title = mappedStatus.HasValue ? TitleFor(status) : "An unexpected error occurred.",
                Detail = mappedStatus.HasValue ? exception.Message : null
            };

            return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = httpContext,
                ProblemDetails = problem,
                Exception = exception
            });
        }

        private static string TitleFor(int status) => status switch
        {
            StatusCodes.Status400BadRequest => "The request is invalid.",
            StatusCodes.Status401Unauthorized => "Authentication is required.",
            StatusCodes.Status403Forbidden => "You do not have permission to perform this action.",
            StatusCodes.Status404NotFound => "The resource was not found.",
            StatusCodes.Status409Conflict => "The request conflicts with the current state of the resource.",
            StatusCodes.Status422UnprocessableEntity => "The request could not be processed.",
            _ => "The request failed."
        };
    }
}
