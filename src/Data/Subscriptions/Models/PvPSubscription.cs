namespace WhMgr.Data.Subscriptions.Models
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    using ServiceStack.DataAnnotations;

    [
        //JsonPropertyName("pvp"),
        Alias("pvp"),
    ]
    public class PvPSubscription : SubscriptionItem
    {
        [
            JsonPropertyName("subscription_id"),
            Alias("subscription_id"),
            ForeignKey(typeof(SubscriptionObject))
        ]
        public int SubscriptionId { get; set; }

        [
            JsonPropertyName("pokemon_id"),
            Alias("pokemon_id"),
            Required
        ]
        public int PokemonId { get; set; }

        [
            JsonPropertyName("form"),
            Alias("form")
        ]
        public string Form { get; set; }

        [
            JsonPropertyName("league"),
            Alias("league"),
            Required
        ]
        public PvPLeague League { get; set; }

        [
            JsonPropertyName("min_rank"),
            Alias("min_rank"),
            Default(25)
        ]
        public int MinimumRank { get; set; }

        [
            JsonPropertyName("min_percent"),
            Alias("min_percent"),
            Default(90.0)
        ]
        public double MinimumPercent { get; set; }

        [
            JsonPropertyName("city"),
            Alias("city"),
        ]
        public List<string> Areas { get; set; }

        [
            JsonPropertyName("location"),
            Alias("location"),
        ]
        public string Location { get; set; }

        public PvPSubscription()
        {
            Form = null;
            League = PvPLeague.Great;
            MinimumRank = 25;
            MinimumPercent = 90;
            Areas = new List<string>();
        }
    }
}