namespace WhMgr.Net.Models
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    using Newtonsoft.Json;

    /// <summary>
    /// RealDeviceMap Gym webhook model class.
    /// </summary>
    public sealed class GymData
    {
        public const string WebhookHeader = "gym";

        [
            JsonProperty("gym_id"),
            Column("id"),
            Key
        ]
        public string GymId { get; set; }

        [
            JsonProperty("gym_name"),
            Column("name")
        ]
        public string GymName { get; set; }

        [
            JsonProperty("url"),
            Column("url")
        ]
        public string Url { get; set; }

        [
            JsonProperty("latitude"),
            Column("lat")
        ]
        public double Latitude { get; set; }

        [
            JsonProperty("longitude"),
            Column("lon")
        ]
        public double Longitude { get; set; }

        [
            JsonProperty("enabled"),
            Column("enabled")
        ]
        public bool Enabled { get; set; }

        [
            JsonProperty("team_id"),
            Column("team_id")
        ]
        public PokemonTeam Team { get; set; } = PokemonTeam.Neutral;

        [
            JsonProperty("last_modified"),
            Column("last_modified_timestamp")
        ]
        public ulong LastModified { get; set; }

        [
            JsonProperty("slots_available"),
            Column("availble_slots")
        ]
        public ushort SlotsAvailable { get; set; }

        [
            JsonProperty("sponsor_id"),
            Column("sponsor_id")
        ]
        public bool SponsorId { get; set; }

        [
            JsonProperty("guard_pokemon_id"),
            Column("guarding_pokemon_id")
        ]
        public int GuardPokemonId { get; set; }

        [
            JsonProperty("raid_active_until"),
            Column("raid_end_timestamp")
        ]
        public ulong RaidActiveUntil { get; set; }
    }
}