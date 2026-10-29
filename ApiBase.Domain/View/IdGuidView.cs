using ApiBase.Domain.Interfaces;

namespace ApiBase.Domain.View
{
    /// <summary>
    /// Base read model (view) for entities with a GUID primary key.
    /// Views are what the API returns: only the properties declared here are exposed,
    /// and only these properties can be used in filter and sort expressions.
    /// The view does not inherit from the entity — the read model stays separate from the domain model.
    /// </summary>
    /// <example>
    /// public class ProductView : IdGuidView
    /// {
    ///     public string Name { get; set; }
    ///     public decimal Price { get; set; }
    /// }
    /// </example>
    public class IdGuidView : IView
    {
        /// <summary>The identifier of the projected entity.</summary>
        public Guid Id { get; set; }
    }
}
