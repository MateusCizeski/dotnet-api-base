using ApiBase.Sample.Domain;

namespace ApiBase.Sample.Data
{
    /// <summary>Fictitious data for the sample and the integration tests.</summary>
    public static class SampleData
    {
        public static void Seed(SampleDbContext db)
        {
            if (db.Products.Any())
            {
                return;
            }

            var fruit = new Category("Fruit");
            var vegetable = new Category("Vegetable");

            db.Products.AddRange(
                new Product("Apple", 1.50m, 0.90m, fruit),
                new Product("Banana", 0.75m, 0.40m, fruit),
                new Product("Cherry", 4.00m, 2.80m, fruit),
                new Product("Carrot", 0.50m, 0.20m, vegetable),
                new Product("Daikon", 1.20m, 0.70m, vegetable),
                new Product("Endive", 2.00m, 1.10m, vegetable));

            db.SaveChanges();
        }
    }
}
