namespace WhMgr.Net.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Newtonsoft.Json;

    using WhMgr.Data;
    using WhMgr.Diagnostics;

    public class TeamRocketInvasion
    {
        private static readonly IEventLogger _logger = EventLogger.GetLogger("TR-INVASION", Program.LogLevel);

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("grunt")]
        public string Grunt { get; set; }

        [JsonProperty("second_reward")]
        public bool? SecondReward { get; set; }

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
            var first = string.Join(", ", Encounters.First.Select(x => MasterFile.GetPokemon(x, 0)?.Name));
            var second = string.Join(", ", Encounters.Second.Select(x => MasterFile.GetPokemon(x, 0)?.Name));
            //var third = string.Join(", ", invasion.Encounters.Third.Select(x => Database.Instance.Pokemon[x].Name));
            var msg = string.Empty;
            if (SecondReward ?? false)
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

        public List<uint> GetEncounterRewards()
        {
            var list = new List<uint>();
            if (Encounters == null)
                return list;

            if (SecondReward ?? false)
            {
                //85%/15% Rate
                for (var i = 0; i < Encounters.Second.Count; i++)
                {
                    var mon = Encounters.Second[i];
                    if (mon == 0)
                        continue;

                    list.Add(mon);
                }
            }
            else
            {
                //100% Rate
                for (var i = 0; i < Encounters.First.Count; i++)
                {
                    var mon = Encounters.First[i];
                    if (mon == 0)
                        continue;

                    list.Add(mon);
                }
            }
            return list;
        }

    }

    public class TeamRocketEncounters
    {
        [JsonProperty("first")]
        public List<uint> First { get; set; }

        [JsonProperty("second")]
        public List<uint> Second { get; set; }

        [JsonProperty("third")]
        public List<uint> Third { get; set; }

        public TeamRocketEncounters()
        {
            First = new List<uint>();
            Second = new List<uint>();
            Third = new List<uint>();
        }
    }
}