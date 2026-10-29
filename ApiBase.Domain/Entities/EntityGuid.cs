using System.ComponentModel;

namespace ApiBase.Domain.Entities
{
    /// <summary>
    /// Base entity with a GUID primary key.
    /// The Id is generated in the constructor and can only be changed by the entity itself,
    /// so rich domain entities (protected constructor, private setters) work out of the box.
    /// To enable soft delete on a derived entity, implement <see cref="ApiBase.Domain.Interfaces.ISoftDelete"/>.
    /// </summary>
    /// <example>
    /// public class Product : EntityGuid
    /// {
    ///     public string Name { get; private set; }
    ///
    ///     protected Product() { } // used by EF Core
    ///
    ///     public Product(string name) => Name = name;
    /// }
    /// </example>
    public abstract class EntityGuid
    {
        /// <summary>The unique identifier for this entity.</summary>
        public Guid Id { get; protected set; }

        protected EntityGuid()
        {
            Id = Guid.NewGuid();
        }

        protected EntityGuid(Guid id)
        {
            Id = id == Guid.Empty ? Guid.NewGuid() : id;
        }

        public override string ToString() => Id.ToString();

        /// <summary>
        /// Returns the customization identifier defined via <see cref="DescriptionAttribute"/>,
        /// or null if not defined.
        /// </summary>
        public string? GetCustomizationIdentifier()
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
