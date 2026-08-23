using Application.Dtos.DigiKala;
using Application.Mediator;
using Application.Services.Implementaions;
using Application.Services.Implementations;
using Application.Services.Interfaces;
using Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Net;
using Web.BackgroundServices;

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

            #region Singleton Services  
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<ISmsQueue, SmsQueue>();
            services.AddSingleton<ICacheManager, CacheManager>();
            services.AddSingleton<DigiKalaSessionState>();
            services.AddSingleton<IBuyBoxStrategy, HighestRankBuyBoxStrategy>();
            services.AddSingleton<IDigiKalaProductParser, DigiKalaProductParser>();
            #endregion

            #region HttpClient Services
            // 4. HTTP Client with decompression handler
            services.AddHttpClient<IDigiKalaProductApiClient, DigiKalaProductApiClient>((sp, client) =>
            {
                client.BaseAddress = new Uri("https://api.digikala.com/");
                client.Timeout = TimeSpan.FromSeconds(15);

                // Configure default headers once per client
                client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/126.0.0.0 Safari/537.36");
                client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("fa-IR,fa;q=0.9,en-US;q=0.8,en;q=0.7");
                client.DefaultRequestHeaders.Accept.ParseAdd("application/json, text/plain, */*");
                client.DefaultRequestHeaders.Referrer = new Uri("https://www.digikala.com/");
                client.DefaultRequestHeaders.Add("X-Web-Client", "DesktopWeb");
            })
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                // Automatically handle gzip/deflate/brotli so we don't have to manually decode
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.Brotli
            });

            services.AddHttpClient<ITelegramService, TelegramService>();
            #endregion

            #region Scoped Services
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<IPermissionService, PermissionService>();
            services.AddScoped<IContactUsFormService, ContactUsFormService>();
            services.AddScoped<INewsLetterService, NewsLetterService>();
            services.AddScoped<ISentLetterService, SentLetterService>();
            services.AddScoped<ISendEmailService, SendEmailService>();
            services.AddScoped<IEmailDataService, EmailDataService>();
            services.AddScoped<IBlogService, BlogService>();
            services.AddScoped<IFaqService, FaqService>();
            services.AddScoped<ISocialMediaService, SocialMediaService>();
            services.AddScoped<IPaymentDataService, PaymentDataService>();
            services.AddScoped<IPaymentService, PaymentService>();
            services.AddScoped<IUsedFreePlanService, UsedFreePlanService>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<ISubscriptionPlanService, SubscriptionPlanService>();
            services.AddScoped<ITicketService, TicketService>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<ISiteMapService, SiteMapService>();
            services.AddScoped<ISmsPanelService, SmsPanelService>();
            services.AddScoped<ISendSmsService, SendSmsService>();
            services.AddScoped<IMediatorHandler, MediatorHandler>();
            services.AddScoped<IMediumService, MediumService>();
            services.AddScoped<IVisitorService, VisitorService>();
            services.AddScoped<ISiteNotificationService, SiteNotificationService>();
            services.AddScoped<IDigiKalaService, DigiKalaService>();
            #endregion

            #region Background Services
            services.AddHostedService<PriceMonitorWorker>();
            services.AddHostedService<SmsConsumerWorker>();
            #endregion
        }
    }
}