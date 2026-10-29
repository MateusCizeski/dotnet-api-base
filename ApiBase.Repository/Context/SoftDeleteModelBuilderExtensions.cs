using ApiBase.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ApiBase.Repository.Contexts
{
    public static class SoftDeleteModelBuilderExtensions
    {
        /// <summary>
        /// Adds a global query filter <c>e =&gt; !e.IsDeleted</c> to every entity that implements
        /// <see cref="ISoftDelete"/>. Being global, it also applies to navigations loaded with Include
        /// and to queries written directly on the DbContext. Use <c>IgnoreQueryFilters()</c> to see deleted rows.
        /// <see cref="Context"/> calls it automatically; call it yourself if your DbContext does not inherit from Context.
        /// </summary>
        public static ModelBuilder ApplySoftDeleteQueryFilters(this ModelBuilder modelBuilder)
        {
            var softDeleteTypes = modelBuilder.Model.GetEntityTypes()
                .Where(t => typeof(ISoftDelete).IsAssignableFrom(t.ClrType) && t.BaseType == null);

            foreach (var entityType in softDeleteTypes)
            {
                // Builds: (TEntity e) => !e.IsDeleted
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var isDeleted = Expression.Property(parameter, nameof(ISoftDelete.IsDeleted));
                var filter = Expression.Lambda(Expression.Not(isDeleted), parameter);

                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);
            }

            return modelBuilder;
        }
    }
}
