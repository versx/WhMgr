namespace WhMgr.Data.Subscriptions.Models
{
    using ServiceStack.DataAnnotations;

    using Newtonsoft.Json;

    [
        JsonObject("pokemon"),
        Alias("pokemon")
    ]
    public class PokemonSubscription
    {
        [
            JsonIgnore,//JsonProperty("id"),
            Alias("id"), 
            PrimaryKey,
            AutoIncrement
        ]
        public int Id { get; set; }

        [
            JsonProperty("user_id"),
            Alias("userId"), 
            ForeignKey(typeof(SubscriptionObject))
        ]
        public ulong UserId { get; set; }

        [
            JsonProperty("pokemon_id"),
            Alias("pokemon_id"), 
            Required
        ]
        public int PokemonId { get; set; }

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

        //[Alias("city")]
        //public string City { get; set; }

        [
            JsonIgnore,
            Ignore
        ]
        public bool HasStats => Attack > 0 || Defense > 0 || Stamina > 0;

        public PokemonSubscription()
        {
            MinimumCP = 0;
            MinimumIV = 0;
            MinimumLevel = 0;
            Gender = "*";
            Attack = 0;
            Defense = 0;
            Stamina = 0;
        }
    }
}