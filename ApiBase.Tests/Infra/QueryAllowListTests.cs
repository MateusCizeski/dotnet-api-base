using ApiBase.Domain.Exceptions;
using ApiBase.Domain.Query;
using ApiBase.Infra.Query;
using ApiBase.Sample.Application;

namespace ApiBase.Tests.Infra
{
    public class QueryAllowListTests
    {
        [Theory]
        [InlineData("Name")]
        [InlineData("price")]
        [InlineData("Category.Name")]
        [InlineData("category.id")]
        public void IsAllowed_PathInsideTheView_ReturnsTrue(string path)
        {
            Assert.True(QueryAllowList.IsAllowed(path, typeof(ProductView)));
        }

        [Theory]
        [InlineData("CostPrice")]            // on the entity, not on the view
        [InlineData("Category.Products")]    // on the Category entity, not on CategorySummaryView
        [InlineData("")]
        public void IsAllowed_PathOutsideTheView_ReturnsFalse(string path)
        {
            Assert.False(QueryAllowList.IsAllowed(path, typeof(ProductView)));
        }

        [Fact]
        public void Validate_NestedPathOutsideTheView_Throws()
        {
            var filters = new List<FilterGroup>
            {
                new() { Filters = [new FilterModel { Property = "Category.Products", Value = "x" }] }
            };

            Assert.Throws<InvalidQueryException>(() => QueryAllowList.Validate(filters, null, typeof(ProductView)));
        }
    }
}
