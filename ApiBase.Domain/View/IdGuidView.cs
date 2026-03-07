using ApiBase.Domain.Entities;
using ApiBase.Domain.Interfaces;

namespace ApiBase.Domain.View
{
    /// <summary>
    /// Base view class for entities with a GUID primary key.
    /// All view/DTO types used with <see cref="ApiBase.Application.ApplicationGuid.ApplicationGuid{TEntity,TRepository,TView}"/>
    /// must inherit from this class.
    /// </summary>
    /// <example>
    /// public class ProductView : IdGuidView
    /// {
    ///     public string Name { get; set; }
    ///     public decimal Price { get; set; }
    /// }
    /// </example>
    public class IdGuidView : EntityGuid, IView { }
}
