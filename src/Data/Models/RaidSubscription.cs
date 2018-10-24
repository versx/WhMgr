namespace WhMgr.Data.Models
{
    using ServiceStack.DataAnnotations;

    [Alias("raids")]
    public class RaidSubscription
    {
        [Alias("id"), PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Alias("userId"), ForeignKey(typeof(SubscriptionObject))]
        public ulong UserId { get; set; }

        [Alias("pokemon_id"), Required]
        public int PokemonId { get; set; }

        [Alias("city"), Required]
        public string City { get; set; }
    }

    //public class RaidSubscription
    //{
    //    [JsonProperty("pokemon_id")]
    //    public int PokemonId { get; set; }

    //    [JsonProperty("city")]
    //    public string City { get; set; }
    //}
}