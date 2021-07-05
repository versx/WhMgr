namespace WhMgr.Services.Subscriptions
{
    using System.Collections.Generic;

    using InvasionCharacter = POGOProtos.Rpc.EnumWrapper.Types.InvasionCharacter;

    using WhMgr.Common;
    using WhMgr.Services.Subscriptions.Models;

    public interface ISubscriptionManagerService
    {
        List<Subscription> GetSubscriptionsByPokemonId(uint pokemonId);

        List<Subscription> GetSubscriptionsByPvpPokemonId(uint pokemonId);

        List<Subscription> GetSubscriptionsByRaidPokemonId(uint pokemonId);

        List<Subscription> GetSubscriptionsByQuest(string pokestopName, string reward);

        List<Subscription> GetSubscriptionsByInvasion(string pokestopName, InvasionCharacter gruntType, List<uint> encounters);

        List<Subscription> GetSubscriptionsByLure(PokestopLureType lure);

        List<Subscription> GetSubscriptionsByGymName(string name);
    }
}