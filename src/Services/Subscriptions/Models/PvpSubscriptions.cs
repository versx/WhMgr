namespace WhMgr.Services.Subscriptions.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Text.Json.Serialization;

    using WhMgr.Common;
    using WhMgr.Configuration;
    using WhMgr.Services.Alarms.Filters;
    using WhMgr.Services.Webhook.Models;

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
            DefaultValue(null),
        ]
        public string Location { get; set; }

        public PvpSubscription()
        {
            Gender = "*";
            League = PvpLeague.Other;
        }

        #region Public Methods

        public bool RankExists(List<PvpRankData> rankData, PvpLeague league, PvpLeagueConfig config)
        {
            return rankData?.Exists(rank => RankExists(rank, league, config.MinimumCP, config.MaximumCP)) ?? false;
        }

        public bool RankExists(PvpRankData rankData, PvpLeague league, ushort minLeagueCP, ushort maxLeagueCP)
        {
            var cp = rankData.CP ?? Strings.Defaults.MinimumCP;
            var rank = rankData.Rank ?? 4096;
            var matchesGender = Filters.MatchesGender(rankData.Gender, string.IsNullOrEmpty(Gender) ? '*' : Gender[0]);
            var matchesLeague = League == league;
            var matchesCP = Filters.MatchesCP(cp, minLeagueCP, maxLeagueCP);
            var matchesRank = rank <= MinimumRank;
            //var matchesPercentage = (x.Percentage ?? 0) * 100 >= pkmnSub.MinimumPercent;
            return matchesLeague && matchesCP && matchesRank && matchesGender;
        }

        #endregion
    }
}