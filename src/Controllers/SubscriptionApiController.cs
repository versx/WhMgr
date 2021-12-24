namespace WhMgr.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json.Serialization;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

    using WhMgr.Extensions;
    using WhMgr.Services.Subscriptions;
    using WhMgr.Services.Subscriptions.Models;

    [ApiController]
    [Route("/api/v1/")]
    public class SubscriptionApiController
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

        [HttpGet("subscriptions")]
        public async Task<string> GetSubscriptions()
        {
            var subscriptions = await _subscriptionManager.GetUserSubscriptionsAsync();
            var response = new SubscriptionsResponse
            {
                Status = "OK",
                Subscriptions = subscriptions,
            };
            return response.ToJson();
        }
    }

    public class SubscriptionsResponse
    {
        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("subscriptions")]
        public List<Subscription> Subscriptions { get; set; }
    }
}