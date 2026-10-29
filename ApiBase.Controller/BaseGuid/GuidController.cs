using ApiBase.Application.ApplicationGuid;
using ApiBase.Controller.Base;
using ApiBase.Domain.DTOs;
using ApiBase.Domain.View;
using ApiBase.Infra.Query;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ApiBase.Controller.BaseGuid
{
    /// <summary>
    /// Base read-only controller for entities with a GUID primary key.
    /// Provides async GET list and GET by Id endpoints out of the box.
    /// Inherit and add POST, PUT, DELETE or custom endpoints that call your own services.
    /// Errors are not caught here: register <c>AddApiBaseExceptionHandler</c> and
    /// <c>app.UseExceptionHandler()</c> to turn them into ProblemDetails.
    /// </summary>
    public class GuidController<TApplication, TView> : BaseApiController<TApplication> where TApplication : IApplicationGuid<TView> where TView : IdGuidView, new()
    {
        protected GuidController(TApplication application, ILogger logger) : base(application, logger) { }

        /// <summary>
        /// Returns a paginated, filtered and sorted list of records.
        /// </summary>
        /// <response code="200">The page and the total number of records that match the filters.</response>
        /// <response code="400">Invalid filter, sort or fields parameter.</response>
        [HttpGet]
        [ProducesResponseType(typeof(ApiPaginatedResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> GetList([FromQuery] QueryParams queryParams, CancellationToken cancellationToken)
        {
            Logger.LogDebug("GET list requested for {Controller}.", GetType().Name);

            var result = await Application.GetAsync(queryParams, cancellationToken);

            return Ok(new ApiPaginatedResponse
            {
                Content = result.Content,
                Total = result.Total
            });
        }

        /// <summary>
        /// Returns a single record by Id.
        /// Optionally projects to specific fields via the fields query parameter.
        /// </summary>
        /// <response code="200">The record.</response>
        /// <response code="404">No record with this Id (or not visible to the current user).</response>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public virtual async Task<IActionResult> GetById([FromRoute] Guid id, [FromQuery] QueryField queryField, CancellationToken cancellationToken)
        {
            Logger.LogDebug("GET by Id requested for {Controller}. Id={Id}.", GetType().Name, id);

            var fields = queryField.GetFields();

            object? content = fields.Count > 0
                ? await Application.GetAsync(id, fields, cancellationToken)
                : await Application.GetAsync(id, cancellationToken);

            if (content == null)
            {
                return NotFound();
            }

            return Ok(content);
        }
    }
}
