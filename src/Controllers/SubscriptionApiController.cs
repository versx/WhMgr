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
            var response = await GetSubscription<PokemonSubscription>(id);
            return new JsonResult(response);
        }

        [HttpPost("subscription/pokemon/create")]
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
                    Status = NotificationStatusType.All,
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

        [HttpPut("subscription/pokemon/update/{id}")]
        [Produces("application/json")]
        public async Task<IActionResult> PokemonUpdate(int id, PokemonSubscription pokemonSubscription)
        {
            var response = await UpdateSubscription(id, pokemonSubscription);
            return new JsonResult(response);
        }

        [HttpDelete("subscription/pokemon/delete/{id}")]
        [Produces("application/json")]
        public async Task<IActionResult> PokemonDelete(int id)
        {
            var response = await DeleteSubscription<PokemonSubscription>(id);
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
                Status = subscription != null
                    ? "OK"
                    : "Error",
                Data = subscription.PvP.ToList(),
            };
            return new JsonResult(response);
        }

        [HttpGet("subscription/pvp/{id}")]
        [Produces("application/json")]
        public async Task<IActionResult> GetPvpSubscription(int id)
        {
            var response = await GetSubscription<PvpSubscription>(id);
            return new JsonResult(response);
        }

        [HttpPost("subscription/pvp/create")]
        [Produces("application/json")]
        public async Task<IActionResult> PvpCreate(PvpSubscription pvpSubscription)
        {
            if (pvpSubscription == null)
            {
                return new JsonResult(new
                {
                    status = "Error",
                    message = "Failed to create PvP subscription, data was null.",
                });
            }

            //  Check if guild_id and user_id not equal to 0
            if (pvpSubscription.GuildId == 0 || pvpSubscription.UserId == 0)
            {
                return new JsonResult(new
                {
                    status = "Error",
                    message = "Both GuildId and UserId are required.",
                });
            }

            var subscription = _subscriptionManager.GetUserSubscriptions(pvpSubscription.GuildId, pvpSubscription.UserId);
            if (subscription == null)
            {
                // Subscription does not exist, create new
                subscription = new Subscription
                {
                    GuildId = pvpSubscription.GuildId,
                    UserId = pvpSubscription.UserId,
                    Status = NotificationStatusType.All,
                };
            }
            subscription.PvP.Add(pvpSubscription);
            var result = await _subscriptionManager.CreateSubscriptionAsync(subscription).ConfigureAwait(false);
            dynamic response = result
                ? new
                {
                    status = "OK",
                    message = "Successfully created PvP subscription.",
                    data = pvpSubscription,
                }
                : new
                {
                    status = "Error",
                    message = "Failed to create PvP subscription.",
                };
            return new JsonResult(response);
        }

        [HttpPut("subscription/pvp/update/{id}")]
        [Produces("application/json")]
        public async Task<IActionResult> PvpUpdate(int id, PvpSubscription pvpSubscription)
        {
            var response = await UpdateSubscription(id, pvpSubscription);
            return new JsonResult(response);
        }

        [HttpDelete("subscription/pvp/delete/{id}")]
        [Produces("application/json")]
        public async Task<IActionResult> PvpDelete(int id)
        {
            var response = await DeleteSubscription<PvpSubscription>(id);
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
                Status = subscription != null
                    ? "OK"
                    : "Error",
                Data = subscription.Raids.ToList(),
            };
            return new JsonResult(response);
        }

        [HttpGet("subscription/raid/{id}")]
        [Produces("application/json")]
        public async Task<IActionResult> GetRaidSubscription(int id)
        {
            var response = await GetSubscription<RaidSubscription>(id);
            return new JsonResult(response);
        }

        [HttpPost("subscription/raid/create")]
        [Produces("application/json")]
        public async Task<IActionResult> RaidCreate(RaidSubscription raidSubscription)
        {
            if (raidSubscription == null)
            {
                return new JsonResult(new
                {
                    status = "Error",
                    message = "Failed to create Raid subscription, data was null.",
                });
            }

            //  Check if guild_id and user_id not equal to 0
            if (raidSubscription.GuildId == 0 || raidSubscription.UserId == 0)
            {
                return new JsonResult(new
                {
                    status = "Error",
                    message = "Both GuildId and UserId are required.",
                });
            }

            var subscription = _subscriptionManager.GetUserSubscriptions(raidSubscription.GuildId, raidSubscription.UserId);
            if (subscription == null)
            {
                // Subscription does not exist, create new
                subscription = new Subscription
                {
                    GuildId = raidSubscription.GuildId,
                    UserId = raidSubscription.UserId,
                    Status = NotificationStatusType.All,
                };
            }
            subscription.Raids.Add(raidSubscription);
            var result = await _subscriptionManager.CreateSubscriptionAsync(subscription).ConfigureAwait(false);
            dynamic response = result
                ? new
                {
                    status = "OK",
                    message = "Successfully created Raid subscription.",
                    data = raidSubscription,
                }
                : new
                {
                    status = "Error",
                    message = "Failed to create Raid subscription.",
                };
            return new JsonResult(response);
        }

        [HttpPut("subscription/raid/update/{id}")]
        [Produces("application/json")]
        public async Task<IActionResult> RaidUpdate(int id, RaidSubscription raidSubscription)
        {
            var response = await UpdateSubscription(id, raidSubscription);
            return new JsonResult(response);
        }

        [HttpDelete("subscription/raid/delete/{id}")]
        [Produces("application/json")]
        public async Task<IActionResult> RaidDelete(int id)
        {
            var response = await DeleteSubscription<RaidSubscription>(id);
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
                Status = subscription != null
                    ? "OK"
                    : "Error",
                Data = subscription.Quests.ToList(),
            };
            return new JsonResult(response);
        }

        [HttpGet("subscription/quest/{id}")]
        [Produces("application/json")]
        public async Task<IActionResult> GetQuestSubscription(int id)
        {
            var response = await GetSubscription<QuestSubscription>(id);
            return new JsonResult(response);
        }

        [HttpPost("subscription/quest/create")]
        [Produces("application/json")]
        public async Task<IActionResult> QuestCreate(QuestSubscription questSubscription)
        {
            if (questSubscription == null)
            {
                return new JsonResult(new
                {
                    status = "Error",
                    message = "Failed to create Quest subscription, data was null.",
                });
            }

            //  Check if guild_id and user_id not equal to 0
            if (questSubscription.GuildId == 0 || questSubscription.UserId == 0)
            {
                return new JsonResult(new
                {
                    status = "Error",
                    message = "Both GuildId and UserId are required.",
                });
            }

            var subscription = _subscriptionManager.GetUserSubscriptions(questSubscription.GuildId, questSubscription.UserId);
            if (subscription == null)
            {
                // Subscription does not exist, create new
                subscription = new Subscription
                {
                    GuildId = questSubscription.GuildId,
                    UserId = questSubscription.UserId,
                    Status = NotificationStatusType.All,
                };
            }
            subscription.Quests.Add(questSubscription);
            var result = await _subscriptionManager.CreateSubscriptionAsync(subscription).ConfigureAwait(false);
            dynamic response = result
                ? new
                {
                    status = "OK",
                    message = "Successfully created Quest subscription.",
                    data = questSubscription,
                }
                : new
                {
                    status = "Error",
                    message = "Failed to create Quest subscription.",
                };
            return new JsonResult(response);
        }

        [HttpPut("subscription/quest/update/{id}")]
        [Produces("application/json")]
        public async Task<IActionResult> QuestUpdate(int id, QuestSubscription questSubscription)
        {
            var response = await UpdateSubscription(id, questSubscription);
            return new JsonResult(response);
        }

        [HttpDelete("subscription/quest/delete/{id}")]
        [Produces("application/json")]
        public async Task<IActionResult> QuestDelete(int id)
        {
            var response = await DeleteSubscription<QuestSubscription>(id);
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
                Status = subscription != null
                    ? "OK"
                    : "Error",
                Data = subscription.Invasions.ToList(),
            };
            return new JsonResult(response);
        }

        [HttpGet("subscription/invasion/{id}")]
        [Produces("application/json")]
        public async Task<IActionResult> GetInvasionSubscription(int id)
        {
            var response = await GetSubscription<InvasionSubscription>(id);
            return new JsonResult(response);
        }

        [HttpPost("subscription/invasion/create")]
        [Produces("application/json")]
        public async Task<IActionResult> QuestCreate(InvasionSubscription invasionSubscription)
        {
            if (invasionSubscription == null)
            {
                return new JsonResult(new
                {
                    status = "Error",
                    message = "Failed to create Invasion subscription, data was null.",
                });
            }

            //  Check if guild_id and user_id not equal to 0
            if (invasionSubscription.GuildId == 0 || invasionSubscription.UserId == 0)
            {
                return new JsonResult(new
                {
                    status = "Error",
                    message = "Both GuildId and UserId are required.",
                });
            }

            var subscription = _subscriptionManager.GetUserSubscriptions(invasionSubscription.GuildId, invasionSubscription.UserId);
            if (subscription == null)
            {
                // Subscription does not exist, create new
                subscription = new Subscription
                {
                    GuildId = invasionSubscription.GuildId,
                    UserId = invasionSubscription.UserId,
                    Status = NotificationStatusType.All,
                };
            }
            subscription.Invasions.Add(invasionSubscription);
            var result = await _subscriptionManager.CreateSubscriptionAsync(subscription).ConfigureAwait(false);
            dynamic response = result
                ? new
                {
                    status = "OK",
                    message = "Successfully created Invasion subscription.",
                    data = invasionSubscription,
                }
                : new
                {
                    status = "Error",
                    message = "Failed to create Invasion subscription.",
                };
            return new JsonResult(response);
        }

        [HttpPut("subscription/invasion/update/{id}")]
        [Produces("application/json")]
        public async Task<IActionResult> InvasionUpdate(int id, InvasionSubscription invasionSubscription)
        {
            var response = await UpdateSubscription(id, invasionSubscription);
            return new JsonResult(response);
        }

        [HttpDelete("subscription/invasion/delete/{id}")]
        [Produces("application/json")]
        public async Task<IActionResult> InvasionDelete(int id)
        {
            var response = await DeleteSubscription<InvasionSubscription>(id);
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
                Status = subscription != null
                    ? "OK"
                    : "Error",
                Data = subscription.Lures.ToList(),
            };
            return new JsonResult(response);
        }

        [HttpGet("subscription/lure/{id}")]
        [Produces("application/json")]
        public async Task<IActionResult> GetLureSubscription(int id)
        {
            var response = await GetSubscription<LureSubscription>(id);
            return new JsonResult(response);
        }

        [HttpPost("subscription/lure/create")]
        [Produces("application/json")]
        public async Task<IActionResult> LureCreate(LureSubscription lureSubscription)
        {
            if (lureSubscription == null)
            {
                return new JsonResult(new
                {
                    status = "Error",
                    message = "Failed to create Lure subscription, data was null.",
                });
            }

            //  Check if guild_id and user_id not equal to 0
            if (lureSubscription.GuildId == 0 || lureSubscription.UserId == 0)
            {
                return new JsonResult(new
                {
                    status = "Error",
                    message = "Both GuildId and UserId are required.",
                });
            }

            var subscription = _subscriptionManager.GetUserSubscriptions(lureSubscription.GuildId, lureSubscription.UserId);
            if (subscription == null)
            {
                // Subscription does not exist, create new
                subscription = new Subscription
                {
                    GuildId = lureSubscription.GuildId,
                    UserId = lureSubscription.UserId,
                    Status = NotificationStatusType.All,
                };
            }
            subscription.Lures.Add(lureSubscription);
            var result = await _subscriptionManager.CreateSubscriptionAsync(subscription).ConfigureAwait(false);
            dynamic response = result
                ? new
                {
                    status = "OK",
                    message = "Successfully created Lure subscription.",
                    data = lureSubscription,
                }
                : new
                {
                    status = "Error",
                    message = "Failed to create Lure subscription.",
                };
            return new JsonResult(response);
        }

        [HttpPut("subscription/lure/update/{id}")]
        [Produces("application/json")]
        public async Task<IActionResult> LureUpdate(int id, LureSubscription lureSubscription)
        {
            var response = await UpdateSubscription(id, lureSubscription);
            return new JsonResult(response);
        }

        [HttpDelete("subscription/lure/delete/{id}")]
        [Produces("application/json")]
        public async Task<IActionResult> LureDelete(int id)
        {
            var response = await DeleteSubscription<LureSubscription>(id);
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
                Status = subscription != null
                    ? "OK"
                    : "Error",
                Data = subscription.Gyms.ToList(),
            };
            return new JsonResult(response);
        }

        [HttpGet("subscription/gym/{id}")]
        [Produces("application/json")]
        public async Task<IActionResult> GetGymSubscription(int id)
        {
            var response = await GetSubscription<GymSubscription>(id);
            return new JsonResult(response);
        }

        [HttpPost("subscription/gym/create")]
        [Produces("application/json")]
        public async Task<IActionResult> LureCreate(GymSubscription gymSubscription)
        {
            if (gymSubscription == null)
            {
                return new JsonResult(new
                {
                    status = "Error",
                    message = "Failed to create Gym subscription, data was null.",
                });
            }

            //  Check if guild_id and user_id not equal to 0
            if (gymSubscription.GuildId == 0 || gymSubscription.UserId == 0)
            {
                return new JsonResult(new
                {
                    status = "Error",
                    message = "Both GuildId and UserId are required.",
                });
            }

            var subscription = _subscriptionManager.GetUserSubscriptions(gymSubscription.GuildId, gymSubscription.UserId);
            if (subscription == null)
            {
                // Subscription does not exist, create new
                subscription = new Subscription
                {
                    GuildId = gymSubscription.GuildId,
                    UserId = gymSubscription.UserId,
                    Status = NotificationStatusType.All,
                };
            }
            subscription.Gyms.Add(gymSubscription);
            var result = await _subscriptionManager.CreateSubscriptionAsync(subscription).ConfigureAwait(false);
            dynamic response = result
                ? new
                {
                    status = "OK",
                    message = "Successfully created Gym subscription.",
                    data = gymSubscription,
                }
                : new
                {
                    status = "Error",
                    message = "Failed to create Gym subscription.",
                };
            return new JsonResult(response);
        }

        [HttpPut("subscription/gym/update/{id}")]
        [Produces("application/json")]
        public async Task<IActionResult> GymUpdate(int id, GymSubscription gymSubscription)
        {
            var response = await UpdateSubscription(id, gymSubscription);
            return new JsonResult(response);
        }

        [HttpDelete("subscription/gym/delete/{id}")]
        [Produces("application/json")]
        public async Task<IActionResult> GymDelete(int id)
        {
            var response = await DeleteSubscription<GymSubscription>(id);
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
                Status = subscription != null
                    ? "OK"
                    : "Error",
                Data = subscription.Locations.ToList(),
            };
            return new JsonResult(response);
        }

        [HttpGet("subscription/location/{id}")]
        [Produces("application/json")]
        public async Task<IActionResult> GetLocationSubscription(int id)
        {
            var response = await GetSubscription<LocationSubscription>(id);
            return new JsonResult(response);
        }

        [HttpPost("subscription/location/create")]
        [Produces("application/json")]
        public async Task<IActionResult> LocationCreate(LocationSubscription locationSubscription)
        {
            if (locationSubscription == null)
            {
                return new JsonResult(new
                {
                    status = "Error",
                    message = "Failed to create Location subscription, data was null.",
                });
            }

            //  Check if guild_id and user_id not equal to 0
            if (locationSubscription.GuildId == 0 || locationSubscription.UserId == 0)
            {
                return new JsonResult(new
                {
                    status = "Error",
                    message = "Both GuildId and UserId are required.",
                });
            }

            var subscription = _subscriptionManager.GetUserSubscriptions(locationSubscription.GuildId, locationSubscription.UserId);
            if (subscription == null)
            {
                // Subscription does not exist, create new
                subscription = new Subscription
                {
                    GuildId = locationSubscription.GuildId,
                    UserId = locationSubscription.UserId,
                    Status = NotificationStatusType.All,
                };
            }
            subscription.Locations.Add(locationSubscription);
            var result = await _subscriptionManager.CreateSubscriptionAsync(subscription).ConfigureAwait(false);
            dynamic response = result
                ? new
                {
                    status = "OK",
                    message = "Successfully created Location subscription.",
                    data = locationSubscription,
                }
                : new
                {
                    status = "Error",
                    message = "Failed to create Location subscription.",
                };
            return new JsonResult(response);
        }

        [HttpPut("subscription/location/update/{id}")]
        [Produces("application/json")]
        public async Task<IActionResult> LocationUpdate(int id, LocationSubscription gymSubscription)
        {
            var response = await UpdateSubscription(id, gymSubscription);
            return new JsonResult(response);
        }

        [HttpDelete("subscription/location/delete/{id}")]
        [Produces("application/json")]
        public async Task<IActionResult> LocationDelete(int id)
        {
            var response = await DeleteSubscription<LocationSubscription>(id);
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

        #region Generic Helpers

        private async Task<dynamic> GetSubscription<T>(int id) where T : BaseSubscription
        {
            var subscription = await _subscriptionManager.FindByIdAsync<T>(id);
            var response = new SubscriptionsResponse<T>
            {
                Status = subscription != null
                    ? "OK"
                    : "Error",
                Data = subscription,
            };
            return response;
        }

        private async Task<dynamic> UpdateSubscription<T>(int id, T updatedSubscription) where T : BaseSubscription
        {
            if (updatedSubscription == null)
            {
                return new JsonResult(new
                {
                    status = "Error",
                    message = $"Failed to create {nameof(updatedSubscription)} subscription, data was null.",
                });
            }

            //  Check if guild_id and user_id not equal to 0
            if (updatedSubscription.GuildId == 0 || updatedSubscription.UserId == 0)
            {
                return new JsonResult(new
                {
                    status = "Error",
                    message = "Both GuildId and UserId are required.",
                });
            }

            var existingSubscription = await _subscriptionManager.FindByIdAsync<T>(id);
            if (existingSubscription == null)
            {
                // Subscription does not exist
                return new JsonResult(new
                {
                    status = "Error",
                    message = $"{nameof(updatedSubscription)} subscription with id {id} does not exist.",
                });
            }

            var result = await _subscriptionManager.UpdateSubscriptionAsync(id, updatedSubscription);
            var response = result
                ? new
                {
                    status = "OK",
                    message = $"Successfully updated {nameof(updatedSubscription)} subscription {id}.",
                }
                : new
                {
                    status = "Error",
                    message = $"Failed to update {nameof(updatedSubscription)} subscription {id}.",
                };
            return response;
        }

        private async Task<dynamic> DeleteSubscription<T>(int id) where T : BaseSubscription
        {
            var result = await _subscriptionManager.DeleteByIdAsync<T>(id);
            var response = result
                ? new
                {
                    status = "OK",
                    message = $"Successfully deleted {nameof(T)} subscription {id}.",
                }
                : new
                {
                    status = "Error",
                    message = $"Failed to delete {nameof(T)} subscription {id}.",
                };
            return response;
        }

        #endregion
    }

    public class SubscriptionsResponse<T>
    {
        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("data")]
        public T Data { get; set; }
    }
}