using Application.Services.Implementaions;
using Application.Services.Interfaces;
using Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Web
{
    public class DependencyContainer
    {
        public static void RegisterDependencies(IServiceCollection services, string connectionString)
        {
            #region Config DataBase
            services.AddDbContextPool<DigiKalaPriceAlertDbContext>(options =>
            {
                options.UseNpgsql(
                    connectionString,
                    npgsqlOptions =>
                    {
                        npgsqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 5,
                            maxRetryDelay: TimeSpan.FromSeconds(5),
                            errorCodesToAdd: null);
                    }
                );

                options.ConfigureWarnings(warnings =>
                    warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
            });
            #endregion

            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IUserValidationService, UserValidationService>();
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<IPermissionService, PermissionService>();
        }
    }
}
