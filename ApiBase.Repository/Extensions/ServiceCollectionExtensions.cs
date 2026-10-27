using ApiBase.Domain.Interfaces;
using ApiBase.Infra.UnitOfWork;
using ApiBase.Repository.Contexts;
using ApiBase.Repository.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace ApiBase.Repository.Extensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers the ApiBase core services: DbContext, generic repositories, UnitOfWork and logging.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="dbContextConfig">
        /// DbContext configuration action. The consuming application must install and
        /// configure its own database provider package.
        /// </param>
        /// <example>
        /// // SQL Server (requires Microsoft.EntityFrameworkCore.SqlServer):
        /// services.AddApiBaseCore(opt => opt.UseSqlServer(connectionString));
        ///
        /// // PostgreSQL (requires Npgsql.EntityFrameworkCore.PostgreSQL):
        /// services.AddApiBaseCore(opt => opt.UseNpgsql(connectionString));
        /// </example>
        public static IServiceCollection AddApiBaseCore(this IServiceCollection services, Action<DbContextOptionsBuilder> dbContextConfig)
        {
            ArgumentNullException.ThrowIfNull(dbContextConfig);

            services.AddDbContext<Context>(dbContextConfig);

            services.AddScoped(typeof(IRepositoryBase<>), typeof(RepositoryBase<>));
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddHttpContextAccessor();
            services.AddHealthChecks();

            return services;
        }

        /// <summary>
        /// Registers JWT Bearer authentication using the "Jwt" section from appsettings.
        /// Only registers if the section exists and contains a non-empty Secret.
        /// </summary>
        /// <example>
        /// appsettings.json:
        /// {
        ///   "Jwt": {
        ///     "Secret": "your-secret-key",
        ///     "Issuer":  "your-issuer",
        ///     "Audience":"your-audience"
        ///   }
        /// }
        /// Program.cs:
        /// services.AddApiBaseCore(opt => opt.UseSqlServer(conn))
        ///         .AddApiBaseJwt(config);
        /// </example>
        public static IServiceCollection AddApiBaseJwt(this IServiceCollection services, IConfiguration config)
        {
            var jwtConfig = config.GetSection("Jwt");

            if (!jwtConfig.Exists() || string.IsNullOrEmpty(jwtConfig["Secret"]))
            {
                return services;
            }

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = jwtConfig["Issuer"] != null,
                        ValidateAudience = jwtConfig["Audience"] != null,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtConfig["Issuer"],
                        ValidAudience = jwtConfig["Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(jwtConfig["Secret"]!))
                    };
                });

            return services;
        }
    }
}
