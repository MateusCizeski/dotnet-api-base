using ApiBase.Domain.Entities;

namespace ApiBase.Sample.Domain
{
    public class Category : EntityGuid
    {
        public string Name { get; private set; } = string.Empty;

        public List<Product> Products { get; private set; } = [];

        protected Category() { }

        public Category(string name)
        {
            Name = name;
        }
    }
}
