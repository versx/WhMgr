namespace WhMgr.Data.Subscriptions.Models
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    using Newtonsoft.Json;

    [
        JsonObject("pokemon"),
        Table("pokemon")
    ]
    public class PokemonSubscription : SubscriptionItem
    {
        #region Properties

        [
            JsonProperty("subscription_id"),
            Column("subscription_id"),
            ForeignKey("subscription_id"),
            Required
        ]
        public int SubscriptionId { get; set; }

        [
            JsonProperty("subscription"),
        ]
        public SubscriptionObject Subscription { get; set; }

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
            JsonProperty("min_cp"),
            Column("min_cp")
        ]
        public int MinimumCP { get; set; }

        [
            JsonProperty("min_iv"),
            Column("min_iv")
        ]
        public int MinimumIV { get; set; }

        [
            JsonProperty("iv_list"),
            Column("iv_list")
        ]
        public List<string> IVList { get; set; }

        [
            JsonProperty("min_lvl"),
            Column("min_lvl")
        ]
        public int MinimumLevel { get; set; }

        [
            JsonProperty("max_lvl"),
            Column("max_lvl")
        ]
        public int MaximumLevel { get; set; }

        [
            JsonProperty("gender"),
            Column("gender")
        ]
        public string Gender { get; set; }

        [
            JsonProperty("city"),
            Column("city")
        ]
        public string City { get; set; }

        [
            JsonIgnore,
            NotMapped
        ]
        public bool HasStats => (IVList?.Count ?? 0) > 0;

        #endregion

        #region Constructor

        public PokemonSubscription()
        {
            MinimumCP = 0;
            MinimumIV = 0;
            MinimumLevel = 0;
            MaximumLevel = 35;
            Gender = "*";
            Form = null;
            City = null;
            IVList = new List<string>();
        }

        #endregion
    }
}