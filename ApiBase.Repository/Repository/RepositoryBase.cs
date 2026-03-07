using ApiBase.Domain.Entities;
using ApiBase.Domain.Interfaces;
using ApiBase.Domain.Query;
using ApiBase.Infra.Query;
using ApiBase.Repository.Contexts;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ApiBase.Repository.Repositories
{
    /// <summary>
    /// Generic base repository providing standard CRUD operations over a DbContext.
    /// Inherit this class to get full query, insert, update and delete support for any EntityGuid.
    /// </summary>
    public abstract class RepositoryBase<T> : IRepositoryBase<T>, IDisposable where T : EntityGuid
    {
        protected readonly DbContext Db;
        protected readonly DbSet<T> DbSet;

        public RepositoryBase(Context context)
        {
            Db = context;
            DbSet = Db.Set<T>();
        }

        public void Dispose()
        {
            // DbContext lifecycle is managed by the DI container (registered as Scoped via AddDbContext).
            // Disposing it here would cause ObjectDisposedException in other repositories
            // sharing the same context within the same request scope.
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc/>
        public virtual void Insert(T entity)
        {
            if (entity.Id == Guid.Empty)
            {
                entity.Id = Guid.NewGuid();
            }

            DbSet.Add(entity);
        }

        /// <inheritdoc/>
        public virtual void Insert(List<T> entities)
        {
            foreach (var entity in entities)
            {
                if (entity.Id == Guid.Empty)
                {
                    entity.Id = Guid.NewGuid();
                }
            }

            DbSet.AddRange(entities);
        }

        /// <inheritdoc/>
        public virtual void Remove(T entity)
        {
            DbSet.Remove(entity);
        }

        /// <inheritdoc/>
        public virtual void Remove(Guid id)
        {
            var entity = DbSet.Find(id);
            if (entity != null)
            {
                Remove(entity);
            }
        }

        /// <inheritdoc/>
        public virtual void Remove(List<T> entities)
        {
            DbSet.RemoveRange(entities);
        }

        /// <inheritdoc/>
        public T GetById(Guid id, params string[] includes)
        {
            return Get(includes).FirstOrDefault(x => x.Id == id);
        }

        /// <inheritdoc/>
        public IQueryable<T> Get(params string[] includes)
        {
            if (includes.Length == 0)
            {
                return DbSet;
            }

            IQueryable<T> queryable = DbSet.AsQueryable();

            foreach (var navigationPropertyPath in includes)
            {
                queryable = queryable.Include(navigationPropertyPath);
            }

            return queryable;
        }

        /// <inheritdoc/>
        public virtual IQueryable<T> Get(QueryParams queryParams)
        {
            var filters = queryParams.GetFilters();
            var includes = queryParams.GetIncludes();
            var order = queryParams.GetSort() ?? DefaultOrder();

            return Get(filters, order, includes.ToArray());
        }

        /// <inheritdoc/>
        public IQueryable<T> Get(List<FilterModel> filters, List<SortModel> order, params string[] includes)
        {
            return Get(new List<FilterGroup> { new FilterGroup { Filters = filters } }, order, includes);
        }

        /// <inheritdoc/>
        public IQueryable<T> Get(List<FilterGroup> filters, List<SortModel> order, params string[] includes)
        {
            IQueryable<T> queryable = DbSet.AsQueryable();

            if (includes != null)
            {
                foreach (var navigationPropertyPath in includes)
                {
                    queryable = queryable.Include(navigationPropertyPath);
                }
            }

            return new QueryBuilder<T>().Build(queryable, filters, order);
        }

        /// <inheritdoc/>
        public IQueryable<T> Where(Expression<Func<T, bool>> expression)
        {
            return DbSet.Where(expression);
        }

        /// <inheritdoc/>
        public T FirstOrDefault()
        {
            return DbSet.FirstOrDefault();
        }

        private List<SortModel> DefaultOrder()
        {
            return new List<SortModel>
            {
                new SortModel { Property = "Id", Direction = "asc" }
            };
        }
    }
}
