﻿namespace WhMgr.Services.Subscriptions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Timers;
    using System.Threading.Tasks;

    using Microsoft.EntityFrameworkCore;
    //using Microsoft.Extensions.Logging;
    using InvasionCharacter = POGOProtos.Rpc.EnumWrapper.Types.InvasionCharacter;

    using WhMgr.Common;
    using WhMgr.Data.Contexts;
    using WhMgr.Extensions;
    using WhMgr.Services.Subscriptions.Models;

    public class SubscriptionManagerService : ISubscriptionManagerService
    {
        private readonly Microsoft.Extensions.Logging.ILogger<ISubscriptionManagerService> _logger;
        private readonly IDbContextFactory<AppDbContext> _dbFactory;
        private List<Subscription> _subscriptions;
        private readonly Timer _timer;

        public IReadOnlyList<Subscription> Subscriptions => _subscriptions;

        public SubscriptionManagerService(
            Microsoft.Extensions.Logging.ILogger<ISubscriptionManagerService> logger,
            IDbContextFactory<AppDbContext> dbFactory)
        {
            _logger = logger;
            _dbFactory = dbFactory;

            _timer = new Timer(60 * 1000); // every minute TODO: Use config value
            _timer.Elapsed += async (sender, e) =>
            {
                await ReloadSubscriptionsAsync();
            };
            _timer.Start();

            Task.Run(async () => await ReloadSubscriptionsAsync(true));
        }

        #region Get Subscriptions

        public async Task<List<Subscription>> GetUserSubscriptionsAsync()
        {
            using (var ctx = _dbFactory.CreateDbContext())
            {
                _subscriptions = (await ctx.Subscriptions.Where(s => s.Status != NotificationStatusType.None)
                                                         // Include Pokemon subscriptions
                                                         .Include(s => s.Pokemon)
                                                         // Include PvP subscriptions
                                                         .Include(s => s.PvP)
                                                         // Include Raid subscriptions
                                                         .Include(s => s.Raids)
                                                         // Include Quest subscriptions
                                                         .Include(s => s.Quests)
                                                         // Include Invasion subscriptions
                                                         .Include(s => s.Invasions)
                                                         // Include Lure subscriptions
                                                         .Include(s => s.Lures)
                                                         // Include Gym subscriptions
                                                         .Include(s => s.Gyms)
                                                         // Include Location subscriptions
                                                         .Include(s => s.Locations)
                                                         .ToListAsync())
                                                         .ToList();
                return _subscriptions;
            }
        }

        public Subscription GetUserSubscriptions(ulong guildId, ulong userId)
        {
            /*
            using (var ctx = _dbFactory.CreateDbContext())
            {
                var subscription = await ctx.Subscriptions.Include(s => s.Pokemon)
                                                          // Include PvP subscriptions
                                                          .Include(s => s.PvP)
                                                          // Include Raid subscriptions
                                                          .Include(s => s.Raids)
                                                          // Include Quest subscriptions
                                                          .Include(s => s.Quests)
                                                          // Include Invasion subscriptions
                                                          .Include(s => s.Invasions)
                                                          // Include Lure subscriptions
                                                          .Include(s => s.Lures)
                                                          // Include Gym subscriptions
                                                          .Include(s => s.Gyms)
                                                          // Include Location subscriptions
                                                          .Include(s => s.Locations)
                                                          .FirstOrDefaultAsync(s => s.Status != NotificationStatusType.None
                                                                                 && s.GuildId == guildId
                                                                                 && s.UserId == userId);
                return subscription;
            }
            */
            return _subscriptions?.FirstOrDefault(x => x.GuildId == guildId && x.UserId == userId);
        }

        public List<Subscription> GetSubscriptionsByPokemonId(uint pokemonId)
        {
            return _subscriptions?
                .Where(x => x.IsEnabled(NotificationStatusType.Pokemon) &&
                            x.Pokemon != null &&
                            x.Pokemon.Any(y => y.PokemonId.Contains(pokemonId))
                      )
                .ToList();
        }

        public List<Subscription> GetSubscriptionsByPvpPokemonId(uint pokemonId)
        {
            return _subscriptions?
                .Where(x => x.IsEnabled(NotificationStatusType.PvP) &&
                            x.PvP != null &&
                            x.PvP.Any(y => y.PokemonId.Contains(pokemonId))
                      )
                .ToList();
        }

        public List<Subscription> GetSubscriptionsByRaidPokemonId(uint pokemonId)
        {
            return _subscriptions?
                .Where(x => x.IsEnabled(NotificationStatusType.Raids) &&
                            x.Raids != null &&
                            x.Raids.Any(y => y.PokemonId.Contains(pokemonId))
                      )
                .ToList();
        }

        public List<Subscription> GetSubscriptionsByQuest(string pokestopName, string reward)
        {
            return _subscriptions?
                .Where(x => x.IsEnabled(NotificationStatusType.Quests) &&
                            x.Quests != null &&
                            x.Quests.Any(y =>
                                reward.Contains(y.RewardKeyword)
                                || (y.PokestopName != null && (pokestopName.Contains(y.PokestopName)
                                || string.Equals(pokestopName, y.PokestopName, StringComparison.OrdinalIgnoreCase)))
                      )
                ).ToList();
        }

        public List<Subscription> GetSubscriptionsByInvasion(string pokestopName, InvasionCharacter gruntType, List<uint> encounterRewards)
        {
            return _subscriptions?
                .Where(x => x.IsEnabled(NotificationStatusType.Invasions) &&
                            x.Invasions != null &&
                            x.Invasions.Any(y =>
                            {
                                var rewardMatches = y.RewardPokemonId.Intersects(encounterRewards);
                                var typeMatches = y.InvasionType.Contains(gruntType) && gruntType != InvasionCharacter.CharacterUnset;
                                var pokestopMatches = !string.IsNullOrWhiteSpace(y.PokestopName) && !string.IsNullOrWhiteSpace(pokestopName) &&
                                (
                                    pokestopName.Contains(y.PokestopName)
                                    || string.Equals(pokestopName, y.PokestopName, StringComparison.OrdinalIgnoreCase)
                                );
                                return rewardMatches || typeMatches || pokestopMatches;
                            })
                        )
                .ToList();
        }

        public List<Subscription> GetSubscriptionsByLure(string pokestopName, PokestopLureType lure)
        {
            return _subscriptions?
                .Where(x => x.IsEnabled(NotificationStatusType.Lures) &&
                            x.Lures != null &&
                            x.Lures.Any(y =>
                                y.LureType?.Contains(lure) ?? false
                                || !string.IsNullOrWhiteSpace(y.PokestopName) && !string.IsNullOrWhiteSpace(pokestopName) &&
                                (
                                    pokestopName.Contains(y.PokestopName)
                                    || string.Equals(pokestopName, y.PokestopName, StringComparison.OrdinalIgnoreCase)
                                )
                            )
                      )
                .ToList();
        }

        public List<Subscription> GetSubscriptionsByGymName(string name)
        {
            return _subscriptions?
                .Where(x => x.IsEnabled(NotificationStatusType.Gyms) &&
                            x.Gyms != null &&
                            x.Gyms.Any(y => string.Equals(name, y.Name, StringComparison.OrdinalIgnoreCase) || y.Name.ToLower().Contains(name.ToLower()))
                      )
                .ToList();
        }

        #endregion

        public async Task SetSubscriptionStatusAsync(Subscription subscription, NotificationStatusType status)
        {
            using (var ctx = _dbFactory.CreateDbContext())
            {
                subscription.Status = status;
                await ctx.SaveChangesAsync(true);
            }
        }

        public bool Save(Subscription subscription)
        {
            // Save subscription changes
            try
            {
                using (var ctx = _dbFactory.CreateDbContext())
                {
                    ctx.Update(subscription);
                    ctx.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to save subscription with id {subscription.Id} (UserId: {subscription.UserId}, GuildId: {subscription.GuildId}): {ex}");
                return false;
            }
        }

        /// <summary>
        /// Reload all user subscriptions
        /// </summary>
        public async Task ReloadSubscriptionsAsync(bool skipCheck = false, ushort reloadM = 5)
        {
            // Only reload based on last_changed timestamp in metadata table
            var lastModifiedTimestamp = GetLastModifiedTimestamp();
            var utcNow = DateTime.UtcNow.GetUnixTimestamp();
            var fiveMinutes = reloadM * 60 * 60;
            var delta = utcNow - lastModifiedTimestamp;
            // Check if last_modified was set within the last 5 minutes
            if (!skipCheck && delta > fiveMinutes)
                return;

            // Updated, reload subscriptions
            var subs = await GetUserSubscriptionsAsync();
            if (subs == null)
                return;

            _subscriptions = subs;
        }

        private ulong GetLastModifiedTimestamp()
        {
            using (var ctx = _dbFactory.CreateDbContext())
            {
                var lastModified = ctx.Metadata.Find("LAST_MODIFIED");
                return !ulong.TryParse(lastModified?.Value, out var value) ? 0 : value;
            }
        }
    }
}