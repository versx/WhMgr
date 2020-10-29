namespace WhMgr.Data.Subscriptions.Models
{
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    using Newtonsoft.Json;

    [
        JsonObject("pvp"),
        Table("pvp")
    ]
    public class PvPSubscription : SubscriptionItem
    {
        [
            JsonProperty("subscription_id"),
            Column("subscription_id"),
            ForeignKey("rk_pvp_subscription_id"),
            Required
        ]
        public int SubscriptionId { get; set; }

        //[
        //    JsonProperty("subscription"),
        //]
        //public SubscriptionObject Subscription { get; set; }

        [
            JsonProperty("pokemon_id"),
            Column("pokemon_id"),
            Required
        ]
        public int PokemonId { get; set; }

        [
            JsonProperty("form"),
            Column("form")
        ]
        public string Form { get; set; }

        [
            JsonProperty("league"),
            Column("league"),
            Required
        ]
        public PvPLeague League { get; set; }

        [
            JsonProperty("min_rank"),
            Column("min_rank"),
            DefaultValue(25),
            Required
        ]
        public int MinimumRank { get; set; }

        [
            JsonProperty("min_percent"),
            Column("min_percent"),
            DefaultValue(90.0)
        ]
        public double MinimumPercent { get; set; }

        [
            JsonProperty("city"),
            Column("city"),
            //Default("all")
        ]
        public string City { get; set; }

        public PvPSubscription()
        {
            Form = null;
            League = PvPLeague.Great;
            MinimumRank = 25;
            MinimumPercent = 90;
            City = null;// "all";
        }
    }
}