using ApiBase.Domain.Entities;
using ApiBase.Domain.Interfaces;

namespace ApiBase.Sample.Domain
{
    /// <summary>
    /// Rich domain entity: protected constructor for EF Core, private setters, rules in methods.
    /// </summary>
    public class Product : EntityGuid, ISoftDelete
    {
        public string Name { get; private set; } = string.Empty;
        public decimal Price { get; private set; }

        /// <summary>Internal data: not in ProductView, so it can be neither returned nor filtered.</summary>
        public decimal CostPrice { get; private set; }

        public Guid CategoryId { get; private set; }
        public Category? Category { get; private set; }

        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }

        protected Product() { }

        public Product(string name, decimal price, decimal costPrice, Category category)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Name is required.", nameof(name));
            }

            if (price <= 0)
            {
                throw new ArgumentException("Price must be greater than zero.", nameof(price));
            }

            Name = name;
            Price = price;
            CostPrice = costPrice;
            CategoryId = category.Id;
            Category = category;
        }
    }
}
