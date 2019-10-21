namespace WhMgr.Net.Models
{
    using System;
    using System.Collections.Generic;

    using Newtonsoft.Json;

    using WhMgr.Extensions;

    /*
[
    {
        "type":"raid",
        "message":
        {
            "end":1541647095,
            "latitude":34.070584,
            "level":3,
            "pokemon_id":210,
            "move_2":279,
            "is_exclusive":false,
            "sponsor_id":false,
            "cp":15328,
            "form":0,
            "move_1":202,
            "spawn":1541640795,
            "start":1541644395,
            "gym_id":"efa2c34f8679419fb508545a504735e1.16",
            "team_id":3,
            "gym_name":"Unknown",
            "longitude":-117.566666
        }
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

        [JsonProperty("gym_url")]
        public string GymUrl { get; set; }

        [JsonProperty("pokemon_id")]
        public int PokemonId { get; set; }

        [JsonProperty("team_id")]
        public PokemonTeam Team { get; set; } = PokemonTeam.Neutral;

        [JsonProperty("level")]
        public string Level { get; set; }

        [JsonProperty("cp")]
        public string CP { get; set; }

        [JsonProperty("move_1")]
        public int FastMove { get; set; }

        [JsonProperty("move_2")]
        public int ChargeMove { get; set; }

        [JsonProperty("latitude")]
        public double Latitude { get; set; }

        [JsonProperty("longitude")]
        public double Longitude { get; set; }

        [JsonProperty("start")]
        public long Start { get; set; }

        [JsonProperty("end")]
        public long End { get; set; }

        //[JsonProperty("is_exclusive")]
        //public bool IsExclusive { get; set; }

        [JsonProperty("ex_raid_eligible")]
        public bool IsExEligible { get; set; }

        [JsonProperty("sponsor_id")]
        public bool SponsorId { get; set; }

        [JsonProperty("form")]
        public int Form { get; set; }

        [JsonProperty("gender")]
        public PokemonGender Gender { get; set; }

        [JsonIgnore]
        public DateTime StartTime { get; private set; }

        [JsonIgnore]
        public DateTime EndTime { get; private set; }

        [JsonIgnore]
        public bool IsEgg => PokemonId == 0;

        [JsonIgnore]
        public List<PokemonType> Weaknesses
        {
            get
            {
                var db = Data.Database.Instance;
                if (db.Pokemon.ContainsKey(PokemonId) && !IsEgg)
                {
                    var list = new List<PokemonType>();
                    db.Pokemon[PokemonId].Types.ForEach(x => x.GetWeaknesses().ForEach(y => list.Add(y)));
                    return list;
                }

                return null;
            }
        }

        [JsonIgnore]
        public bool IsMissingStats => FastMove == 0;

        public RaidData()
        {
            SetTimes();
        }

        public void SetTimes()
        {
            StartTime = Start.FromUnix();
            //if (TimeZoneInfo.Local.IsDaylightSavingTime(StartTime))
            //{
            //    StartTime = StartTime.AddHours(1); //DST
            //}

            EndTime = End.FromUnix();
            //if (TimeZoneInfo.Local.IsDaylightSavingTime(EndTime))
            //{
            //    EndTime = EndTime.AddHours(1); //DST
            //}
        }
    }
}