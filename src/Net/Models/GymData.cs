namespace WhMgr.Net.Models
{
    using Newtonsoft.Json;

    public sealed class GymData
    {
        public const string WebhookHeader = "gym";

        [JsonProperty("gym_id")]
        public string GymId { get; set; }

        [JsonProperty("gym_name")]
        public string GymName { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("latitude")]
        public double Latitude { get; set; }

        [JsonProperty("longitude")]
        public double Longitude { get; set; }

        [JsonProperty("enabled")]
        public bool Enabled { get; set; }

        [JsonProperty("team_id")]
        public PokemonTeam Team { get; set; } = PokemonTeam.Neutral;

        [JsonProperty("last_modified")]
        public ulong LastModified { get; set; }

        [JsonProperty("slots_available")]
        public ushort SlotsAvailable { get; set; }

        [JsonProperty("sponsor_id")]
        public bool SponsorId { get; set; }

        [JsonProperty("guard_pokemon_id")]
        public int GuardPokemonId { get; set; }

        [JsonProperty("raid_active_until")]
        public ulong RaidActiveUntil { get; set; }
    }
}