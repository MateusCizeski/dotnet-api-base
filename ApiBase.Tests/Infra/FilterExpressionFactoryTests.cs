using ApiBase.Domain.Enums;
using ApiBase.Domain.Query;
using ApiBase.Infra.Extensions;
using ApiBase.Tests.Fixtures;
using System.Linq.Expressions;

namespace ApiBase.Tests.Infra
{
    public class FilterExpressionFactoryTests
    {
        private static List<ProductEntity> ApplyFilter(string propertyName, object value, FilterOperator op, bool not = false)
        {
            var query = ProductFixture.GetQueryable();
            var parameter = Expression.Parameter(typeof(ProductEntity), "x");
            var property = typeof(ProductEntity).GetProperty(propertyName)!;
            var memberExpr = Expression.Property(parameter, property);

            var filter = new FilterModel { Property = propertyName, Value = value, Operator = op, Not = not };
            var converted = ValueConverter.Convert(filter, property, memberExpr);

            if (converted == null) return query.ToList();

            var right = Expression.Constant(converted, property.PropertyType);
            var condition = FilterExpressionFactory.Create(filter, property, memberExpr, right, converted, query);

            var lambda = Expression.Lambda<Func<ProductEntity, bool>>(condition, parameter);
            return query.Where(lambda).ToList();
        }

        [Fact]
        public void Equal_ReturnsMatchingRecord()
        {
            var result = ApplyFilter(nameof(ProductEntity.Name), "Apple", FilterOperator.Equal);

            Assert.Single(result);
            Assert.Equal("Apple", result[0].Name);
        }

        [Fact]
        public void NotEqual_ExcludesMatchingRecord()
        {
            var result = ApplyFilter(nameof(ProductEntity.Name), "Apple", FilterOperator.NotEqual);

            Assert.DoesNotContain(result, p => p.Name == "Apple");
        }

        [Fact]
        public void Contains_CaseInsensitive_SingleMatch()
        {
            var result = ApplyFilter(nameof(ProductEntity.Name), "ARR", FilterOperator.Contains);

            Assert.Single(result);
            Assert.Equal("Carrot", result[0].Name);
        }

        [Fact]
        public void Contains_ReturnsMultipleMatches()
        {
            var result = ApplyFilter(nameof(ProductEntity.Name), "A", FilterOperator.Contains);

            Assert.Contains(result, p => p.Name == "Apple");
            Assert.Contains(result, p => p.Name == "Banana");
            Assert.True(result.Count >= 2);
        }

        [Fact]
        public void StartsWith_ReturnsMatchingRecords()
        {
            var result = ApplyFilter(nameof(ProductEntity.Name), "ba", FilterOperator.StartsWith);

            Assert.Single(result);
            Assert.Equal("Banana", result[0].Name);
        }

        [Fact]
        public void EndsWith_ReturnsMatchingRecords()
        {
            var result = ApplyFilter(nameof(ProductEntity.Name), "ot", FilterOperator.EndsWith);

            Assert.Single(result);
            Assert.Equal("Carrot", result[0].Name);
        }

        [Fact]
        public void GreaterThan_ReturnsCorrectRecords()
        {
            var result = ApplyFilter(nameof(ProductEntity.Price), "1.00", FilterOperator.GreaterThan);

            Assert.NotEmpty(result);
            Assert.All(result, p => Assert.True(p.Price > 1.00m));
        }

        [Fact]
        public void LessThan_ReturnsCorrectRecords()
        {
            var result = ApplyFilter(nameof(ProductEntity.Price), "1.00", FilterOperator.LessThan);

            Assert.NotEmpty(result);
            Assert.All(result, p => Assert.True(p.Price < 1.00m));
        }

        [Fact]
        public void GreaterThanOrEqual_IncludesBoundary()
        {
            var result = ApplyFilter(nameof(ProductEntity.Price), "1.50", FilterOperator.GreaterThanOrEqual);

            Assert.NotEmpty(result);
            Assert.All(result, p => Assert.True(p.Price >= 1.50m));
            Assert.Contains(result, p => p.Price == 1.50m);
        }

        [Fact]
        public void LessThanOrEqual_IncludesBoundary()
        {
            var result = ApplyFilter(nameof(ProductEntity.Price), "0.75", FilterOperator.LessThanOrEqual);

            Assert.NotEmpty(result);
            Assert.All(result, p => Assert.True(p.Price <= 0.75m));
            Assert.Contains(result, p => p.Price == 0.75m);
        }

        [Fact]
        public void In_ReturnsOnlyMatchingRecords()
        {
            var query = ProductFixture.GetQueryable();
            var parameter = Expression.Parameter(typeof(ProductEntity), "x");
            var property = typeof(ProductEntity).GetProperty(nameof(ProductEntity.Name))!;
            var memberExpr = Expression.Property(parameter, property);

            var value = new List<string> { "Apple", "Banana" };
            var filter = new FilterModel { Property = nameof(ProductEntity.Name), Value = value, Operator = FilterOperator.In };
            var right = Expression.Constant(value, value.GetType());

            var condition = FilterExpressionFactory.Create(filter, property, memberExpr, right, value, query);
            var lambda = Expression.Lambda<Func<ProductEntity, bool>>(condition, parameter);
            var result = query.Where(lambda).ToList();

            Assert.Equal(2, result.Count);
            Assert.All(result, p => Assert.Contains(p.Name, new[] { "Apple", "Banana" }));
        }

        [Fact]
        public void InOrNull_IncludesNullAndMatchingRecords()
        {
            var query = ProductFixture.GetQueryable();
            var parameter = Expression.Parameter(typeof(ProductEntity), "x");
            var property = typeof(ProductEntity).GetProperty(nameof(ProductEntity.Tag))!;
            var memberExpr = Expression.Property(parameter, property);

            var value = new List<string> { "organic" };
            var filter = new FilterModel { Property = nameof(ProductEntity.Tag), Value = value, Operator = FilterOperator.InOrNull };
            var right = Expression.Constant(value, value.GetType());

            var condition = FilterExpressionFactory.Create(filter, property, memberExpr, right, value, query);
            var lambda = Expression.Lambda<Func<ProductEntity, bool>>(condition, parameter);
            var result = query.Where(lambda).ToList();

            Assert.Contains(result, p => p.Tag == "organic");
            Assert.Contains(result, p => p.Tag == null);
        }

        [Fact]
        public void ContainsAll_AllTermsMustMatch()
        {
            var query = ProductFixture.GetQueryable();
            var parameter = Expression.Parameter(typeof(ProductEntity), "x");
            var property = typeof(ProductEntity).GetProperty(nameof(ProductEntity.Name))!;
            var memberExpr = Expression.Property(parameter, property);

            var value = new List<string> { "an", "na" };
            var filter = new FilterModel { Property = nameof(ProductEntity.Name), Value = value, Operator = FilterOperator.ContainsAll };
            var right = Expression.Constant(value, value.GetType());

            var condition = FilterExpressionFactory.Create(filter, property, memberExpr, right, value, query);
            var lambda = Expression.Lambda<Func<ProductEntity, bool>>(condition, parameter);
            var result = query.Where(lambda).ToList();

            Assert.Single(result);
            Assert.Equal("Banana", result[0].Name);
        }
    }
}
