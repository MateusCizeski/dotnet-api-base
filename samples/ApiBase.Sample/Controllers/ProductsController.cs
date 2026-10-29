using ApiBase.Controller.BaseGuid;
using ApiBase.Sample.Application;
using Microsoft.AspNetCore.Mvc;

namespace ApiBase.Sample.Controllers
{
    /// <summary>
    /// GET /api/products and GET /api/products/{id} come from GuidController.
    /// </summary>
    [ApiController]
    [Route("api/products")]
    public class ProductsController : GuidController<IProductApplication, ProductView>
    {
        public ProductsController(IProductApplication application, ILogger<ProductsController> logger) : base(application, logger) { }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            await Application.DeleteAsync(id, cancellationToken);
            return NoContent();
        }
    }
}
