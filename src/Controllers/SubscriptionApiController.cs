namespace WhMgr.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json.Serialization;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

    using WhMgr.Services.Subscriptions;
    using WhMgr.Services.Subscriptions.Models;
    using WhMgr.Web.Api.Requests.Pokemon;

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

        #region User Subscriptions

        [HttpGet("subscriptions")]
        [Produces("application/json")]
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

        [HttpGet("subscription/{guildId}/{userId}")]
        [Produces("application/json")]
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

        #endregion

        #region Pokemon Subscriptions

        [HttpGet("subscription/pokemon/{guildId}/{userId}")]
        [Produces("application/json")]
        public IActionResult GetPokemonSubscriptions(ulong guildId, ulong userId)
        {
            var subscription = _subscriptionManager.GetUserSubscriptions(guildId, userId);
            var response = new SubscriptionsResponse<List<PokemonSubscription>>
            {
                Status = subscription != null
                    ? "OK"
                    : "Error",
                Data = subscription.Pokemon.ToList(),
            };
            return new JsonResult(response);
        }

        [HttpGet("subscription/pokemon/{id}")]
        [Produces("application/json")]
        public async Task<IActionResult> GetPokemonSubscription(int id)
        {
            var subscription = await _subscriptionManager.FindByIdAsync<PokemonSubscription>(id);
            var response = new SubscriptionsResponse<PokemonSubscription>
            {
                Status = subscription != null
                    ? "OK"
                    : "Error",
                Data = subscription,
            };
            return new JsonResult(response);
        }

        [HttpPost("pokemon/create")]
        [Produces("application/json")]
        public async Task<IActionResult> PokemonCreate(PokemonSubscription pokemonSubscription)
        {
            if (pokemonSubscription == null)
            {
                return new JsonResult(new
                {
                    status = "Error",
                    message = "Failed to create Pokemon subscription, data was null.",
                });
            }

            //  Check if guild_id and user_id not equal to 0
            if (pokemonSubscription.GuildId == 0 || pokemonSubscription.UserId == 0)
            {
                return new JsonResult(new
                {
                    status = "Error",
                    message = "Both GuildId and UserId are required.",
                });
            }

            var subscription = _subscriptionManager.GetUserSubscriptions(pokemonSubscription.GuildId, pokemonSubscription.UserId);
            if (subscription == null)
            {
                // Subscription does not exist, create new
                subscription = new Subscription
                {
                    GuildId = pokemonSubscription.GuildId,
                    UserId = pokemonSubscription.UserId,
                };
            }
            subscription.Pokemon.Add(pokemonSubscription);
            var result = await _subscriptionManager.CreateSubscriptionAsync(subscription).ConfigureAwait(false);
            dynamic response = result
                ? new
                {
                    status = "OK",
                    message = "Successfully created Pokemon subscription.",
                    data = pokemonSubscription,
                }
                : new
                {
                    status = "Error",
                    message = "Failed to create Pokemon subscription.",
                };
            return new JsonResult(response);
        }

        [HttpPut("pokemon/update/{id}")]
        [Produces("application/json")]
        public async Task<IActionResult> PokemonUpdate(int id, PokemonSubscription pokemonSubscription)
        {
            if (pokemonSubscription == null)
            {
                return new JsonResult(new
                {
                    status = "Error",
                    message = "Failed to create Pokemon subscription, data was null.",
                });
            }

            //  Check if guild_id and user_id not equal to 0
            if (pokemonSubscription.GuildId == 0 || pokemonSubscription.UserId == 0)
            {
                return new JsonResult(new
                {
                    status = "Error",
                    message = "Both GuildId and UserId are required.",
                });
            }

            var subscription = await _subscriptionManager.FindByIdAsync<PokemonSubscription>(id);
            if (subscription == null)
            {
                // Subscription does not exist, create new
                return new JsonResult(new
                {
                    status = "Error",
                    message = $"Pokemon subscription with id {id} does not exist.",
                });
            }

            var result = await _subscriptionManager.UpdateSubscriptionAsync(id, pokemonSubscription);
            var response = result
                ? new
                {
                    status = "OK",
                    message = $"Successfully updated Pokemon subscription {id}.",
                }
                : new
                {
                    status = "Error",
                    message = $"Failed to update Pokemon subscription {id}.",
                };
            return new JsonResult(response);
        }

        [HttpDelete("pokemon/delete/{id}")]
        [Produces("application/json")]
        public async Task<IActionResult> PokemonDelete(int id)
        {
            var result = await _subscriptionManager.DeleteByIdAsync<PokemonSubscription>(id);
            var response = result
                ? new
                {
                    status = "OK",
                    message = $"Successfully deleted Pokemon subscription {id}.",
                }
                : new
                {
                    status = "Error",
                    message = $"Failed to delete Pokemon subscription {id}.",
                };
            return new JsonResult(response);
        }

        #endregion

        #region PvP Subscriptions

        [HttpGet("subscription/pvp/{guildId}/{userId}")]
        [Produces("application/json")]
        public IActionResult GetPvpSubscriptions(ulong guildId, ulong userId)
        {
            var subscription = _subscriptionManager.GetUserSubscriptions(guildId, userId);
            var response = new SubscriptionsResponse<List<PvpSubscription>>
            {
                Status = "OK",
                Data = subscription.PvP.ToList(),
            };
            return new JsonResult(response);
        }

        #endregion

        #region Raid Subscriptions

        [HttpGet("subscription/raids/{guildId}/{userId}")]
        [Produces("application/json")]
        public IActionResult GetRaidSubscriptions(ulong guildId, ulong userId)
        {
            var subscription = _subscriptionManager.GetUserSubscriptions(guildId, userId);
            var response = new SubscriptionsResponse<List<RaidSubscription>>
            {
                Status = "OK",
                Data = subscription.Raids.ToList(),
            };
            return new JsonResult(response);
        }

        #endregion

        #region Quest Subscriptions

        [HttpGet("subscription/quests/{guildId}/{userId}")]
        [Produces("application/json")]
        public IActionResult GetQuestSubscriptions(ulong guildId, ulong userId)
        {
            var subscription = _subscriptionManager.GetUserSubscriptions(guildId, userId);
            var response = new SubscriptionsResponse<List<QuestSubscription>>
            {
                Status = "OK",
                Data = subscription.Quests.ToList(),
            };
            return new JsonResult(response);
        }

        #endregion

        #region Invasion Subscriptions

        [HttpGet("subscription/invasions/{guildId}/{userId}")]
        [Produces("application/json")]
        public IActionResult GetInvasionSubscriptions(ulong guildId, ulong userId)
        {
            var subscription = _subscriptionManager.GetUserSubscriptions(guildId, userId);
            var response = new SubscriptionsResponse<List<InvasionSubscription>>
            {
                Status = "OK",
                Data = subscription.Invasions.ToList(),
            };
            return new JsonResult(response);
        }

        #endregion

        #region Lure Subscriptions

        [HttpGet("subscription/lures/{guildId}/{userId}")]
        [Produces("application/json")]
        public IActionResult GetLureSubscriptions(ulong guildId, ulong userId)
        {
            var subscription = _subscriptionManager.GetUserSubscriptions(guildId, userId);
            var response = new SubscriptionsResponse<List<LureSubscription>>
            {
                Status = "OK",
                Data = subscription.Lures.ToList(),
            };
            return new JsonResult(response);
        }

        #endregion

        #region Gym Subscriptions

        [HttpGet("subscription/gyms/{guildId}/{userId}")]
        [Produces("application/json")]
        public IActionResult GetGymSubscriptions(ulong guildId, ulong userId)
        {
            var subscription = _subscriptionManager.GetUserSubscriptions(guildId, userId);
            var response = new SubscriptionsResponse<List<GymSubscription>>
            {
                Status = "OK",
                Data = subscription.Gyms.ToList(),
            };
            return new JsonResult(response);
        }

        #endregion

        #region Location Subscriptions

        [HttpGet("subscription/locations/{guildId}/{userId}")]
        [Produces("application/json")]
        public IActionResult GetLocationSubscriptions(ulong guildId, ulong userId)
        {
            var subscription = _subscriptionManager.GetUserSubscriptions(guildId, userId);
            var response = new SubscriptionsResponse<List<LocationSubscription>>
            {
                Status = "OK",
                Data = subscription.Locations.ToList(),
            };
            return new JsonResult(response);
        }

        #endregion

        [HttpPost("test")]
        [Produces("application/json")]
        public IActionResult Test(dynamic json)
        {
            Console.WriteLine($"Json: {json}");
            var response = new
            {
                status = "OK",
                message = "Data Fetched",
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