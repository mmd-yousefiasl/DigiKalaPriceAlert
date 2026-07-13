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
            #endregion

            #region HttpClient Services
            services.AddHttpClient<IDigiKalaService, DigiKalaService>()
                .ConfigurePrimaryHttpMessageHandler(() =>
                {
                    return new HttpClientHandler
                    {
                        UseCookies = true,
                        CookieContainer = new CookieContainer(),
                        //Proxy = new WebProxy("socks5://127.0.0.1:10808"),
                        //UseProxy = false,
                        AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
                    };
                });
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
            #endregion

            #region Background Services
            services.AddHostedService<PriceMonitorWorker>();
            services.AddHostedService<SmsConsumerWorker>();
            #endregion
        }
    }
}