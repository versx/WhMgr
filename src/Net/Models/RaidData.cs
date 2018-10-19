namespace T.Net.Models
{
    using System;

    using Newtonsoft.Json;

    using T.Extensions;

    /*
    {
        "type":"raid",
        "message":
        {
            "level":5,
            "spawn":1539192227,
            "end":1539198527,
            "pokemon_id":0,
            "longitude":-117.750491,
            "gym_id":"78cff8e3e34b48ddad0d08af322106a2.16",
            "cp":0,
            "move_1":0,
            "gym_name":"Unknown",
            "team_id":2,
            "start":1539195827,
            "move_2":0,
            "latitude":34.062143
        }
     }
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

        public RaidData()
        {
            SetTimes();
        }

        public void SetTimes()
        {
            StartTime = Start.FromUnix().Subtract(TimeSpan.FromHours(1));
            EndTime = End.FromUnix().Subtract(TimeSpan.FromHours(1));
        }

        //public RaidData(string gymId, int pokemonId, PokemonTeam team, string level, string cp, string move1, string move2, double lat, double lng, DateTime startTime, DateTime endTime)
        //{
        //    GymId = gymId;
        //    PokemonId = pokemonId;
        //    Team = team;
        //    Level = level;
        //    CP = cp;
        //    FastMove = move1;
        //    ChargeMove = move2;
        //    Latitude = lat;
        //    Longitude = lng;
        //    StartTime = startTime;
        //    EndTime = endTime;
        //}
    }

    /*
[
{
    "message": 
    {
        "gym_name": "First Church of Nazarene Ontario", 
        "latitude": 34.081156,
        "longitude": -117.675116, 
        "level": 4, 
        "pokemon_id": 0,
        "raid_end": 1538842500,
        "raid_begin": 1538839800, 
        "cp": 0, 
        "move_1": 0, 
        "move_2": 0,
        "gym_id": "bd8b2926f06a4fa3a462017a4af78e38.16",
        "team_id": 0
    },
    "type": "raid"
}
]
     */
}