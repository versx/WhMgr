namespace WhMgr.Data.Subscriptions.Models
{
    using System;

    using ServiceStack.DataAnnotations;

    [Alias("quest_stats")]
    public class QuestStatistics
    {
        [Alias("id"), PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Alias("userId"), ForeignKey(typeof(SubscriptionObject))]
        public ulong UserId { get; set; }

        [Alias("date")]
        public DateTime Date { get; set; }

        [Alias("reward")]
        public string Reward { get; set; }

        [Alias("lat")]
        public double Latitude { get; set; }

        [Alias("lon")]
        public double Longitude { get; set; }
    }
}