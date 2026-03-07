using ApiBase.Domain.Interfaces;
using ApiBase.Infra.UnitOfWork;
using ApiBase.Repository.Contexts;
using ApiBase.Repository.Repositorys;
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
        public static IServiceCollection AddApiBaseCore(this IServiceCollection services, Action<DbContextOptionsBuilder>? dbContextConfig = null)
        {
            if (dbContextConfig != null)
            {
                services.AddDbContext<Context>(dbContextConfig);
            }
            else
            {
                services.AddDbContext<Context>(options => options.UseInMemoryDatabase("DefaultDatabase"));
            }

            services.AddScoped(typeof(IRepositoryBase<>), typeof(RepositoryBase<>));
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddHttpContextAccessor();
            services.AddHealthChecks();

            return services;
        }

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
