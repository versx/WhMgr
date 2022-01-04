namespace WhMgr.Web.Middleware
{
    using System;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;

    using WhMgr.Extensions;

    public class RequestsMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestsMiddleware> _logger;

        public RequestsMiddleware(RequestDelegate next, ILogger<RequestsMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            _logger.Information($"Path: {httpContext.Request.Path}");
            await _next(httpContext).ConfigureAwait(false);
        }
    }
}