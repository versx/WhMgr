namespace WhMgr.Services.Subscriptions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
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

        public SubscriptionManagerService(
            ILogger<ISubscriptionManagerService> logger,
            IDbContextFactory<AppDbContext> dbFactory)
        {
            _logger = logger;
            _dbFactory = dbFactory;

            // TODO: Fix
            ReloadSubscriptions().ConfigureAwait(false)
                                 .GetAwaiter()
                                 .GetResult();
        }

        public async Task<List<Subscription>> GetUserSubscriptions()
        {
            using (var ctx = _dbFactory.CreateDbContext())
            {
                _subscriptions = (await ctx.Subscriptions.Where(s => s.Status != NotificationStatusType.None)
                                                         // Include Pokemon subscriptions
                                                         .Include(s => s.Pokemon)
                                                         .ThenInclude(p => p.Subscription)
                                                         // Include PvP subscriptions
                                                         .Include(s => s.PvP)
                                                         .ThenInclude(p => p.Subscription)
                                                         // Include Raid subscriptions
                                                         .Include(s => s.Raids)
                                                         .ThenInclude(r => r.Subscription)
                                                         // Include Quest subscriptions
                                                         .Include(s => s.Quests)
                                                         .ThenInclude(q => q.Subscription)
                                                         // Include Invasion subscriptions
                                                         .Include(s => s.Invasions)
                                                         .ThenInclude(i => i.Subscription)
                                                         // Include Lure subscriptions
                                                         .Include(s => s.Lures)
                                                         .ThenInclude(l => l.Subscription)
                                                         // Include Gym subscriptions
                                                         .Include(s => s.Gyms)
                                                         .ThenInclude(g => g.Subscription)
                                                         // Include Location subscriptions
                                                         .Include(s => s.Locations)
                                                         .ThenInclude(l => l.Subscription)
                                                         .ToListAsync()
                                                         )
                                                         //.Where(x => x.Status != NotificationStatusType.None)
                                                         .ToList();
                return _subscriptions;
            }
        }

        public List<Subscription> GetSubscriptionsByPokemonId(uint pokemonId)
        {
            /*
            var subscriptions = _subscriptions.Where(x =>
                x.IsEnabled(NotificationStatusType.Pokemon) &&
                //x.Pokemon != null &&
                x.Pokemon.Exists(y => y.PokemonId.Contains(pokemonId))
            ).ToList();
            return subscriptions;
            */
            //using (var ctx = _dbFactory.CreateDbContext())
            //{
                /*
                var pokemon = await ctx.Subscriptions.Where(x => x.Pokemon.FirstOrDefault(y => y.PokemonId.Contains(pokemonId)) != null).ToListAsync();
                return pokemon;
                */
                return _subscriptions?
                    .Where(x => x.IsEnabled(NotificationStatusType.Pokemon) &&
                                x.Pokemon != null &&
                                x.Pokemon.Any(y => y.PokemonId.Contains(pokemonId))
                          )
                    .ToList();
            //}
        }

        public async Task<List<PvpSubscription>> GetSubscriptionsByPvpPokemonId(uint pokemonId)
        {
            /*
            return _subscriptions.Where(x =>
                x.IsEnabled(NotificationStatusType.PvP) &&
                //x.PvP != null &&
                x.PvP.Exists(y => y.PokemonId == pokemonId)
            ).ToList();
            */
            using (var ctx = _dbFactory.CreateDbContext())
            {
                var pvp = await ctx.Pvp.Where(x => x.PokemonId.Contains(pokemonId)).ToListAsync();
                return pvp;
            }
        }

        public async Task<List<RaidSubscription>> GetSubscriptionsByRaidPokemonId(uint pokemonId)
        {
            /*
            return _subscriptions.Where(x =>
                x.IsEnabled(NotificationStatusType.Raids) &&
                //x.Raids != null &&
                x.Raids.Exists(y => y.PokemonId == pokemonId)
            ).ToList();
            */
            using (var ctx = _dbFactory.CreateDbContext())
            {
                var raids = await ctx.Raids.Where(x => x.PokemonId == pokemonId).ToListAsync();
                return raids;
            }
        }

        public async Task<List<QuestSubscription>> GetSubscriptionsByQuest(string pokestopName, string reward)
        {
            /*
            return _subscriptions.Where(x =>
                x.IsEnabled(NotificationStatusType.Quests) &&
                //x.Quests != null &&
                x.Quests.Exists(y =>
                    reward.Contains(y.RewardKeyword) ||
                    (y.PokestopName != null && (pokestopName.Contains(y.PokestopName) || string.Equals(pokestopName, y.PokestopName, StringComparison.OrdinalIgnoreCase)))
                )
            ).ToList();
            */
            using (var ctx = _dbFactory.CreateDbContext())
            {
                var quests = await ctx.Quests.Where(x =>
                    reward.Contains(x.RewardKeyword) ||
                    (x.PokestopName != null && (pokestopName.Contains(x.PokestopName) || string.Equals(pokestopName, x.PokestopName, StringComparison.OrdinalIgnoreCase))))
                        .ToListAsync();
                return quests;
            }
        }

        public async Task<List<InvasionSubscription>> GetSubscriptionsByInvasion(string pokestopName, InvasionCharacter gruntType, List<uint> encounterRewards)
        {
            /*
            return _subscriptions?
                .Where(x => x.IsEnabled(NotificationStatusType.Invasions) &&
                            //x.Invasions != null &&
                            x.Invasions.Exists(y =>
                                y.RewardPokemonId.Intersects(encounterRewards) ||
                                gruntType == y.InvasionType ||
                                (!string.IsNullOrEmpty(y.PokestopName) && !string.IsNullOrEmpty(pokestopName) && pokestopName.Contains(y.PokestopName)) || string.Equals(pokestopName, y.PokestopName, StringComparison.OrdinalIgnoreCase)
                            )
                        )
                .ToList();
            */
            using (var ctx = _dbFactory.CreateDbContext())
            {
                var invasions = await ctx.Invasions.Where(x =>
                    x.RewardPokemonId.Intersects(encounterRewards) ||
                    gruntType == x.InvasionType ||
                    (!string.IsNullOrEmpty(x.PokestopName) && !string.IsNullOrEmpty(pokestopName) && pokestopName.Contains(x.PokestopName)) || string.Equals(pokestopName, x.PokestopName, StringComparison.OrdinalIgnoreCase)
                ).ToListAsync();
                return invasions;
            }
        }

        // TODO: Pokestop name
        public async Task<List<LureSubscription>> GetSubscriptionsByLure(PokestopLureType lure)
        {
            /*
            return _subscriptions?
                .Where(x => x.IsEnabled(NotificationStatusType.Lures) &&
                            //x.Lures != null &&
                            x.Lures.Exists(y => lure == y.LureType))
                .ToList();
            */
            using (var ctx = _dbFactory.CreateDbContext())
            {
                var lures = await ctx.Lures.Where(x =>
                    lure == x.LureType
                ).ToListAsync();
                return lures;
            }
        }

        public async Task<List<GymSubscription>> GetSubscriptionsByGymName(string name)
        {
            /*
            return _subscriptions?
                .Where(x => x.IsEnabled(NotificationStatusType.Gyms) &&
                            //x.Gyms != null &&
                            x.Gyms.Exists(y => string.Compare(y.Name, name, true) == 0 || y.Name.ToLower().Contains(name.ToLower()))
                        )
                .ToList();
            */
            using (var ctx = _dbFactory.CreateDbContext())
            {
                var gyms = await ctx.Gyms.Where(x =>
                    string.Compare(x.Name, name, true) == 0 || x.Name.ToLower().Contains(name.ToLower())
                ).ToListAsync();
                return gyms;
            }
        }


        /// <summary>
        /// Reload all user subscriptions
        /// </summary>
        public async Task ReloadSubscriptions()
        {
            // TODO: Only reload based on last_changed timestamp in metadata table

            var subs = await GetUserSubscriptions();
            if (subs == null)
                return;

            _subscriptions = subs;
        }
    }
}