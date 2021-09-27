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
        public TeamRocketEncounters Encounters { get; set; } = new();

        [JsonIgnore]
        public bool HasEncounter => Encounters?.First?.Count > 0 || Encounters?.Second?.Count > 0 || Encounters?.Third?.Count > 0;

        public List<dynamic> GetPossibleInvasionEncounters()
        {
            var first = string.Join(", ", Encounters.First.Select(id => MasterFile.GetPokemon(id)?.Name));
            var second = string.Join(", ", Encounters.Second.Select(id => MasterFile.GetPokemon(id)?.Name));
            var msg = string.Empty;
            if (SecondReward ?? false)
            {
                // 85%/15% Rate
                return new List<dynamic>
                {
                    new { chance = "85%", pokemon = first, },
                    new { chance = "15%", pokemon = second, },
                };
            }
            return new List<dynamic>
            {
                new { chance = "100%", pokemon = first, },
            };
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
        public List<uint> First { get; set; } = new();

        [JsonPropertyName("second")]
        public List<uint> Second { get; set; } = new();

        [JsonPropertyName("third")]
        public List<uint> Third { get; set; } = new();
    }
}