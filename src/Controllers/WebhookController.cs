namespace WhMgr.Controllers
{
    using System.Collections.Generic;
    using System.Threading;

    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

    using WhMgr.Services.Subscriptions;
    using WhMgr.Services.Webhook;

    [ApiController]
    public class WebhookController : ControllerBase
    {
        private readonly ILogger<WebhookController> _logger;
        private readonly IWebhookProcessorService _webhookService;
        private readonly ISubscriptionProcessorService _subscriptionService;

        public WebhookController(
            ILogger<WebhookController> logger,
            IWebhookProcessorService webhookService,
            ISubscriptionProcessorService subscriptionService)
        {
            _logger = logger;
            _webhookService = webhookService;
            _subscriptionService = subscriptionService;
        }

        [HttpGet("/")]
        public IActionResult Get()
        {
            _logger.LogDebug($"Endpoint GET / hit");
            return Content("Webhook Manager v5 is running...");
        }

        [HttpPost("/")]
        public IActionResult HandleData(List<WebhookPayload> data)
        {
            ThreadPool.QueueUserWorkItem(x => _webhookService.ParseData(data));
            ThreadPool.QueueUserWorkItem(x => _subscriptionService.ParseData(data));
            return Ok();
        }
    }
}