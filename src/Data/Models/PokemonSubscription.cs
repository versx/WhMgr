namespace WhMgr.Data.Models
{
    using ServiceStack.DataAnnotations;

    [Alias("pokemon")]
    public class PokemonSubscription
    {
        [Alias("id"), PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Alias("userId"), ForeignKey(typeof(SubscriptionObject))]
        public ulong UserId { get; set; }

        [Alias("pokemon_id"), Unique]
        public int PokemonId { get; set; }

        [Alias("min_cp"), Default(0)]
        public int MinimumCP { get; set; }

        [Alias("miv_iv"), Default(0)]
        public int MinimumIV { get; set; }

        [Alias("min_lvl"), Default(0)]
        public int MinimumLevel { get; set; }

        [Alias("gender"), Default("*")]
        public string Gender { get; set; }

        //public string City { get; set; }

        public PokemonSubscription()
        {
            Gender = "*";
        }
    }

    //public class PokemonSubscription
    //{
    //    [JsonProperty("pokemon_id")]
    //    public int PokemonId { get; set; }

    //    [JsonProperty("min_cp")]
    //    public int MinimumCP { get; set; }

    //    [JsonProperty("miv_iv")]
    //    public int MinimumIV { get; set; }

    //    [JsonProperty("min_lvl")]
    //    public int MinimumLevel { get; set; }

    //    [JsonProperty("gender")]
    //    public string Gender { get; set; }

    //    //public string City { get; set; }

    //    public PokemonSubscription()
    //    {
    //        Gender = "*";
    //    }
    //}
}