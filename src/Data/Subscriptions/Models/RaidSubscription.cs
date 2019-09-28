namespace WhMgr.Data.Subscriptions.Models
{
    using ServiceStack.DataAnnotations;

    using Newtonsoft.Json;

    [
        JsonObject("raids"),
        Alias("raids")
    ]
    public class RaidSubscription
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
            JsonProperty("pokemon_id"),
            Alias("pokemon_id"), 
            Required
        ]
        public int PokemonId { get; set; }

        [
            JsonProperty("form"),
            Alias("form")
        ]
        public string Form { get; set; }

        [
            JsonProperty("city"),
            Alias("city"), 
            Required
        ]
        public string City { get; set; }
    }
}