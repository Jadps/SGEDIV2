using Microsoft.AspNetCore.Antiforgery;
using Microsoft.Extensions.Options;
using BACKSGEDI.Configuration;

namespace BACKSGEDI.Infrastructure.Middleware;

public static class AntiforgeryMiddlewareExtensions
{
    public static IApplicationBuilder UseAntiforgeryTokenMiddleware(this IApplicationBuilder app)
    {
        return app.Use(async (context, next) =>
        {
            var antiforgery = context.RequestServices.GetRequiredService<IAntiforgery>();
            var appOptions = context.RequestServices.GetRequiredService<IOptions<AppOptions>>().Value;

            var tokens = antiforgery.GetAndStoreTokens(context);

            if (tokens.RequestToken != null)
            {
                context.Response.Cookies.Append(
                    "XSRF-TOKEN",
                    tokens.RequestToken,
                    new CookieOptions
                    {
                        HttpOnly = false,
                        Secure = true,
                        SameSite = SameSiteMode.None,
                        Domain = string.IsNullOrEmpty(appOptions.CookieDomain) ? null : appOptions.CookieDomain
                    });
            }

            await next();
        });
    }
}
