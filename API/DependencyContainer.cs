using API.BackgroundServices;
using Application.Services.Implementaions;
using Application.Services.Interfaces;
using Data.Context;
using Microsoft.EntityFrameworkCore;

namespace API
{
    public class DependencyContainer
    {
        public static void RegisterDependencies(IServiceCollection services, string connectionString)
        {
            #region Config DataBase
            services.AddDbContext<DigiKalaPriceAlertDbContext>(options =>
            options.UseNpgsql(
                connectionString,
                npgsqlOptions =>
                {
                    npgsqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(5),
                        errorCodesToAdd: null);
                }
            ));
            #endregion

            services.AddHttpClient<IDigiKalaService, DigiKalaService>();
            services.AddTransient<ITelegramService, TelegramService>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ITelegramUpdateHandler, TelegramUpdateHandler>();

            services.AddSingleton<INotificationQueue, NotificationQueue>();

            services.AddHostedService<PriceMonitorWorker>();
            services.AddHostedService<NotificationConsumerWorker>();
        }
    }
}
