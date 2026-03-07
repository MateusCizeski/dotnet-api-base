using ApiBase.Domain.Entities;
using ApiBase.Domain.Interfaces;
using ApiBase.Domain.Query;
using ApiBase.Infra.Query;
using ApiBase.Repository.Contexts;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ApiBase.Repository.Repositorys
{
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
            GC.SuppressFinalize(this);
        }

        public virtual void Insert(T entity)
        {
            if (entity.Id == Guid.Empty)
            {
                entity.Id = Guid.NewGuid();
            }

            DbSet.Add(entity);
        }

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

        public virtual void Remove(T entity)
        {
            DbSet.Remove(entity);
        }

        public virtual void Remove(Guid id)
        {
            var entity = DbSet.Find(id);

            if (entity != null)
            {
                Remove(entity);
            }
        }

        public virtual void Remove(List<T> entities)
        {
            DbSet.RemoveRange(entities);
        }

        public T GetById(Guid id, params string[] includes)
        {
            return Get(includes).FirstOrDefault(x => x.Id == id);
        }

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

        public virtual IQueryable<T> Get(QueryParams queryParams)
        {
            var filters = queryParams.GetFilters();
            var includes = queryParams.GetIncludes();
            var order = queryParams.GetSort() ?? DefaultOrder();
            return Get(filters, order, includes.ToArray());
        }

        private List<SortModel> DefaultOrder()
        {
            return new List<SortModel>
            {
                new SortModel { Property = "Id", Direction = "asc" }
            };
        }

        public IQueryable<T> Get(List<FilterModel> filters, List<SortModel> order, params string[] includes)
        {
            return Get(new List<FilterGroup> { new FilterGroup { Filters = filters } }, order, includes);
        }

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

        public IQueryable<T> Where(Expression<Func<T, bool>> expression)
        {
            return DbSet.Where(expression);
        }

        public T FirstOrDefault()
        {
            return DbSet.FirstOrDefault();
        }
    }
}
