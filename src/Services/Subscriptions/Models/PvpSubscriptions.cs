namespace WhMgr.Services.Subscriptions.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Text.Json.Serialization;

    using WhMgr.Common;

    [Table("pvp")]
    public class PvpSubscription : BasePokemonSubscription
    {
        [
            JsonPropertyName("subscription_id"),
            Column("subscription_id"),
            //ForeignKey(typeof(Subscription))
            ForeignKey("subscription_id"),
        ]
        public int SubscriptionId { get; set; }

        [JsonIgnore]
        public Subscription Subscription { get; set; }

        [
            JsonPropertyName("gender"),
            Column("gender"),
            DefaultValue("*"),
            Required,
        ]
        public string Gender { get; set; }

        [
            JsonPropertyName("league"),
            Column("league"),
            Required,
        ]
        public PvpLeague League { get; set; }

        [
            JsonPropertyName("min_rank"),
            Column("min_rank"),
            DefaultValue(25),
        ]
        public int MinimumRank { get; set; }

        [
            JsonPropertyName("min_percent"),
            Column("min_percent"),
            DefaultValue(90.0),
        ]
        public double MinimumPercent { get; set; }

        [
            JsonPropertyName("areas"),
            Column("areas"),
        ]
        public List<string> Areas { get; set; } = new();

        [
            JsonPropertyName("location"),
            Column("location"),
        ]
        public string Location { get; set; }

        public PvpSubscription()
        {
            Gender = "*";
            League = PvpLeague.Great;
            MinimumRank = Strings.Defaults.MinimumRank;
            MinimumPercent = Strings.Defaults.MinimumPercent;
        }
    }
}