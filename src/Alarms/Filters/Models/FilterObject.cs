namespace WhMgr.Alarms.Filters.Models
{
    using Newtonsoft.Json;

    [JsonObject("filter")]
    public class FilterObject
    {
        [JsonProperty("pokemon")]
        public FilterPokemonObject Pokemon { get; set; }

        [JsonProperty("raids")]
        public FilterRaidObject Raids { get; set; }

        [JsonProperty("eggs")]
        public FilterEggObject Eggs { get; set; }

        [JsonProperty("quests")]
        public FilterQuestObject Quests { get; set; }
    }
}