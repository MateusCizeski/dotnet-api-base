using System.ComponentModel;

namespace ApiBase.Domain.Entities
{
    public class EntityGuid
    {
        public Guid Id { get; set; }

        public override string ToString()
        {
            return Id.ToString();
        }

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
