using ApiBase.Controller.Errors;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace ApiBase.Controller.Extensions
{
    public static class ControllerServiceCollectionExtensions
    {
        /// <summary>
        /// Registers <see cref="ApiBaseExceptionHandler"/> and ProblemDetails.
        /// Also call <c>app.UseExceptionHandler()</c> in the pipeline.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configure">Optional mapping of your own exceptions to status codes.</param>
        public static IServiceCollection AddApiBaseExceptionHandler(this IServiceCollection services, Action<ApiBaseExceptionOptions>? configure = null)
        {
            var optionsBuilder = services.AddOptions<ApiBaseExceptionOptions>();

            if (configure != null)
            {
                optionsBuilder.Configure(configure);
            }

            services.AddProblemDetails();
            services.AddExceptionHandler<ApiBaseExceptionHandler>();

            return services;
        }

        /// <summary>
        /// Registers JWT Bearer authentication using the "Jwt" section of the configuration.
        /// Only registers if the section exists and contains a non-empty Secret.
        /// Keep the secret out of appsettings in source control: use User Secrets in development
        /// and an environment variable (Jwt__Secret) or a secret vault in production.
        /// </summary>
        /// <exception cref="InvalidOperationException">The secret is shorter than 32 bytes (HS256 needs 256 bits).</exception>
        /// <example>
        /// Configuration:
        /// {
        ///   "Jwt": {
        ///     "Secret": "(from user secrets / environment)",
        ///     "Issuer":  "your-issuer",
        ///     "Audience":"your-audience"
        ///   }
        /// }
        /// Program.cs:
        /// builder.Services.AddApiBaseJwt(builder.Configuration);
        /// </example>
        public static IServiceCollection AddApiBaseJwt(this IServiceCollection services, IConfiguration config)
        {
            var jwtConfig = config.GetSection("Jwt");
            string? secret = jwtConfig["Secret"];

            if (!jwtConfig.Exists() || string.IsNullOrEmpty(secret))
            {
                return services;
            }

            byte[] key = Encoding.UTF8.GetBytes(secret);

            if (key.Length < 32)
            {
                throw new InvalidOperationException("Jwt:Secret must have at least 32 bytes (256 bits) for HS256.");
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
                        IssuerSigningKey = new SymmetricSecurityKey(key)
                    };
                });

            return services;
        }
    }
}
