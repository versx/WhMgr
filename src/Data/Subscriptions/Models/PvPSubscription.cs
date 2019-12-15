namespace WhMgr.Data.Subscriptions.Models
{
    using System;

    using ServiceStack.DataAnnotations;

    using Newtonsoft.Json;

    public enum PvPLeague
    {
        Other = 0,
        Great = 1500,
        Ultra = 2500,
        Master = 5000
    }

    [
        JsonObject("pvp"),
        Alias("pvp")
    ]
    public class PvPSubscription : SubscriptionItem
    {
        [
            Alias("subscription_id"),
            ForeignKey(typeof(SubscriptionObject))
        ]
        public int SubscriptionId { get; set; }

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
            JsonProperty("league"),
            Alias("league")
        ]
        public PvPLeague League { get; set; }

        [
            JsonProperty("min_cp"),
            Ignore//Alias("min_cp")
        ]
        public int MinimumCP { get; set; }

        [
            JsonProperty("max_cp"),
            Ignore//Alias("max_cp")
        ]
        public int MaximumCP => Convert.ToInt32(League);

        [
            JsonProperty("min_rank"),
            Alias("miv_rank")
        ]
        public int MinimumRank { get; set; }

        [
            JsonProperty("min_percent"),
            Alias("min_percent")
        ]
        public double MinimumPercent { get; set; }

        public PvPSubscription()
        {
            League = PvPLeague.Great;
            MinimumCP = 0;
            //MaximumCP = 2500;
            MinimumRank = 5;
            MinimumPercent = 90;
        }
    }
}