using API.BackgroundServices;
using Application.Services.Implementaions;
using Application.Services.Interfaces;
using Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace API
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

            services.AddHttpClient<IDigiKalaService, DigiKalaService>();
            services.AddTransient<ITelegramService, TelegramService>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ITelegramUpdateHandler, TelegramUpdateHandler>();
            services.AddScoped<ISubscriptionPlanService, SubscriptionPlanService>();

            services.AddSingleton<INotificationQueue, NotificationQueue>();
            services.AddSingleton<ISmsQueue, SmsQueue>();

            services.AddHostedService<PriceMonitorWorker>();
            services.AddHostedService<NotificationConsumerWorker>();
            services.AddHostedService<SmsConsumerWorker>();
        }
    }
}
