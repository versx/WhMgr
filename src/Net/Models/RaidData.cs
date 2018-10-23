namespace WhMgr.Net.Models
{
    using System;

    using Newtonsoft.Json;

    using WhMgr.Extensions;

    /*
[
    {
        "message": 
        {
            "gym_name": "Cypress Avenue Park",
            "latitude": 34.052729,
            "longitude": -117.663367,
            "level": 2,
            "pokemon_id": 0,
            "raid_end": 1538842380,
            "raid_begin": 1538839680,
            "cp": 0, 
            "move_1": 0,
            "move_2": 0, 
            "gym_id": "0aeca5201bda4602a26bd2afba855149.16",
            "team_id": 0
        }, 
        "type": "raid"
    }
]
     */

    public sealed class RaidData
    {
        public const string WebHookHeader = "raid";

        [JsonProperty("gym_id")]
        public string GymId { get; set; }

        [JsonProperty("gym_name")]
        public string GymName { get; set; }

        [JsonProperty("pokemon_id")]
        public int PokemonId { get; set; }

        [JsonProperty("team_id")]
        public PokemonTeam Team { get; set; } = PokemonTeam.Neutral;

        [JsonProperty("level")]
        public string Level { get; set; }

        [JsonProperty("cp")]
        public string CP { get; set; }

        [JsonProperty("move_1")]
        public string FastMove { get; set; }

        [JsonProperty("move_2")]
        public string ChargeMove { get; set; }

        [JsonProperty("latitude")]
        public double Latitude { get; set; }

        [JsonProperty("longitude")]
        public double Longitude { get; set; }

        [JsonProperty("start")]
        public long Start { get; set; }

        [JsonProperty("end")]
        public long End { get; set; }

        [JsonIgnore]
        public DateTime StartTime { get; private set; }

        [JsonIgnore]
        public DateTime EndTime { get; private set; }

        [JsonIgnore]
        public bool IsEgg => PokemonId == 0;

        [JsonIgnore]
        public bool IsMissingStats
        {
            get
            {
                return string.IsNullOrEmpty(FastMove) ||
                       FastMove == "?" ||
                       string.IsNullOrEmpty(ChargeMove) ||
                       ChargeMove == "?" ||
                       string.IsNullOrEmpty(Level) ||
                       Level == "?";
            }
        }

        public RaidData()
        {
            SetTimes();
        }

        public void SetTimes()
        {
            StartTime = Start.FromUnix();
            EndTime = End.FromUnix();
        }
    }
}