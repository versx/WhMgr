namespace WhMgr.Services.Subscriptions
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using InvasionCharacter = POGOProtos.Rpc.EnumWrapper.Types.InvasionCharacter;

    using WhMgr.Common;
    using WhMgr.Services.Subscriptions.Models;

    public interface ISubscriptionManagerService
    {
        IReadOnlyList<Subscription> Subscriptions { get; }

        Task<Subscription> GetUserSubscriptionsAsync(ulong guildId, ulong userId);

        List<Subscription> GetSubscriptionsByPokemonId(uint pokemonId);

        List<Subscription> GetSubscriptionsByPvpPokemonId(uint pokemonId);

        List<Subscription> GetSubscriptionsByRaidPokemonId(uint pokemonId);

        List<Subscription> GetSubscriptionsByQuest(string pokestopName, string reward);

        List<Subscription> GetSubscriptionsByInvasion(string pokestopName, InvasionCharacter gruntType, List<uint> encounters);

        List<Subscription> GetSubscriptionsByLure(string pokestopName, PokestopLureType lure);

        List<Subscription> GetSubscriptionsByGymName(string name);

        Task ReloadSubscriptionsAsync();

        bool Save(Subscription subscription);
    }
}