namespace WhMgr.Services.Subscriptions
{
    using WhMgr.Services.Webhook.Models;

    public interface ISubscriptionProcessor
    {
        void ProcessPokemon(PokemonData pokemon);

        void ProcessPvpPokemon(PokemonData pokemon);

        void ProcessRaidPokemon(RaidData raid);
    }
}