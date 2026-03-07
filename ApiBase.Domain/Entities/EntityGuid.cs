using System.ComponentModel;

namespace ApiBase.Domain.Entities
{
    /// <summary>
    /// Base entity with a GUID primary key.
    /// To enable soft delete on a derived entity, implement <see cref="ApiBase.Domain.Interfaces.ISoftDelete"/>.
    /// </summary>
    /// <example>
    /// public class Product : EntityGuid, ISoftDelete
    /// {
    ///     public string Name { get; set; }
    ///     public bool IsDeleted { get; set; }
    ///     public DateTime? DeletedAt { get; set; }
    /// }
    /// </example>
    public class EntityGuid
    {
        /// <summary>The unique identifier for this entity.</summary>
        public Guid Id { get; set; }

        public override string ToString() => Id.ToString();

        /// <summary>
        /// Returns the customization identifier defined via <see cref="DescriptionAttribute"/>,
        /// or null if not defined.
        /// </summary>
        public string GetCustomizationIdentifier()
        {
            object[] customAttributes = GetType().GetCustomAttributes(typeof(DescriptionAttribute), inherit: true);

            if (customAttributes.Length == 0)
            {
                return null;
            }

            return ((DescriptionAttribute)customAttributes[0]).Description;
        }
    }
}
