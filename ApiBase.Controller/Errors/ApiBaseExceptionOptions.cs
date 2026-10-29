using ApiBase.Domain.Exceptions;
using Microsoft.AspNetCore.Http;

namespace ApiBase.Controller.Errors
{
    /// <summary>
    /// Maps exception types to HTTP status codes for <see cref="ApiBaseExceptionHandler"/>.
    /// Defaults: <see cref="InvalidQueryException"/> → 400 and <see cref="NotFoundException"/> → 404.
    /// Map your own domain exceptions so the API returns their message with the right status.
    /// </summary>
    /// <example>
    /// builder.Services.AddApiBaseExceptionHandler(o =&gt;
    /// {
    ///     o.Map&lt;DomainException&gt;(StatusCodes.Status400BadRequest);
    ///     o.Map&lt;ConflictException&gt;(StatusCodes.Status409Conflict);
    /// });
    /// </example>
    public class ApiBaseExceptionOptions
    {
        private readonly Dictionary<Type, int> _statusCodes = new()
        {
            [typeof(InvalidQueryException)] = StatusCodes.Status400BadRequest,
            [typeof(NotFoundException)] = StatusCodes.Status404NotFound
        };

        /// <summary>
        /// Maps <typeparamref name="TException"/> (and its subclasses) to <paramref name="statusCode"/>.
        /// Only map exceptions whose messages are safe to show to the client.
        /// </summary>
        public ApiBaseExceptionOptions Map<TException>(int statusCode) where TException : Exception
        {
            _statusCodes[typeof(TException)] = statusCode;
            return this;
        }

        /// <summary>
        /// Returns the status code of the closest mapped type in the exception's hierarchy, or null when not mapped.
        /// </summary>
        public int? GetStatusCode(Exception exception)
        {
            for (Type? type = exception.GetType(); type != null && type != typeof(object); type = type.BaseType)
            {
                if (_statusCodes.TryGetValue(type, out int status))
                {
                    return status;
                }
            }

            return null;
        }
    }
}
