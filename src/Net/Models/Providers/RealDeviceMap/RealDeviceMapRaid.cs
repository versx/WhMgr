namespace WhMgr.Net.Models.Providers.RealDeviceMap
{
    using System;

    using Newtonsoft.Json;

    using WhMgr.Extensions;

    public class RealDeviceMapRaid : IMapProviderRaid
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

        public RealDeviceMapRaid()
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