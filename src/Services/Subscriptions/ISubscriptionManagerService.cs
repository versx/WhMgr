namespace WhMgr.Services.Subscriptions
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using InvasionCharacter = POGOProtos.Rpc.EnumWrapper.Types.InvasionCharacter;

    using WhMgr.Services.Subscriptions.Models;
    using WhMgr.Services.Webhook.Models;

    public interface ISubscriptionManagerService
    {
        Task<List<Subscription>> GetSubscriptionsByPokemonId(uint pokemonId);

        Task<List<Subscription>> GetSubscriptionsByPvpPokemonId(uint pokemonId);

        Task<List<Subscription>> GetSubscriptionsByRaidPokemonId(uint pokemonId);

        Task<List<Subscription>> GetSubscriptionsByQuest(string pokestopName, string reward);

        Task<List<Subscription>> GetSubscriptionsByInvasion(string pokestopName, InvasionCharacter gruntType, List<uint> encounters);

        Task<List<Subscription>> GetSubscriptionsByLure(PokestopLureType lure);

        Task<List<Subscription>> GetSubscriptionsByGymName(string name);
    }
}