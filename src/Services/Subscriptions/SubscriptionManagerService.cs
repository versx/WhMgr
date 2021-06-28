namespace WhMgr.Services.Subscriptions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    using WhMgr.Data.Contexts;
    using WhMgr.Services.Subscriptions.Models;

    public class SubscriptionManagerService : ISubscriptionManagerService
    {
        private readonly ILogger<ISubscriptionManagerService> _logger;
        private readonly AppDbContext _dbContext;

        public SubscriptionManagerService(
            ILogger<ISubscriptionManagerService> logger,
            AppDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
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

        // TODO: Gyms
        // TODO: Invasions
        // TODO: Lures
    }
}