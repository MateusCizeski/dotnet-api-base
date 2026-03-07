namespace ApiBase.Infra.Attributes
{
    /// <summary>
    /// Marks a property as requiring complex projection logic during expression tree building.
    /// When applied, the projection pipeline will handle this property with a custom strategy
    /// instead of the default direct binding.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class ComplexProjectionAttribute : Attribute { }
}
