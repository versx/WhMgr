namespace WhMgr.Services.Subscriptions
{
    using WhMgr.Services.Webhook.Models;

    public interface ISubscriptionProcessorService
    {
        void ProcessPokemonSubscription(PokemonData pokemon);

        void ProcessPvpSubscription(PokemonData pokemon);

        void ProcessRaidSubscription(RaidData raid);

        void ProcessQuestSubscription(QuestData quest);

        void ProcessPokestopSubscription(PokestopData pokestop);
    }
}