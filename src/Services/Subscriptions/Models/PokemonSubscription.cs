namespace WhMgr.Services.Subscriptions.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using System.Text.Json.Serialization;

    using WhMgr.Common;

    [Table("pokemon")]
    public class PokemonSubscription : BasePokemonSubscription
    {
        #region Properties

        [
            JsonPropertyName("subscription_id"),
            Column("subscription_id"),
            //ForeignKey(typeof(Subscription)),
            ForeignKey("subscription_id"),
        ]
        public int SubscriptionId { get; set; }

        [JsonIgnore]
        public Subscription Subscription { get; set; }

        [
            JsonPropertyName("min_cp"),
            Column("min_cp"),
        ]
        public int MinimumCP { get; set; }

        // TODO: Maximum CP

        [
            JsonPropertyName("min_iv"),
            Column("min_iv"),
        ]
        public int MinimumIV { get; set; }

        // TODO: Maximum IV (maybe)

        [
            JsonPropertyName("iv_list"),
            Column("iv_list"),
        ]
        public List<string> IVList { get; set; } = new();

        [
            JsonPropertyName("min_lvl"),
            Column("min_lvl"),
        ]
        public int MinimumLevel { get; set; }

        [
            JsonPropertyName("max_lvl"),
            Column("max_lvl"),
        ]
        public int MaximumLevel { get; set; }

        // TODO: Moves

        [
            JsonPropertyName("gender"),
            Column("gender"),
        ]
        public string Gender { get; set; }

        [
            JsonPropertyName("size"),
            Column("size"),
            DefaultValue((uint)PokemonSize.All),
        ]
        public PokemonSize Size { get; set; }

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

        [
            JsonIgnore,
            NotMapped
        ]
        public bool HasIVStats => IVList?.Any() ?? false;

        #endregion

        #region Constructor

        public PokemonSubscription()
        {
            MinimumCP = 0;
            MinimumIV = 0;
            MinimumLevel = 0;
            MaximumLevel = 35;
            Gender = "*";
            Size = PokemonSize.All;
            PokemonId = new List<uint>();
        }

        #endregion
    }
}