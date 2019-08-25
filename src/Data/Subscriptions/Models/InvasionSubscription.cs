namespace WhMgr.Data.Subscriptions.Models
{
    using ServiceStack.DataAnnotations;

    using WhMgr.Net.Models;

    [Alias("invasions")]
    public class InvasionSubscription
    {
        [Alias("id"), PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Alias("userId"), ForeignKey(typeof(SubscriptionObject))]
        public ulong UserId { get; set; }

        [Alias("grunt_type"), Required]
        public InvasionGruntType GruntType { get; set; }

        [Alias("city"), Required]
        public string City { get; set; }
    }
}