namespace WhMgr.Services.Webhook.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json.Serialization;

    using WhMgr.Data;

    public class TeamRocketInvasion
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("grunt")]
        public string Grunt { get; set; }

        [JsonPropertyName("second_reward")]
        public bool? SecondReward { get; set; }

        [JsonPropertyName("encounters")]
        public TeamRocketEncounters Encounters { get; set; }

        [JsonIgnore]
        public bool HasEncounter => Encounters?.First?.Count > 0 || Encounters?.Second?.Count > 0 || Encounters?.Third?.Count > 0;

        public TeamRocketInvasion()
        {
            Encounters = new TeamRocketEncounters();
        }

        public List<dynamic> GetPossibleInvasionEncounters()
        {
            var first = string.Join(", ", Encounters.First.Select(id => MasterFile.GetPokemon(id, 0)?.Name));
            var second = string.Join(", ", Encounters.Second.Select(id => MasterFile.GetPokemon(id, 0)?.Name));
            //var third = string.Join(", ", invasion.Encounters.Third.Select(x => Database.Instance.Pokemon[x].Name));
            var msg = string.Empty;
            if (SecondReward ?? false)
            {
                //85%/15% Rate
                return new List<dynamic>
                {
                    new { chance = "85%", pokemon = first, },
                    new { chance = "15%", pokemon = second, },
                };
                //msg += $"85% - {first}\r\n";
                //msg += $"15% - {second}\r\n";
            }
            return new List<dynamic>
            {
                new { chance = "100%", pokemon = first, },
            };
            /*
            else
            {
                //100% Rate
                msg += $"100% - {first}\r\n";
            }
            return msg;
            */
        }

        public List<uint> GetEncounterRewards()
        {
            var list = new List<uint>();
            if (Encounters == null)
                return list;

            if (SecondReward ?? false)
            {
                // 85%/15% Rate
                list.AddRange(Encounters.Second);
            }
            else
            {
                // 100% Rate
                list.AddRange(Encounters.First);
            }
            return list;
        }
    }

    public class TeamRocketEncounters
    {
        [JsonPropertyName("first")]
        public List<uint> First { get; set; }

        [JsonPropertyName("second")]
        public List<uint> Second { get; set; }

        [JsonPropertyName("third")]
        public List<uint> Third { get; set; }

        public TeamRocketEncounters()
        {
            First = new List<uint>();
            Second = new List<uint>();
            Third = new List<uint>();
        }
    }
}