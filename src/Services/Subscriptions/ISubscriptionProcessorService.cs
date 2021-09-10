namespace WhMgr.Services.Subscriptions
{
    using System.Threading.Tasks;

    using WhMgr.Services.Webhook.Models;

    public interface ISubscriptionProcessorService
    {
        Task ProcessPokemonSubscriptionAsync(PokemonData pokemon);

        Task ProcessPvpSubscriptionAsync(PokemonData pokemon);

        Task ProcessRaidSubscriptionAsync(RaidData raid);

        Task ProcessQuestSubscriptionAsync(QuestData quest);

        Task ProcessInvasionSubscriptionAsync(PokestopData pokestop);

        Task ProcessLureSubscriptionAsync(PokestopData pokestop);

        Task ProcessGymSubscriptionAsync(RaidData raid);
    }
}