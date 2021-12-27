namespace WhMgr.Web.Middleware
{
    using System;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Antiforgery;
    using Microsoft.AspNetCore.Http;

    public class CsrfTokenMiddleware
    {
        private readonly RequestDelegate _next;

        public CsrfTokenMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var requestPath = context.Request.Path.Value;
            Console.WriteLine($"Middleware: {requestPath}");

            //var antiforgery = context.ApplicationServices.GetRequiredService<IAntiforgery>();
            var antiforgery = context.RequestServices.GetService(typeof(IAntiforgery));
            if (string.Equals(requestPath, "/", StringComparison.OrdinalIgnoreCase)
                || string.Equals(requestPath, "/dashboard", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine($"Context: {context.Request.Headers["X-CSRF-TOKEN-HEADERNAME"]}");
                /*
                var tokenSet = antiforgery.GetAndStoreTokens(context);
                Console.WriteLine($"csrf token: {tokenSet.RequestToken}");
                context.Response.Cookies.Append(
                    "XSRF-TOKEN",
                    tokenSet.RequestToken!,
                    new CookieOptions { HttpOnly = false }
                );
                */
            }
            // TODO: Set header with request token
            await _next(context);
        }
    }
}