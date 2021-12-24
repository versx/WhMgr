namespace WhMgr.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json.Serialization;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

    using WhMgr.Services.Subscriptions;
    using WhMgr.Services.Subscriptions.Models;

    [ApiController]
    [Route("/api/v1/")]
    public class SubscriptionApiController : ControllerBase
    {
        private readonly ILogger<SubscriptionApiController> _logger;
        private readonly ISubscriptionManagerService _subscriptionManager;

        public SubscriptionApiController(
            ILogger<SubscriptionApiController> logger,
            ISubscriptionManagerService subscriptionManager)
        {
            _logger = logger;
            _subscriptionManager = subscriptionManager;
        }

        [
            HttpGet("subscriptions"),
            Produces("application/json"),
        ]
        public async Task<IActionResult> GetSubscriptions()
        {
            var subscriptions = await _subscriptionManager.GetUserSubscriptionsAsync().ConfigureAwait(false);
            var response = new SubscriptionsResponse<List<Subscription>>
            {
                Status = "OK",
                Data = subscriptions,
            };
            return new JsonResult(response);
        }

        [
            HttpGet("subscription/{guildId}/{userId}"),
            Produces("application/json"),
        ]
        public IActionResult GetSubscription(ulong guildId, ulong userId)
        {
            var subscription = _subscriptionManager.GetUserSubscriptions(guildId, userId);
            var response = new SubscriptionsResponse<Subscription>
            {
                Status = "OK",
                Data = subscription,
            };
            return new JsonResult(response);
        }
    }

    public class SubscriptionsResponse<T>
    {
        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("data")]
        public T Data { get; set; }
    }
}