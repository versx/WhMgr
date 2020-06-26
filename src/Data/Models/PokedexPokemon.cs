namespace WhMgr.Data.Models
{
    using System;
    using System.Collections.Generic;

    using Newtonsoft.Json;

    using WhMgr.Net.Models;

    public class PokedexPokemon
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("forms")]
        public Dictionary<int, PokedexPokemon> Forms { get; set; }

        [JsonProperty("default_form")]
        public int? DefaultForm { get; set; }

        [JsonProperty("evolved_form")]
        public int? EvolvedForm { get; set; }

        [JsonProperty("evolutions")]
        //public List<int> Evolutions { get; set; }
        public List<string> Evolutions { get; set; } //TODO: Check "657_0"

        [JsonProperty("types")]
        public List<PokemonType> Types { get; set; }

        [JsonProperty("dex")]
        public string Dex { get; set; }

        [JsonProperty("attack")]
        public int? Attack { get; set; }

        [JsonProperty("defense")]
        public int? Defense { get; set; }

        [JsonProperty("stamina")]
        public int? Stamina { get; set; }

        [JsonProperty("height")]
        public double? Height { get; set; }

        [JsonProperty("weight")]
        public double? Weight { get; set; }

        [JsonProperty("male_percent")]
        public double? MalePercent { get; set; }

        [JsonProperty("female_percent")]
        public double? FemalePercent { get; set; }

        [JsonProperty("flee_rate")]
        public double? FleeRate { get; set; }

        [JsonProperty("quickmove")]
        public List<string> QuickMoves { get; set; }

        [JsonProperty("chargedmove")]
        public List<string> ChargedMoves { get; set; }

        [JsonProperty("candy")]
        public int? Candy { get; set; }

        [JsonProperty("buddy_distance")]
        public int? BuddyDistance { get; set; }

        [JsonProperty("third_move")]
        public ThirdMove ThirdMove { get; set; }

        [JsonProperty("evolution_item")]
        public string EvolutionItem { get; set; }

        [JsonProperty("legendary")]
        public string Legendary { get; set; }

        [JsonProperty("gender_requirement")]
        public string GenderRequirement { get; set; }

        [JsonProperty("buddy_distance_evolve")]
        public int? BuddyDistanceEvolve { get; set; }
    }

    public class ThirdMove
    {
        [JsonProperty("stardustToUnlock")]
        public int StardustToUnlock { get; set; }

        [JsonProperty("candyToUnlock")]
        public int CandyToUnlock { get; set; }
    }
}