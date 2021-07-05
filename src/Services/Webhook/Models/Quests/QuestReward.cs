namespace WhMgr.Services.Webhook.Models.Quests
{
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    using POGOProtos.Rpc;

    public sealed class QuestReward
    {
        [JsonPropertyName("pokemon_id")]
        public uint PokemonId { get; set; }

        [JsonPropertyName("costume_id")]
        public uint CostumeId { get; set; }

        [JsonPropertyName("form_id")]
        public uint FormId { get; set; }

        [JsonPropertyName("gender_id")]
        public int GenderId { get; set; }

        [JsonPropertyName("ditto")]
        public bool Ditto { get; set; }

        [JsonPropertyName("shiny")]
        public bool Shiny { get; set; }

        [JsonPropertyName("amount")]
        public int Amount { get; set; }

        [JsonPropertyName("item_id")]
        public Item Item { get; set; }

        [JsonPropertyName("raid_levels")]
        public List<int> RaidLevels { get; set; }

        [JsonPropertyName("mega_resource")]
        public QuestMegaResource MegaResource { get; set; }

        [JsonPropertyName("sticker_id")]
        public string StickerId { get; set; }

        // TODO: Pokemon alignment
    }
}