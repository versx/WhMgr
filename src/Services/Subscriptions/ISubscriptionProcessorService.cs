namespace WhMgr.Services.Subscriptions
{
    using System.Collections.Generic;

    using WhMgr.Services.Webhook;
    using WhMgr.Services.Webhook.Models;

    public interface ISubscriptionProcessorService
    {
        void ProcessPokemon(PokemonData pokemon);

        void ProcessPvpPokemon(PokemonData pokemon);

        void ProcessRaidPokemon(RaidData raid);
    }
}