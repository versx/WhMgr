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

        public WebhookController(
            ILogger<WebhookController> logger,
            IWebhookProcessorService webhookService)
        {
            _logger = logger;
            _webhookService = webhookService;
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
            return Ok();
        }
    }
}