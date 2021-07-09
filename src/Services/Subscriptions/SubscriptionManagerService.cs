namespace WhMgr.Services.Subscriptions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Timers;
    using System.Threading.Tasks;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using InvasionCharacter = POGOProtos.Rpc.EnumWrapper.Types.InvasionCharacter;

    using WhMgr.Common;
    using WhMgr.Data.Contexts;
    using WhMgr.Extensions;
    using WhMgr.Services.Subscriptions.Models;

    public class SubscriptionManagerService : ISubscriptionManagerService
    {
        private readonly ILogger<ISubscriptionManagerService> _logger;
        private readonly IDbContextFactory<AppDbContext> _dbFactory;
        private List<Subscription> _subscriptions;
        private readonly Timer _timer;

        public IReadOnlyList<Subscription> Subscriptions => _subscriptions;

        public SubscriptionManagerService(
            ILogger<ISubscriptionManagerService> logger,
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

            // TODO: Fix
            ReloadSubscriptionsAsync();
        }

        #region Get Subscriptions

        public async Task<List<Subscription>> GetUserSubscriptions()
        {
            using (var ctx = _dbFactory.CreateDbContext())
            {
                _subscriptions = (await ctx.Subscriptions.Where(s => s.Status != NotificationStatusType.None)
                                                         // Include Pokemon subscriptions
                                                         .Include(s => s.Pokemon)
                                                         //.ThenInclude(p => p.Subscription)
                                                         // Include PvP subscriptions
                                                         .Include(s => s.PvP)
                                                         //.ThenInclude(p => p.Subscription)
                                                         // Include Raid subscriptions
                                                         .Include(s => s.Raids)
                                                         //.ThenInclude(r => r.Subscription)
                                                         // Include Quest subscriptions
                                                         .Include(s => s.Quests)
                                                         //.ThenInclude(q => q.Subscription)
                                                         // Include Invasion subscriptions
                                                         .Include(s => s.Invasions)
                                                         //.ThenInclude(i => i.Subscription)
                                                         // Include Lure subscriptions
                                                         .Include(s => s.Lures)
                                                         //.ThenInclude(l => l.Subscription)
                                                         // Include Gym subscriptions
                                                         .Include(s => s.Gyms)
                                                         //.ThenInclude(g => g.Subscription)
                                                         // Include Location subscriptions
                                                         .Include(s => s.Locations)
                                                         //.ThenInclude(l => l.Subscription)
                                                         .ToListAsync()
                                                         )
                                                         //.Where(x => x.Status != NotificationStatusType.None)
                                                         .ToList();
                return _subscriptions;
            }
        }

        public async Task<Subscription> GetUserSubscriptionsAsync(ulong guildId, ulong userId)
        {
            using (var ctx = _dbFactory.CreateDbContext())
            {
                var subscription = await ctx.Subscriptions.FirstOrDefaultAsync(s => s.Status != NotificationStatusType.None
                                                                                  && s.GuildId == guildId
                                                                                  && s.UserId == userId);
                    /*
                                                         // Include Pokemon subscriptions
                                                         .Include(s => s.Pokemon)
                                                         //.ThenInclude(p => p.Subscription)
                                                         // Include PvP subscriptions
                                                         .Include(s => s.PvP)
                                                         //.ThenInclude(p => p.Subscription)
                                                         // Include Raid subscriptions
                                                         .Include(s => s.Raids)
                                                         //.ThenInclude(r => r.Subscription)
                                                         // Include Quest subscriptions
                                                         .Include(s => s.Quests)
                                                         //.ThenInclude(q => q.Subscription)
                                                         // Include Invasion subscriptions
                                                         .Include(s => s.Invasions)
                                                         //.ThenInclude(i => i.Subscription)
                                                         // Include Lure subscriptions
                                                         .Include(s => s.Lures)
                                                         //.ThenInclude(l => l.Subscription)
                                                         // Include Gym subscriptions
                                                         .Include(s => s.Gyms)
                                                         //.ThenInclude(g => g.Subscription)
                                                         // Include Location subscriptions
                                                         .Include(s => s.Locations)
                                                         //.ThenInclude(l => l.Subscription)
                                                         .ToListAsync()
                                                         );
                */
                return subscription;
            }
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
                                reward.Contains(y.RewardKeyword) ||
                                (y.PokestopName != null && (pokestopName.Contains(y.PokestopName) ||
                                string.Equals(pokestopName, y.PokestopName, StringComparison.OrdinalIgnoreCase)))
                      )
                ).ToList();
        }

        public List<Subscription> GetSubscriptionsByInvasion(string pokestopName, InvasionCharacter gruntType, List<uint> encounterRewards)
        {
            return _subscriptions?
                .Where(x => x.IsEnabled(NotificationStatusType.Invasions) &&
                            x.Invasions != null &&
                            x.Invasions.Any(y =>
                                y.RewardPokemonId.Intersects(encounterRewards) ||
                                gruntType == y.InvasionType ||
                                (!string.IsNullOrEmpty(y.PokestopName) && !string.IsNullOrEmpty(pokestopName) && pokestopName.Contains(y.PokestopName))
                                || string.Equals(pokestopName, y.PokestopName, StringComparison.OrdinalIgnoreCase)
                            )
                        )
                .ToList();
        }

        public List<Subscription> GetSubscriptionsByLure(string pokestopName, PokestopLureType lure)
        {
            return _subscriptions?
                .Where(x => x.IsEnabled(NotificationStatusType.Lures) &&
                            x.Lures != null &&
                            x.Lures.Any(y => lure == y.LureType ||
                                            (!string.IsNullOrEmpty(y.PokestopName) && !string.IsNullOrEmpty(pokestopName) && pokestopName.Contains(y.PokestopName))
                                            || string.Equals(pokestopName, y.PokestopName, StringComparison.OrdinalIgnoreCase))
                      )
                .ToList();
        }

        public List<Subscription> GetSubscriptionsByGymName(string name)
        {
            return _subscriptions?
                .Where(x => x.IsEnabled(NotificationStatusType.Gyms) &&
                            x.Gyms != null &&
                            x.Gyms.Any(y => string.Compare(y.Name, name, true) == 0 || y.Name.ToLower().Contains(name.ToLower()))
                      )
                .ToList();
        }

        #endregion

        public bool Save(Subscription subscription)
        {
            return true;
        }

        /// <summary>
        /// Reload all user subscriptions
        /// </summary>
        public async Task ReloadSubscriptionsAsync()
        {
            // TODO: Only reload based on last_changed timestamp in metadata table
            var subs = await GetUserSubscriptions();
            if (subs == null)
                return;

            _subscriptions = subs;
        }
    }
}