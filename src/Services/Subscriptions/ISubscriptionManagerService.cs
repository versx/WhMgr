namespace WhMgr.Services.Subscriptions
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using WhMgr.Services.Subscriptions.Models;

    public interface ISubscriptionManagerService
    {
        Task<List<Subscription>> GetSubscriptionsByPokemonId(uint pokemonId);

        Task<List<Subscription>> GetSubscriptionsByPvpPokemonId(uint pokemonId);

        Task<List<Subscription>> GetSubscriptionsByRaidPokemonId(uint pokemonId);
    }
}