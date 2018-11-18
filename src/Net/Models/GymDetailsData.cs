namespace WhMgr.Net.Models
{
    using Newtonsoft.Json;

    public sealed class GymDetailsData
    {
        public const string WebhookHeader = "gym_details";

        [JsonProperty("id")]
        public string GymId { get; set; }

        [JsonProperty("name")]
        public string GymName { get; set; } = "Unknown";

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("latitude")]
        public double Latitude { get; set; }

        [JsonProperty("longitude")]
        public double Longitude { get; set; }

        [JsonProperty("team")]
        public PokemonTeam Team { get; set; } = PokemonTeam.Neutral;

        [JsonProperty("slots_available")]
        public ushort SlotsAvailable { get; set; }

        [JsonProperty("sponsor_id")]
        public bool SponsorId { get; set; }

        [JsonProperty("in_battle")]
        public bool InBattle { get; set; }
    }
}