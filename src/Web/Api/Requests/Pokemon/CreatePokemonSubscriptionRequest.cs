namespace WhMgr.Web.Api.Requests.Pokemon
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    using WhMgr.Services.Subscriptions.Models;

    public class CreatePokemonSubscriptionRequest
    {
        [JsonPropertyName("pokemon")]
        public List<uint> Pokemon { get; set; } = new();

        [JsonPropertyName("forms")]
        public List<string> Forms { get; set; } = new();

        [JsonPropertyName("costumes")]
        public List<string> Costumes { get; set; } = new();

        [JsonPropertyName("min_iv")]
        public uint MinimumIV { get; set; }

        [JsonPropertyName("iv_list")]
        public List<string> IVList { get; set; } = new();

        [JsonPropertyName("min_lvl")]
        public ushort MinimumLevel { get; set; }

        [JsonPropertyName("max_lvl")]
        public ushort MaximumLevel { get; set; } = 35;

        [JsonPropertyName("gender")]
        public char Gender { get; set; } = '*';

        [JsonPropertyName("size")]
        public PokemonSize Size { get; set; }

        [JsonPropertyName("location")]
        public string Location { get; set; }

        [JsonPropertyName("areas")]
        public List<string> Areas { get; set; } = new();
    }
}