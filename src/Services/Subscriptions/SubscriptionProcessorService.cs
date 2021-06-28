namespace WhMgr.Services.Subscriptions
{
    using System.Collections.Generic;

    using WhMgr.Services.Webhook;
    using WhMgr.Services.Webhook.Models;

    public class SubscriptionProcessorService : ISubscriptionProcessorService
    {
        public void ParseData(List<WebhookPayload> payloads)
        {
            for (var i = 0; i < payloads.Count; i++)
            {
                var payload = payloads[i];
                switch (payload.Type)
                {
                    case WebhookHeaders.Pokemon:
                        ProcessPokemon(payload.Message);
                        break;
                    case WebhookHeaders.Raid:
                    case WebhookHeaders.Quest:
                    case WebhookHeaders.Invasion:
                    case WebhookHeaders.Pokestop:
                        break;
                        // TODO: Gym
                        // TODO: Weather
                }
            }
        }

        public void ProcessPokemon(PokemonData pokemon)
        {
        }

        public void ProcessPvpPokemon(PokemonData pokemon)
        {
        }

        public void ProcessRaidPokemon(RaidData raid)
        {
        }

        // TODO: Quest

        // TODO: Invasion
        
        // TODO: Lure

        // TODO: Gym
    }
}