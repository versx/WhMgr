namespace WhMgr.Services.Subscriptions.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using System.Text.Json.Serialization;

    [Table("pokemon")]
    public class PokemonSubscription : BaseSubscription
    {
        #region Properties

        [
            JsonPropertyName("subscription_id"),
            Column("subscription_id"),
            //ForeignKey(typeof(Subscription)),
            ForeignKey("subscription_id"),
        ]
        public int SubscriptionId { get; set; }

        public Subscription Subscription { get; set; }

        /*
        [
            JsonIgnore,
            NotMapped,
        ]
        public List<uint> PokemonId
        {
            get
            {
                try
                {
                    return PokemonIdString?.Split(',')?
                                           .Select(x => uint.Parse(x))
                                           .ToList();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Error] Failed to parse pokemon id string: {ex}");
                }
                return new List<uint>();
            }
        }
        */

        [
            JsonPropertyName("pokemon_id"),
            Column("pokemon_id"),
            Required,
        ]
        public List<uint> PokemonId { get; set; }

        [
            JsonIgnore,
            NotMapped,
        ]
        public List<string> Forms => FormsString?.Split(',').ToList();

        [
            JsonPropertyName("form"),
            Column("form"),
        ]
        public string FormsString { get; set; }

        [
            JsonPropertyName("min_cp"),
            Column("min_cp"),
        ]
        public int MinimumCP { get; set; }

        [
            JsonPropertyName("min_iv"),
            Column("min_iv"),
        ]
        public int MinimumIV { get; set; }

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

        [
            JsonPropertyName("gender"),
            Column("gender"),
        ]
        public string Gender { get; set; }

        [
            JsonIgnore,
            NotMapped,
        ]
        public PokemonSize Size => (PokemonSize)_Size;

        [
            JsonPropertyName("size"),
            Column("size"),
            DefaultValue((uint)PokemonSize.All),
        ]
        public uint _Size { get; set; }

        [
            JsonPropertyName("city"),
            Column("city"),
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
        public bool HasStats => IVList?.Any() ?? false;

        #endregion

        #region Constructor

        public PokemonSubscription()
        {
            MinimumCP = 0;
            MinimumIV = 0;
            MinimumLevel = 0;
            MaximumLevel = 35;
            Gender = "*";
            _Size = (uint)PokemonSize.All;
            FormsString = null;
        }

        #endregion
    }
}