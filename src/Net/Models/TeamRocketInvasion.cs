namespace WhMgr.Net.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Newtonsoft.Json;

    using WhMgr.Data;

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

        public string GetPossibleInvasionEncounters()
        {
            var toInt = new Func<string, int>(x =>
            {
                var val = x.Split('_')[0];
                if (!int.TryParse(val, out var result))
                {
                    Console.Error.WriteLine($"Failed to parse {val} as integer");
                }
                return result;
            });
            var first = string.Join(", ", Encounters.First.Select(x => MasterFile.GetPokemon(toInt(x), 0)?.Name));
            var second = string.Join(", ", Encounters.Second.Select(x => MasterFile.GetPokemon(toInt(x), 0)?.Name));
            //var third = string.Join(", ", invasion.Encounters.Third.Select(x => Database.Instance.Pokemon[x].Name));
            var msg = string.Empty;
            if (SecondReward)
            {
                //85%/15% Rate
                msg += $"85% - {first}\r\n";
                msg += $"15% - {second}\r\n";
            }
            else
            {
                //100% Rate
                msg += $"100% - {first}\r\n";
            }
            return msg;
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