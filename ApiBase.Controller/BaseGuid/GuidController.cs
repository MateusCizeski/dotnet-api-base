using ApiBase.Application.ApplicationGuid;
using ApiBase.Controller.Base;
using ApiBase.Domain.DTOs;
using ApiBase.Domain.View;
using ApiBase.Infra.Query;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;

namespace ApiBase.Controller.BaseGuid
{
    /// <summary>
    /// Base CRUD controller for entities with a GUID primary key.
    /// Provides async GET list and GET by Id endpoints out of the box.
    /// Inherit and override to add POST, PUT, DELETE or custom endpoints.
    /// </summary>
    public class GuidController<TApplication, TView> : BaseApiController<TApplication> where TApplication : IApplicationGuid<TView> where TView : IdGuidView, new()
    {
        protected GuidController(TApplication application, ILogger logger) : base(application, logger) { }

        /// <summary>
        /// Returns a paginated, filtered and sorted list of records.
        /// </summary>
        [HttpGet]
        public virtual async Task<IActionResult> GetList([FromQuery] QueryParams queryParams)
        {
            try
            {
                Logger.LogDebug("GET list requested for {Controller}.", GetType().Name);

                var result = await Application.GetAsync(queryParams);

                return Respond(HttpStatusCode.OK, new ApiPaginatedResponse
                {
                    Content = result.Content,
                    Total = result.Total
                });
            }
            catch (Exception ex)
            {
                return RespondError(ex);
            }
        }

        /// <summary>
        /// Returns a single record by Id.
        /// Optionally projects to specific fields via the fields query parameter.
        /// </summary>
        [HttpGet("{id}")]
        public virtual async Task<IActionResult> GetById([FromRoute] Guid id, [FromQuery] QueryField queryField)
        {
            try
            {
                Logger.LogDebug("GET by Id requested for {Controller}. Id={Id}.", GetType().Name, id);

                var fields = queryField.GetFields();

                object content = fields.Any()
                    ? await Application.GetAsync(id, fields)
                    : await Application.GetAsync(id);

                return Respond(HttpStatusCode.OK, content);
            }
            catch (Exception ex)
            {
                return RespondError(ex);
            }
        }
    }
}
