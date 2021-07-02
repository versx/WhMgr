namespace WhMgr.Services.Subscriptions
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using InvasionCharacter = POGOProtos.Rpc.EnumWrapper.Types.InvasionCharacter;

    using WhMgr.Services.Subscriptions.Models;
    using WhMgr.Services.Webhook.Models;

    public interface ISubscriptionManagerService
    {
        //Task<List<Subscription>> GetSubscriptionsByPokemonId(uint pokemonId);
        Task<List<PokemonSubscription>> GetSubscriptionsByPokemonId(uint pokemonId);

        Task<List<PvpSubscription>> GetSubscriptionsByPvpPokemonId(uint pokemonId);

        Task<List<RaidSubscription>> GetSubscriptionsByRaidPokemonId(uint pokemonId);

        Task<List<QuestSubscription>> GetSubscriptionsByQuest(string pokestopName, string reward);

        Task<List<InvasionSubscription>> GetSubscriptionsByInvasion(string pokestopName, InvasionCharacter gruntType, List<uint> encounters);

        Task<List<LureSubscription>> GetSubscriptionsByLure(PokestopLureType lure);

        Task<List<GymSubscription>> GetSubscriptionsByGymName(string name);
    }
}