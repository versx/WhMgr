namespace WhMgr.Data.Subscriptions.Models
{
    using ServiceStack.DataAnnotations;

    [Alias("gyms")]
    public class GymSubscription
    {
        [Alias("id"), PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Alias("userId"), ForeignKey(typeof(SubscriptionObject))]
        public ulong UserId { get; set; }

        [Alias("name"), Unique]
        public string Name { get; set; }
    }
}