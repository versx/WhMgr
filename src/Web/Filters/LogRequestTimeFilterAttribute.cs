namespace WhMgr.Web.Filters
{
    using System.Diagnostics;

    using Microsoft.AspNetCore.Http.Extensions;
    using Microsoft.AspNetCore.Mvc.Filters;

    using WhMgr.Web.Events;

    public class LogRequestTimeFilterAttribute : ActionFilterAttribute
    {
        private readonly Stopwatch _stopwatch = new();

        public override void OnActionExecuting(ActionExecutingContext context) => _stopwatch.Start();

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            _stopwatch.Stop();

            MinimalEventCounterSource.Logger.Request(
                context.HttpContext.Request.GetDisplayUrl(),
                _stopwatch.ElapsedMilliseconds
            );
        }
    }
}