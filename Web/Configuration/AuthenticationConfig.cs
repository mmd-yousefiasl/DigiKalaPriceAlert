using Domain.Statics;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Web.Configuration
{
    public static class AuthenticationConfig
    {
        public static void ConfigAuthentication(this WebApplicationBuilder builder)
        {
            builder.Services.AddAntiforgery(options =>
            {
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.Strict;
            });

            builder.Services.AddAuthentication(option =>
            {
                option.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                option.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                option.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                option.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                option.DefaultSignOutScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            }).AddCookie(option =>
            {
                option.LoginPath = "/login";
                option.LogoutPath = "/logout";
                option.ExpireTimeSpan = TimeSpan.FromDays(SiteTools.LoginExpirationInDays);
                option.SlidingExpiration = true;
                option.Cookie.IsEssential = true;
                option.Cookie.HttpOnly = true;
                option.Cookie.SameSite = SameSiteMode.Lax;
                option.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            });
        }
    }
}
