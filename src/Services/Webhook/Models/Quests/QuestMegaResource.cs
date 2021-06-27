namespace WhMgr.Services.Webhook.Models.Quests
{
    using System.Text.Json.Serialization;

    public sealed class QuestMegaResource
    {
        [JsonPropertyName("pokemon_id")]
        public ushort PokemonId { get; set; }

        [JsonPropertyName("amount")]
        public int Amount { get; set; }
    }
}