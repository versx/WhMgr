namespace WhMgr.Web.Extensions
{
    using Microsoft.AspNetCore.Builder;

    using WhMgr.Web.Middleware;

    public static class RequestCultureMiddlewareExtensions
    {
        public static IApplicationBuilder UseCsrfTokens(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<CsrfTokenMiddleware>();
        }
    }
}