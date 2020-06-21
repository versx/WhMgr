namespace WhMgr.Data.Subscriptions.Models
{
    using System;

    using ServiceStack.DataAnnotations;

    using Newtonsoft.Json;

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
            Alias("league"),
            Required
        ]
        public PvPLeague League { get; set; }

        [
            JsonProperty("min_cp"),
            Ignore//Alias("min_cp")
        ]
        public int MinimumCP
        {
            get
            {
                switch (League)
                {
                    case PvPLeague.Great:
                        return 0;
                    case PvPLeague.Ultra:
                        return 1500;
                    case PvPLeague.Master:
                        return 2500;
                    case PvPLeague.Other:
                    default:
                        return 0;
                }
            }
        }

        [
            JsonProperty("max_cp"),
            Ignore//Alias("max_cp")
        ]
        public int MaximumCP => Convert.ToInt32(League);

        [
            JsonProperty("min_rank"),
            Alias("miv_rank"),
            Default(25)
        ]
        public int MinimumRank { get; set; }

        [
            JsonProperty("min_percent"),
            Alias("min_percent"),
            Default(90.0)
        ]
        public double MinimumPercent { get; set; }

        [
            JsonProperty("city"),
            Alias("city"),
            Default("all")
        ]
        public string City { get; set; }

        public PvPSubscription()
        {
            Form = null;
            League = PvPLeague.Great;
            MinimumRank = 25;
            MinimumPercent = 90;
            City = "all";
        }
    }
}