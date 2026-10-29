namespace ApiBase.Domain.Interfaces
{
    /// <summary>
    /// Marks an entity as soft-deletable.
    /// Entities implementing this interface are never physically removed from the database.
    /// Instead, <see cref="IsDeleted"/> is set to true and <see cref="DeletedAt"/> is recorded.
    /// A global EF Core query filter hides soft-deleted records from every query,
    /// including navigations loaded with Include. Use IgnoreQueryFilters() to see them.
    /// </summary>
    public interface ISoftDelete
    {
        /// <summary>
        /// Indicates whether this entity has been soft-deleted.
        /// </summary>
        bool IsDeleted { get; set; }

        /// <summary>
        /// The UTC timestamp when this entity was soft-deleted. Null if not deleted.
        /// </summary>
        DateTime? DeletedAt { get; set; }
    }
}
