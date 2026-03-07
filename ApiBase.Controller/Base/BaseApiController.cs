using ApiBase.Domain.DTOs;
using ApiBase.Domain.Query;
using ApiBase.Infra.Extensions;
using ApiBase.Infra.Projection;
using ApiBase.Infra.Query;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace ApiBase.Controller.Base
{
    public class BaseApiController<TApplication> : ControllerBase
    {
        protected TApplication Application { get; set; }

        protected BaseApiController(TApplication application)
        {
            Application = application;
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult Respond<TResponse>(HttpStatusCode statusCode, TResponse response)
        {
            return new ObjectResult(response) { StatusCode = (int)statusCode };
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet("ReturnErrorWithObject")]
        public IActionResult RespondError(ApiErrorResponse error)
        {
            return BadRequest(error);
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet("ReturnError")]
        public IActionResult RespondError(string message = "", object content = null, int? errorCode = null)
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
        [HttpGet("ReturnErrorFromException")]
        public IActionResult RespondError(Exception ex)
        {
            return RespondError(ex.FlattenMessage());
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet("ReturnSuccessWithObject")]
        public IActionResult RespondSuccess(ApiSuccessResponse success)
        {
            return Ok(success);
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet("ReturnSuccess")]
        public IActionResult RespondSuccess(string message = "", object content = null, int? requestCode = null)
        {
            var successResponse = new ApiSuccessResponse
            {
                Message = message,
                Content = content ?? new { },
                RequestCode = requestCode
            };

            return RespondSuccess(successResponse);
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        public static ApiPaginatedResponse BuildPaginatedResponse<T>(QueryParams queryParams, IQueryable<T> query) where T : class
        {
            var result = BuildQueryResponse(queryParams, query);

            if (result.Content is not IQueryable<object> contentQueryable)
            {
                throw new InvalidCastException("The content returned from BuildQueryResponse is not of type IQueryable<object>.");
            }

            var paged = Paginate(contentQueryable, queryParams.Page.GetValueOrDefault(), queryParams.Limit.GetValueOrDefault(25));

            return new ApiPaginatedResponse
            {
                Content = paged.AsQueryable(),
                Total = result.Total
            };
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        public static ApiPaginatedResponse BuildQueryResponse<T>(QueryParams queryParams, IQueryable<T> query) where T : class
        {
            query = ApplyOrdering(queryParams, query);

            IQueryable<object> resultQuery;
            var fields = queryParams.GetFields();

            if (fields.Count > 0)
            {
                var propertyDict = typeof(T)
                    .GetProperties()
                    .Where(p => fields.Contains(p.Name))
                    .ToDictionary(p => p.Name, p => p.PropertyType);

                var dynamicType = CustomTypeBuilder.CreateType(propertyDict);
                var selector = new ProjectionBuilder().Build<T>(dynamicType);
                resultQuery = query.Select(selector);
            }
            else
            {
                resultQuery = query.Cast<object>();
            }

            return new ApiPaginatedResponse
            {
                Content = resultQuery,
                Total = resultQuery.Count()
            };
        }

        private static IQueryable<T> ApplyOrdering<T>(QueryParams queryParams, IQueryable<T> query)
        {
            var orderList = queryParams.GetSort() ?? new List<SortModel>
            {
                new SortModel { FilterValue = "Id", Direction = "asc" }
            };

            return new OrderByQuery().ApplySorting(query, orderList);
        }

        private static List<object> Paginate(IQueryable<object> query, int page, int limit)
        {
            return query.Skip((page - 1) * limit).Take(limit).ToList();
        }
    }
}
