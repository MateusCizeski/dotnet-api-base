using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiBase.Domain.Interfaces
{
    /// <summary>
    /// Marks an entity as soft-deletable.
    /// Entities implementing this interface are never physically removed from the database.
    /// Instead, <see cref="IsDeleted"/> is set to true and <see cref="DeletedAt"/> is recorded.
    /// The base repository and application layer automatically filter out soft-deleted records.
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
