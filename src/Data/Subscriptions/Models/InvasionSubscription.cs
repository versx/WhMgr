namespace WhMgr.Data.Subscriptions.Models
{
    using ServiceStack.DataAnnotations;

    using Newtonsoft.Json;

    using WhMgr.Net.Models;

    [
        JsonObject("invasions"),
        Alias("invasions")
    ]
    public class InvasionSubscription
    {
        [
            JsonIgnore,
            Alias("id"),
            PrimaryKey, 
            AutoIncrement
        ]
        public int Id { get; set; }

        [
            JsonProperty("user_id"),
            Alias("userId"),
            ForeignKey(typeof(SubscriptionObject))
        ]
        public ulong UserId { get; set; }

        [
            JsonProperty("grunt_type"),
            Alias("grunt_type"), 
            Required
        ]
        public InvasionGruntType GruntType { get; set; }

        [
            JsonProperty("city"),
            Alias("city"), 
            Required
        ]
        public string City { get; set; }
    }
}