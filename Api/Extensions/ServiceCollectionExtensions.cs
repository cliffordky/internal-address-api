using dordle.common.models.authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace dordle.common.service.Extensions
{
    /// <summary>
    /// Extensions for the service collection to configre authentication and authorization.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        private const string SECURITY_DEFINITION = "Keycloak";
        private const string SECURITY_SCHEME = "Bearer";

        /// <summary>
        /// Adds SwaggerGen with client credentials authentication.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static IServiceCollection AddSwaggerGenWithClientCredentials(this IServiceCollection services, IConfiguration configuration, Action<SwaggerGenOptions> action = null)
        {
            var authenticationOptions = configuration.GetSection(AuthenticationOptions.ConfigKey).Get<AuthenticationOptions>()!;

            services.AddSwaggerGen(o =>
            {
                action?.Invoke(o);

                o.CustomSchemaIds(id => id.FullName!.Replace('+', '-'));

                o.AddSecurityDefinition(SECURITY_DEFINITION, new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.OAuth2,
                    Flows = new OpenApiOAuthFlows
                    {
                        ClientCredentials = new OpenApiOAuthFlow
                        {
                            //AuthorizationUrl = new Uri(authenticationOptions.AuthorizationUrl),
                            TokenUrl = new Uri(authenticationOptions.TokenUrl),
                            //Scopes = authenticationOptions.Scopes.Select(s => new KeyValuePair<string, string>(s, s)).ToDictionary()
                        }
                    }
                });

                var securityRequirement = new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Id = SECURITY_DEFINITION,
                                Type = ReferenceType.SecurityScheme
                            },
                            In = ParameterLocation.Header,
                            Name = SECURITY_SCHEME,
                            Scheme = SECURITY_SCHEME
                        },
                        []
                    }
                };

                o.AddSecurityRequirement(securityRequirement);
            });

            return services;
        }

        /// <summary>
        /// Adds client credentials authentication to the service collection.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static IServiceCollection AddClientCredentialsAuthentication(this IServiceCollection services, IConfiguration configuration, Action<IServiceCollection> action = null)
        {
            var authenticationOptions = configuration.GetSection(AuthenticationOptions.ConfigKey).Get<AuthenticationOptions>()!;

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(o =>
                {
                    o.RequireHttpsMetadata = true;
                    o.Audience = authenticationOptions.Audience;
                    o.MetadataAddress = authenticationOptions.MetadataAddress;
                    o.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidIssuer = authenticationOptions.ValidIssuer
                    };
                });

            //invoke the callback if configured
            action?.Invoke(services);

            return services;
        }

        /// <summary>
        /// Adds client credentials authorization to the service collection. Includes a read and write policy.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static IServiceCollection AddClientCredentialsAuthorization(this IServiceCollection services, IConfiguration configuration, Action<IServiceCollection> action = null)
        {
            var authenticationOptions = configuration.GetSection(AuthenticationOptions.ConfigKey).Get<AuthenticationOptions>()!;

            services.AddAuthorization(options =>
            {
                options.AddPolicy("read", policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireAssertion(context =>
                    {
                        var scopeClaim = context.User.FindFirst(claim => claim.Type == "scope");
                        if (scopeClaim != null)
                        {
                            return scopeClaim.Value.Split(' ').Any(s => s.Equals($"dordle:{authenticationOptions.ProtectedEntity}", StringComparison.OrdinalIgnoreCase));
                        }
                        return false;
                    });
                });
                options.AddPolicy("write", policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireAssertion(context =>
                    {
                        var scopeClaim = context.User.FindFirst(claim => claim.Type == "scope");
                        if (scopeClaim != null)
                        {
                            return scopeClaim.Value.Split(' ').Any(s => s.Equals($"dordle:{authenticationOptions.ProtectedEntity}:write", StringComparison.OrdinalIgnoreCase));
                        }
                        return false;
                    });
                });
            });

            //invoke the callback if configured
            action?.Invoke(services);

            return services;
        }
    }
}
