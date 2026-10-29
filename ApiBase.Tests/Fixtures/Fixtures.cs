using ApiBase.Domain.Entities;
using ApiBase.Domain.Interfaces;

namespace ApiBase.Tests.Fixtures
{
    public class ProductEntity : EntityGuid
    {
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string Category { get; set; } = string.Empty;
        public bool Active { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? Tag { get; set; }
    }

    public class SoftProductEntity : EntityGuid, ISoftDelete
    {
        public string Name { get; set; } = string.Empty;
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
    }

    public static class ProductFixture
    {
        public static IQueryable<ProductEntity> GetQueryable() => new List<ProductEntity>
        {
            new() { Name = "Apple",  Price = 1.50m,  Stock = 100, Category = "Fruit",     Active = true,  CreatedAt = new DateTime(2024, 1, 1), Tag = null },
            new() { Name = "Banana", Price = 0.75m,  Stock = 200, Category = "Fruit",     Active = true,  CreatedAt = new DateTime(2024, 2, 1), Tag = "organic" },
            new() { Name = "Carrot", Price = 0.50m,  Stock = 150, Category = "Vegetable", Active = false, CreatedAt = new DateTime(2024, 3, 1), Tag = "organic" },
            new() { Name = "Daikon", Price = 1.20m,  Stock = 50,  Category = "Vegetable", Active = true,  CreatedAt = new DateTime(2024, 4, 1), Tag = null },
            new() { Name = "Endive", Price = 2.00m,  Stock = 30,  Category = "Vegetable", Active = true,  CreatedAt = new DateTime(2024, 5, 1), Tag = "premium" },
        }.AsQueryable();
    }
}
