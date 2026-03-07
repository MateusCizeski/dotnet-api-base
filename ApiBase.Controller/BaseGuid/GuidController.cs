using ApiBase.Application.ApplicationGuid;
using ApiBase.Controller.Base;
using ApiBase.Domain.DTOs;
using ApiBase.Domain.View;
using ApiBase.Infra.Query;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace ApiBase.Controller.BaseGuid
{
    public class GuidController<TApplication, TView> : BaseApiController<TApplication> where TApplication : IApplicationGuid<TView> where TView : IdGuidView, new()
    {
        protected GuidController(TApplication application) : base(application) { }

        [HttpGet]
        public virtual IActionResult GetList([FromQuery] QueryParams queryParams)
        {
            try
            {
                var result = Application.Get(queryParams);

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

        [HttpGet("{id}")]
        public virtual IActionResult GetById([FromRoute] Guid id, [FromQuery] QueryField queryField)
        {
            try
            {
                var fields = queryField.GetFields();

                object content = fields.Any() ? Application.Get(id, fields) : Application.Get(id);

                return Respond(HttpStatusCode.OK, content);
            }
            catch (Exception ex)
            {
                return RespondError(ex);
            }
        }
    }
}
