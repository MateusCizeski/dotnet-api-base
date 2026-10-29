using ApiBase.Domain.Query;
using ApiBase.Domain.View;
using ApiBase.Infra.Query;

namespace ApiBase.Application.ApplicationGuid
{
    /// <summary>
    /// Read-side application service contract for entities with a GUID primary key.
    /// Provides read operations with pagination, filtering, field projection and dynamic sorting.
    /// Writes (commands) are not part of this contract: they go through your own services
    /// and the domain model.
    /// </summary>
    /// <typeparam name="TView">The view/DTO type returned by queries.</typeparam>
    public interface IApplicationGuid<TView> where TView : IdGuidView, new()
    {
        // -------------------------
        // Read (sync)
        // -------------------------

        /// <summary>
        /// Returns a paginated, filtered and sorted list of records.
        /// </summary>
        GetView Get(QueryParams queryParams);

        /// <summary>
        /// Returns a single record by Id, or null when it does not exist.
        /// </summary>
        TView? Get(Guid id);

        /// <summary>
        /// Returns a single record by Id projected to the specified fields only, or null when it does not exist.
        /// </summary>
        object? Get(Guid id, List<string> fields);

        // -------------------------
        // Read (async)
        // -------------------------

        /// <summary>
        /// Asynchronously returns a paginated, filtered and sorted list of records.
        /// </summary>
        Task<GetView> GetAsync(QueryParams queryParams, CancellationToken cancellationToken = default);

        /// <summary>
        /// Asynchronously returns a single record by Id, or null when it does not exist.
        /// </summary>
        Task<TView?> GetAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Asynchronously returns a single record by Id projected to the specified fields only,
        /// or null when it does not exist.
        /// </summary>
        Task<object?> GetAsync(Guid id, List<string> fields, CancellationToken cancellationToken = default);
    }
}
