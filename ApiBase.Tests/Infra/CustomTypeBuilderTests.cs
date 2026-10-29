using ApiBase.Infra.Extensions;

namespace ApiBase.Tests.Infra
{
    public class CustomTypeBuilderTests
    {
        [Fact]
        public void CreateType_SamePropertiesInAnotherOrder_ReturnsTheSameType()
        {
            var first = CustomTypeBuilder.CreateType(new() { ["Name"] = typeof(string), ["Price"] = typeof(decimal) });
            var second = CustomTypeBuilder.CreateType(new() { ["Price"] = typeof(decimal), ["Name"] = typeof(string) });

            Assert.Same(first, second);
        }

        [Fact]
        public void CreateType_ConcurrentCalls_ReturnASingleType()
        {
            var types = new System.Collections.Concurrent.ConcurrentBag<Type>();

            Parallel.For(0, 50, _ =>
                types.Add(CustomTypeBuilder.CreateType(new() { ["Concurrent"] = typeof(int), ["Key"] = typeof(Guid) })));

            Assert.Single(types.Distinct());
        }
    }
}
