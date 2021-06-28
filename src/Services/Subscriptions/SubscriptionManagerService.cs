namespace WhMgr.Services.Subscriptions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using InvasionCharacter = POGOProtos.Rpc.EnumWrapper.Types.InvasionCharacter;

    using WhMgr.Data.Contexts;
    using WhMgr.Extensions;
    using WhMgr.Services.Subscriptions.Models;
    using WhMgr.Services.Webhook.Models;

    public class SubscriptionManagerService : ISubscriptionManagerService
    {
        private readonly ILogger<ISubscriptionManagerService> _logger;
        private readonly IDbContextFactory<AppDbContext> _dbFactory;
        private readonly AppDbContext _dbContext;

        public SubscriptionManagerService(
            ILogger<ISubscriptionManagerService> logger,
            IDbContextFactory<AppDbContext> dbFactory)
        {
            _logger = logger;
            _dbFactory = dbFactory;
            _dbContext = _dbFactory.CreateDbContext();
        }

        public async Task<List<Subscription>> GetSubscriptionsByPokemonId(uint pokemonId)
        {
            return await _dbContext.Subscriptions.Where(x =>
                x.IsEnabled(NotificationStatusType.Pokemon) &&
                x.Pokemon != null &&
                x.Pokemon.Exists(y => y.PokemonId.Contains(pokemonId))
            ).ToListAsync();
        }

        public async Task<List<Subscription>> GetSubscriptionsByPvpPokemonId(uint pokemonId)
        {
            return await _dbContext.Subscriptions.Where(x =>
                x.IsEnabled(NotificationStatusType.PvP) &&
                x.PvP != null &&
                x.PvP.Exists(y => y.PokemonId == pokemonId)
            ).ToListAsync();
        }

        public async Task<List<Subscription>> GetSubscriptionsByRaidPokemonId(uint pokemonId)
        {
            return await _dbContext.Subscriptions.Where(x =>
                x.IsEnabled(NotificationStatusType.Raids) &&
                x.Raids != null &&
                x.Raids.Exists(y => y.PokemonId == pokemonId)
            ).ToListAsync();
        }

        public async Task<List<Subscription>> GetSubscriptionsByQuest(string pokestopName, string reward)
        {
            return await _dbContext.Subscriptions.Where(x =>
                x.IsEnabled(NotificationStatusType.Quests) &&
                x.Quests != null &&
                x.Quests.Exists(y =>
                    reward.Contains(y.RewardKeyword) ||
                    (y.PokestopName != null && (pokestopName.Contains(y.PokestopName) || string.Equals(pokestopName, y.PokestopName, StringComparison.OrdinalIgnoreCase)))
                )
            ).ToListAsync();
        }

        public async Task<List<Subscription>> GetSubscriptionsByInvasion(string pokestopName, InvasionCharacter gruntType, List<uint> encounterRewards)
        {
            return await _dbContext.Subscriptions?
                .Where(x => x.IsEnabled(NotificationStatusType.Invasions) &&
                            x.Invasions != null &&
                            x.Invasions.Exists(y =>
                                y.RewardPokemonId.Intersects(encounterRewards) ||
                                gruntType == y.InvasionType ||
                                (!string.IsNullOrEmpty(y.PokestopName) && !string.IsNullOrEmpty(pokestopName) && pokestopName.Contains(y.PokestopName)) || string.Equals(pokestopName, y.PokestopName, StringComparison.OrdinalIgnoreCase)
                            )
                      )
                .ToListAsync();
        }

        // TODO: Pokestop name
        public async Task<List<Subscription>> GetSubscriptionsByLure(PokestopLureType lure)
        {
            return await _dbContext.Subscriptions?
                .Where(x => x.IsEnabled(NotificationStatusType.Lures) &&
                            x.Lures != null &&
                            x.Lures.Exists(y => lure == y.LureType))
                .ToListAsync();
        }

        public async Task<List<Subscription>> GetSubscriptionsByGymName(string name)
        {
            return await _dbContext.Subscriptions?
                .Where(x => x.IsEnabled(NotificationStatusType.Gyms) &&
                            x.Gyms != null &&
                            x.Gyms.Exists(y => string.Compare(y.Name, name, true) == 0 || y.Name.ToLower().Contains(name.ToLower()))
                       )
                .ToListAsync();
        }
    }
}