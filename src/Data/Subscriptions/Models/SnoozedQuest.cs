namespace WhMgr.Data.Subscriptions.Models
{
    using System;

    using ServiceStack.DataAnnotations;
    
    using WhMgr.Net.Models;

    [Alias("snoozed_quest")]
    public class SnoozedQuest
    {
        //TODO: Request by reward or all.

        [Alias("date")]
        public DateTime Date { get; set; }

        [Alias("id"), PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Alias("userId"), ForeignKey(typeof(SubscriptionObject))]
        public ulong UserId { get; set; }

        [Alias("latitude"), Default(0)]
        public double Latitude { get; set; }

        [Alias("longitude"), Default(0)]
        public double Longitude { get; set; }

        [Alias("pokestop_name")]
        public string PokestopName { get; set; }

        [Alias("quest"), Required]
        public string Quest { get; set; }

        [Alias("reward"), Required]
        public string Reward { get; set; }

        [Alias("reward_type"), Required]
        public QuestRewardType RewardType { get; set; }

        [Alias("condition")]
        public string Condition { get; set; }

        [Alias("icon_url")]
        public string IconUrl { get; set; }

        [Alias("city"), Required]
        public string City { get; set; }

        [Alias("requested")]
        public bool Requested { get; set; }

        [Ignore]
        public TimeSpan TimeLeft => DateTime.Today.AddDays(1) - DateTime.Now;
    }
}