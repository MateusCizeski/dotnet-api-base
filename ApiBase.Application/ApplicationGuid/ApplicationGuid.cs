using ApiBase.Application.Base;
using ApiBase.Domain.Entities;
using ApiBase.Domain.Interfaces;
using ApiBase.Domain.Query;
using ApiBase.Domain.View;
using ApiBase.Infra.Extensions;
using ApiBase.Infra.Helpers;
using ApiBase.Infra.Query;
using System.Linq.Expressions;
using System.Reflection;

namespace ApiBase.Application.ApplicationGuid
{
    public abstract class ApplicationGuid<TEntity, TRepository, TView> : ApplicationBase<TEntity, TRepository>, IApplicationGuid<TView>
        where TEntity : EntityGuid, new()
        where TRepository : IRepositoryBase<TEntity>
        where TView : IdGuidView, new()
    {
        public ApplicationGuid(IUnitOfWork unitOfWork, TRepository repository) : base(unitOfWork, repository) { }

        public virtual GetView Get(QueryParams queryParams)
        {
            IQueryable<TEntity> query = Repository.Get().Where(DefaultFilter());

            GetView result = new GuidQueryHelper().Page<TEntity, TView>(query, queryParams);

            result.Content = UnitOfWork.BuildCustomFieldsList<TEntity>(result.Content.Cast<object>().ToList());

            return result;
        }

        public virtual object Get(Guid id)
        {
            IQueryable<TEntity> query = Repository.Where(e => e.Id == id).Where(DefaultFilter());

            var view = query.Project().To<TView>().FirstOrDefault();

            if (view == null)
            {
                return new { };
            }

            var withCustomFields = UnitOfWork.BuildCustomFieldsList<TEntity>(new List<object> { view });
            var first = withCustomFields.FirstOrDefault();

            return first ?? new { };
        }

        public object Get(Guid id, List<string> fields)
        {
            IQueryable<TEntity> query = Repository.Where(e => e.Id == id).Where(DefaultFilter());

            if (fields == null || fields.Count == 0)
            {
                var view = query.Project().To<TView>().FirstOrDefault();
                
                if (view == null)
                {
                    return new { };
                }

                return view;
            }

            var propertyMap = typeof(TView)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => fields.Contains(p.Name))
                .ToDictionary(p => p.Name, p => p.PropertyType);

            var dynamicType = CustomTypeBuilder.CreateType(propertyMap);

            var result = query.Project().To(dynamicType).FirstOrDefault();

            if (result == null)
            {
                return new { };
            }

            return result;
        }

        public virtual Expression<Func<TEntity, bool>> DefaultFilter()
        {
            return (TEntity e) => true;
        }
    }
}
