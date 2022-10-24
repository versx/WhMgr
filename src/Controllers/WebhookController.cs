namespace WhMgr.Controllers
{
    using System.Collections.Generic;
    using System.Threading;

    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

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
        public IActionResult Index()
        {
            return Content($"{Strings.BotName} {Strings.BotVersion} is running...");
        }

        [HttpPost("/")]
        public IActionResult HandleData(List<WebhookPayload> data)
        {
            if (!ThreadPool.QueueUserWorkItem(async _ => await _webhookService.ParseDataAsync(data)))
            {
                return Unauthorized();
            }
            return Ok();
        }
    }
}