using ApiBase.Domain.Enums;
using ApiBase.Domain.Query;
using ApiBase.Infra.Extensions;
using ApiBase.Tests.Fixtures;
using System.Reflection;

namespace ApiBase.Tests.Infra
{
    public class ValueConverterTests
    {
        private static (FilterModel filter, PropertyInfo property) Setup<TEntity>(string propertyName, object? value, FilterOperator op = FilterOperator.Equal)
        {
            var filter = new FilterModel { Property = propertyName, Value = value, Operator = op };
            var property = typeof(TEntity).GetProperty(propertyName)!;

            return (filter, property);
        }

        [Fact]
        public void Convert_StringValue_ReturnsString()
        {
            var (filter, property) = Setup<ProductEntity>(nameof(ProductEntity.Name), "Apple");
            var result = ValueConverter.Convert(filter, property, null);

            Assert.Equal("Apple", result);
        }

        [Fact]
        public void Convert_DecimalValue_ReturnsDecimal()
        {
            var (filter, property) = Setup<ProductEntity>(nameof(ProductEntity.Price), "1.50");
            var result = ValueConverter.Convert(filter, property, null);

            Assert.IsType<decimal>(result);
            Assert.Equal(1.50m, (decimal)result!);
        }

        [Fact]
        public void Convert_IntValue_ReturnsInt()
        {
            var (filter, property) = Setup<ProductEntity>(nameof(ProductEntity.Stock), "100");
            var result = ValueConverter.Convert(filter, property, null);

            Assert.IsType<int>(result);
            Assert.Equal(100, (int)result!);
        }

        [Fact]
        public void Convert_BoolValue_ReturnsBool()
        {
            var (filter, property) = Setup<ProductEntity>(nameof(ProductEntity.Active), "true");
            var result = ValueConverter.Convert(filter, property, null);

            Assert.IsType<bool>(result);
            Assert.True((bool)result!);
        }

        [Fact]
        public void Convert_GuidValue_ReturnsGuid()
        {
            var guid = Guid.NewGuid();
            var (filter, property) = Setup<ProductEntity>(nameof(ProductEntity.Id), guid.ToString());
            var result = ValueConverter.Convert(filter, property, null);

            Assert.IsType<Guid>(result);
            Assert.Equal(guid, (Guid)result!);
        }

        [Fact]
        public void Convert_DateTimeValue_ReturnsDateTime()
        {
            var (filter, property) = Setup<ProductEntity>(nameof(ProductEntity.CreatedAt), "2024-01-01");
            var result = ValueConverter.Convert(filter, property, null);

            Assert.IsType<DateTime>(result);
            Assert.Equal(new DateTime(2024, 1, 1), (DateTime)result!);
        }

        [Fact]
        public void Convert_NullValue_ReturnsNull()
        {
            var (filter, property) = Setup<ProductEntity>(nameof(ProductEntity.Name), null);
            var result = ValueConverter.Convert(filter, property, null);

            Assert.Null(result);
        }

        [Fact]
        public void Convert_InvalidGuid_ReturnsNull()
        {
            var (filter, property) = Setup<ProductEntity>(nameof(ProductEntity.Id), "not-a-guid");
            var result = ValueConverter.Convert(filter, property, null);

            Assert.Null(result);
        }

        [Fact]
        public void Convert_InvalidDecimal_ReturnsNull()
        {
            var (filter, property) = Setup<ProductEntity>(nameof(ProductEntity.Price), "not-a-number");
            var result = ValueConverter.Convert(filter, property, null);

            Assert.Null(result);
        }

        [Fact]
        public void Convert_ListOfGuids_ReturnsGuidList()
        {
            var g1 = Guid.NewGuid();
            var g2 = Guid.NewGuid();
            var (filter, property) = Setup<ProductEntity>(nameof(ProductEntity.Id), new List<object> { g1.ToString(), g2.ToString() }, FilterOperator.In);
            var result = ValueConverter.Convert(filter, property, null);
            var list = Assert.IsType<List<Guid>>(result);

            Assert.Contains(g1, list);
            Assert.Contains(g2, list);
        }

        [Fact]
        public void Convert_ListOfStrings_ReturnsStringList()
        {
            var (filter, property) = Setup<ProductEntity>(nameof(ProductEntity.Name), new List<object> { "Apple", "Banana" }, FilterOperator.In);
            var result = ValueConverter.Convert(filter, property, null);
            var list = Assert.IsType<List<string>>(result);

            Assert.Equal(2, list.Count);
        }
    }
}
