namespace WhMgr.Web.Middleware
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Http;

    using WhMgr.Extensions;

    public class DiscordAuthMiddleware
    {
        private static readonly ICollection<string> IgnoreRequestPaths = new List<string>
        {
            "/",
            "/dashboard/login",
            "/dashboard/logout",
            "/auth/discord/login",
            "/auth/discord/callback",
            "/api/v1", // TODO: Fix
        };
        private readonly RequestDelegate _next;

        public DiscordAuthMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            if (!httpContext.Session.GetValue<bool>("is_valid")
                && !IgnoreRequestPaths.Contains(httpContext.Request.Path))
            {
                httpContext.Session.SetValue("last_redirect", httpContext.Request.Path.Value);
                httpContext.Response.Redirect("/auth/discord/login");
            }
            else
            {
                await _next(httpContext).ConfigureAwait(false);
            }
        }
    }
}