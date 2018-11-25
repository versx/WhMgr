namespace WhMgr.Data.Models
{
    using System;

    using ServiceStack.DataAnnotations;

    using WhMgr.Net.Models;

    [Alias("quest_stats")]
    public class QuestStats
    {
        [Alias("date")]
        public DateTime Date { get; set; }

        [Alias("reward_type")]
        public QuestRewardType RewardType { get; set; }

        [Alias("pokemon_id")]
        public int PokemonId { get; set; }

        [Alias("item_id")]
        public ItemId ItemId { get; set; }

        [Alias("count")]
        public long Count { get; set; }
    }
}