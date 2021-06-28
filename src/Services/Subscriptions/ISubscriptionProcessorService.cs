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

        Task ProcessPokestopSubscription(PokestopData pokestop);
    }
}