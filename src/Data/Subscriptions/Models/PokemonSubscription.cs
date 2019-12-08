namespace WhMgr.Data.Subscriptions.Models
{
    using ServiceStack.DataAnnotations;

    using Newtonsoft.Json;

    [
        JsonObject("pokemon"),
        Alias("pokemon")
    ]
    public class PokemonSubscription : SubscriptionItem
    {
        #region Properties

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
            JsonProperty("min_cp"),
            Alias("min_cp")
        ]
        public int MinimumCP { get; set; }

        [
            JsonProperty("min_iv"),
            Alias("miv_iv")
        ]
        public int MinimumIV { get; set; }

        [
            JsonProperty("min_lvl"),
            Alias("min_lvl")
        ]
        public int MinimumLevel { get; set; }

        [
            JsonProperty("gender"),
            Alias("gender")
        ]
        public string Gender { get; set; }

        [
            JsonProperty("attack"),
            Alias("attack")
        ]
        public int Attack { get; set; }

        [
            JsonProperty("defense"),
            Alias("defense")
        ]
        public int Defense { get; set; }

        [
            JsonProperty("stamina"),
            Alias("stamina")
        ]
        public int Stamina { get; set; }

        [
            JsonProperty("min_rank"),
            Alias("min_rank")
        ]
        public int MinimumRank { get; set; }

        //[Alias("city")]
        //public string City { get; set; }

        [
            JsonIgnore,
            Ignore
        ]
        public bool HasStats => Attack > 0 || Defense > 0 || Stamina > 0;

        #endregion

        #region Constructor

        public PokemonSubscription()
        {
            MinimumCP = 0;
            MinimumIV = 0;
            MinimumLevel = 0;
            Gender = "*";
            Attack = 0;
            Defense = 0;
            Stamina = 0;
            MinimumRank = 0;
            Form = string.Empty;
        }

        #endregion
    }
}