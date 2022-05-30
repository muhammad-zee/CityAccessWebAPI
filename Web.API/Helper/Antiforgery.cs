using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Web.API.Helper
{
    internal class Antiforgery
    {
        private readonly RequestDelegate next;
        private readonly IAntiforgery antiforgery;
        public Antiforgery(RequestDelegate next, IAntiforgery antiforgery)
        {
            this.next = next;
            this.antiforgery = antiforgery;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            httpContext.Response.OnStarting((state) =>
            {
                var context = (HttpContext)state;
                var tokens = antiforgery.GetAndStoreTokens(httpContext);
                httpContext.Response.Cookies.Append("XSRF-TOKEN", tokens.RequestToken, new CookieOptions() { Path = "/", HttpOnly = false });
                return Task.CompletedTask;
            }, httpContext);

            await next(httpContext);
        }
    }

    public static class AntiforgeryExtensions
    {
        public static IApplicationBuilder UseAntiforgery(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<Antiforgery>();
        }
    }
}
