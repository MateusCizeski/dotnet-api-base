using ApiBase.Domain.Query;
using ApiBase.Infra.Query;
using ApiBase.Tests.Fixtures;

namespace ApiBase.Tests.Infra
{
    public class OrderByQueryTests
    {
        private static IQueryable<ProductEntity> Query => ProductFixture.GetQueryable();

        [Fact]
        public void ApplySorting_NullSortList_ReturnsOriginalQuery()
        {
            var result = new OrderByQuery().ApplySorting(Query, null).ToList();

            Assert.Equal(5, result.Count);
        }

        [Fact]
        public void ApplySorting_EmptySortList_ReturnsOriginalQuery()
        {
            var result = new OrderByQuery().ApplySorting(Query, new List<SortModel>()).ToList();

            Assert.Equal(5, result.Count);
        }

        [Fact]
        public void ApplySorting_ByNameAsc_ReturnsSortedAscending()
        {
            var sort = new List<SortModel> { new() { Property = "Name", Direction = "asc" } };
            var result = new OrderByQuery().ApplySorting(Query, sort).ToList();

            var names = result.Select(p => p.Name).ToList();
            Assert.Equal(names.OrderBy(n => n).ToList(), names);
        }

        [Fact]
        public void ApplySorting_ByNameDesc_ReturnsSortedDescending()
        {
            var sort = new List<SortModel> { new() { Property = "Name", Direction = "desc" } };
            var result = new OrderByQuery().ApplySorting(Query, sort).ToList();

            var names = result.Select(p => p.Name).ToList();
            Assert.Equal(names.OrderByDescending(n => n).ToList(), names);
        }

        [Fact]
        public void ApplySorting_ByPriceAsc_ReturnsSortedByPrice()
        {
            var sort = new List<SortModel> { new() { Property = "Price", Direction = "asc" } };
            var result = new OrderByQuery().ApplySorting(Query, sort).ToList();

            var prices = result.Select(p => p.Price).ToList();
            Assert.Equal(prices.OrderBy(p => p).ToList(), prices);
        }

        [Fact]
        public void ApplySorting_MultipleSorts_AppliesThenBy()
        {
            var sort = new List<SortModel>
            {
                new() { Property = "Category", Direction = "asc" },
                new() { Property = "Name",     Direction = "asc" }
            };

            var result = new OrderByQuery().ApplySorting(Query, sort).ToList();

            var fruits = result.Where(p => p.Category == "Fruit").Select(p => p.Name).ToList();
            Assert.Equal(fruits.OrderBy(n => n).ToList(), fruits);

            var vegetables = result.Where(p => p.Category == "Vegetable").Select(p => p.Name).ToList();
            Assert.Equal(vegetables.OrderBy(n => n).ToList(), vegetables);
        }

        [Fact]
        public void ApplySorting_ConditionalOrder_PinsValueToTop()
        {
            var sort = new List<SortModel>
            {
                new() { Property = "Category", Direction = "asc", FilterValue = "Vegetable" }
            };

            var result = new OrderByQuery().ApplySorting(Query, sort).ToList();

            var firstNonVegetableIndex = result.FindIndex(p => p.Category != "Vegetable");
            var lastVegetableIndex = result.FindLastIndex(p => p.Category == "Vegetable");

            Assert.True(lastVegetableIndex < firstNonVegetableIndex);
        }

        [Fact]
        public void ApplySorting_InvalidProperty_ThrowsArgumentException()
        {
            var sort = new List<SortModel> { new() { Property = "NonExistentProperty", Direction = "asc" } };

            Assert.Throws<ArgumentException>(() => new OrderByQuery().ApplySorting(Query, sort).ToList());
        }

        [Fact]
        public void ApplySorting_ByStockDesc_ReturnsCorrectOrder()
        {
            var sort = new List<SortModel> { new() { Property = "Stock", Direction = "desc" } };
            var result = new OrderByQuery().ApplySorting(Query, sort).ToList();

            var stocks = result.Select(p => p.Stock).ToList();
            Assert.Equal(stocks.OrderByDescending(s => s).ToList(), stocks);
        }
    }
}
