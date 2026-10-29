using ApiBase.Domain.Interfaces;
using ApiBase.Infra.Options;
using ApiBase.Infra.UnitOfWork;
using ApiBase.Repository.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ApiBase.Repository.Extensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers the ApiBase core services: your DbContext, the generic repository,
        /// the unit of work and the query options.
        /// </summary>
        /// <typeparam name="TContext">Your DbContext (usually inheriting <c>ApiBase.Repository.Contexts.Context</c>).</typeparam>
        /// <param name="services">The service collection.</param>
        /// <param name="dbContextConfig">
        /// DbContext configuration. The consuming application installs and configures its own
        /// database provider package. Pass null when the DbContext is already registered
        /// (e.g. by your own infrastructure layer) — only the ApiBase services are added then.
        /// </param>
        /// <param name="configureOptions">Optional query options (page size limits).</param>
        /// <example>
        /// // SQL Server (requires Microsoft.EntityFrameworkCore.SqlServer):
        /// services.AddApiBaseCore&lt;AppDbContext&gt;(opt =&gt; opt.UseSqlServer(connectionString));
        ///
        /// // DbContext already registered elsewhere, custom page size:
        /// services.AddApiBaseCore&lt;AppDbContext&gt;(configureOptions: o =&gt; o.MaxPageSize = 50);
        /// </example>
        public static IServiceCollection AddApiBaseCore<TContext>(
            this IServiceCollection services,
            Action<DbContextOptionsBuilder>? dbContextConfig = null,
            Action<ApiBaseOptions>? configureOptions = null)
            where TContext : DbContext
        {
            if (dbContextConfig != null)
            {
                services.AddDbContext<TContext>(dbContextConfig);
            }

            // Repositories and the unit of work ask for DbContext: both must receive the same
            // scoped TContext instance, otherwise the commit would not see what the repository added.
            services.TryAddScoped<DbContext>(sp => sp.GetRequiredService<TContext>());

            services.TryAddScoped(typeof(IRepositoryBase<>), typeof(EfRepository<>));
            services.TryAddScoped<IUnitOfWork, UnitOfWork>();

            var optionsBuilder = services.AddOptions<ApiBaseOptions>();

            if (configureOptions != null)
            {
                optionsBuilder.Configure(configureOptions);
            }

            return services;
        }
    }
}
