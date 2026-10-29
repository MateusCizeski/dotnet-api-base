using ApiBase.Domain.View;

namespace ApiBase.Sample.Application
{
    /// <summary>
    /// What the API returns. Properties are matched by name with the entity; a nested view
    /// (Category) becomes a JOIN in the SQL. CostPrice is not here, so it is not exposed.
    /// </summary>
    public class ProductView : IdGuidView
    {
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public CategorySummaryView? Category { get; set; }
    }

    public class CategorySummaryView
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
