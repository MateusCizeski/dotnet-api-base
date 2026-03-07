using ApiBase.Domain.Query;
using ApiBase.Domain.View;
using ApiBase.Infra.Query;

namespace ApiBase.Application.ApplicationGuid
{
    /// <summary>
    /// Application service contract for entities with a GUID primary key.
    /// Provides standard read operations with support for pagination, filtering,
    /// field projection and dynamic sorting.
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
        /// Returns a single record by Id, including all fields.
        /// Returns an empty object if not found.
        /// </summary>
        object Get(Guid id);

        /// <summary>
        /// Returns a single record by Id, projected to the specified fields only.
        /// Returns an empty object if not found.
        /// </summary>
        object Get(Guid id, List<string> fields);

        // -------------------------
        // Read (async)
        // -------------------------

        /// <summary>
        /// Asynchronously returns a paginated, filtered and sorted list of records.
        /// </summary>
        Task<GetView> GetAsync(QueryParams queryParams);

        /// <summary>
        /// Asynchronously returns a single record by Id, including all fields.
        /// Returns an empty object if not found.
        /// </summary>
        Task<object> GetAsync(Guid id);

        /// <summary>
        /// Asynchronously returns a single record by Id, projected to the specified fields only.
        /// Returns an empty object if not found.
        /// </summary>
        Task<object> GetAsync(Guid id, List<string> fields);
    }
}
