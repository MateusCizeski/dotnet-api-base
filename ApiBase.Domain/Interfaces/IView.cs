namespace ApiBase.Domain.Interfaces
{
    /// <summary>
    /// Marker interface for view/DTO types used in query projections.
    /// Implement this interface on any class that represents a projected result
    /// to enable use with generic type constraints in the framework.
    /// </summary>
    public interface IView { }
}
