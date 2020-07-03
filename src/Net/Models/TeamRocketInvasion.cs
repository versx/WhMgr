namespace WhMgr.Net.Models
{
    using System;
    using System.Collections.Generic;

    using Newtonsoft.Json;

    public class TeamRocketInvasion
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("grunt")]
        public string Grunt { get; set; }

        [JsonProperty("second_reward")]
        public bool SecondReward { get; set; }

        [JsonProperty("encounters")]
        public TeamRocketEncounters Encounters { get; set; }

        [JsonIgnore]
        public bool HasEncounter => Encounters?.First?.Count > 0 || Encounters?.Second?.Count > 0 || Encounters?.Third?.Count > 0;

        public TeamRocketInvasion()
        {
            Encounters = new TeamRocketEncounters();
        }
    }

    public class TeamRocketEncounters
    {
        [JsonProperty("first")]
        public List<string> First { get; set; }

        [JsonProperty("second")]
        public List<string> Second { get; set; }

        [JsonProperty("third")]
        public List<string> Third { get; set; }

        public TeamRocketEncounters()
        {
            First = new List<string>();
            Second = new List<string>();
            Third = new List<string>();
        }
    }
}