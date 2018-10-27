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

        [Alias("min_cp")]
        public int MinimumCP { get; set; }

        [Alias("miv_iv")]
        public int MinimumIV { get; set; }

        [Alias("min_lvl")]
        public int MinimumLevel { get; set; }

        [Alias("gender")]
        public string Gender { get; set; }

        //public string City { get; set; }

        public PokemonSubscription()
        {
            MinimumCP = 0;
            MinimumIV = 0;
            MinimumLevel = 0;
            Gender = "*";
        }
    }
}