﻿namespace WhMgr.Services.Subscriptions.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using System.Text.Json.Serialization;

    // TODO: Use interface/abstract class for pokemon_id, forms, costumes between subscription objects for easiler filter checks

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

        [JsonIgnore]
        public Subscription Subscription { get; set; }

        [
            JsonPropertyName("pokemon_id"),
            Column("pokemon_id"),
            Required,
        ]
        public List<uint> PokemonId { get; set; } = new();

        /*
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
        */

        [
            JsonPropertyName("forms"),
            Column("forms"),
        ]
        public List<string> Forms { get; set; } = new();

        /*
        [
            JsonPropertyName("costumes"),
            Column("costumes"),
        ]
        public List<string> Costumes { get; set; }
        */

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

        /*
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
        */

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
            Size = (uint)PokemonSize.All;
            PokemonId = new List<uint>();
            Forms = new List<string>();
            //Costumes = new List<string>();
        }

        #endregion
    }
}