namespace WhMgr.Services.Subscriptions
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using InvasionCharacter = POGOProtos.Rpc.EnumWrapper.Types.InvasionCharacter;

    using WhMgr.Common;
    using WhMgr.Services.Subscriptions.Models;
    using WhMgr.Web.Api.Requests.Pokemon;

    public interface ISubscriptionManagerService
    {
        IReadOnlyList<Subscription> Subscriptions { get; }

        Task SetSubscriptionStatusAsync(Subscription subscription, NotificationStatusType status);

        Task<List<Subscription>> GetUserSubscriptionsAsync();

        Subscription GetUserSubscriptions(ulong guildId, ulong userId);

        #region Pokemon Subscriptions

        List<Subscription> GetSubscriptionsByPokemonId(uint pokemonId);

        Task<bool> CreatePokemonSubscription(PokemonSubscription subscription);

        #endregion


        TEntity FindById<TEntity>(int id) where TEntity : BaseSubscription;

        Task<TEntity> FindByIdAsync<TEntity>(int id) where TEntity : BaseSubscription;


        List<Subscription> GetSubscriptionsByPvpPokemonId(uint pokemonId);

        List<Subscription> GetSubscriptionsByPvpPokemonId(List<uint> pokemonId);

        List<Subscription> GetSubscriptionsByRaidPokemonId(uint pokemonId);

        List<Subscription> GetSubscriptionsByQuest(string pokestopName, string reward);

        List<Subscription> GetSubscriptionsByInvasion(string pokestopName, InvasionCharacter gruntType, List<uint> encounters);

        List<Subscription> GetSubscriptionsByLure(string pokestopName, PokestopLureType lure);

        List<Subscription> GetSubscriptionsByGymName(string name);

        Task ReloadSubscriptionsAsync(bool skipCheck = false, ushort reloadM = 5);

        bool Save(Subscription subscription);
    }
}