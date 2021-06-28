namespace WhMgr.Services.Subscriptions
{
    using System.Threading.Tasks;

    using WhMgr.Services.Webhook.Models;

    public interface ISubscriptionProcessorService
    {
        Task ProcessPokemonSubscription(PokemonData pokemon);

        Task ProcessPvpSubscription(PokemonData pokemon);

        Task ProcessRaidSubscription(RaidData raid);

        Task ProcessQuestSubscription(QuestData quest);

        Task ProcessInvasionSubscription(PokestopData pokestop);

        Task ProcessLureSubscription(PokestopData pokestop);

        Task ProcessGymSubscription(RaidData raid);
    }
}