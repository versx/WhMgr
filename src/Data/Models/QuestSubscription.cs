namespace WhMgr.Data.Models
{
    using ServiceStack.DataAnnotations;

    [Alias("quests")]
    public class QuestSubscription
    {
        [Alias("id"), PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Alias("userId"), ForeignKey(typeof(SubscriptionObject))]
        public ulong UserId { get; set; }

        [Alias("reward"), Required]
        public string RewardKeyword { get; set; }

        [Alias("city"), Required]
        public string City { get; set; }
    }
}