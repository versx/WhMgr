﻿namespace WhMgr.Data
{
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    using static POGOProtos.Rpc.BelugaPokemonProto.Types;

    using WhMgr.Common;

    public class PokedexPokemon
    {
        [JsonPropertyName("pokedex_id")]
        public uint PokedexId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("forms")]
        public Dictionary<uint, PokedexPokemon> Forms { get; set; }

        /*
        [JsonPropertyName("default_form_id")]
        public int? DefaultFormId { get; set; }

        [JsonPropertyName("default_form")]
        public string DefaultForm { get; set; }
        */

        [JsonPropertyName("evolutions")]
        public List<PokedexPokemonEvolution> Evolutions { get; set; }

        [JsonPropertyName("form")]
        public ushort? Form { get; set; }

        [JsonPropertyName("gender_requirement")]
        public PokemonGender GenderRequirement { get; set; }

        [JsonPropertyName("types")]
        public List<PokemonType> Types { get; set; }

        [JsonPropertyName("attack")]
        public int? Attack { get; set; }

        [JsonPropertyName("defense")]
        public int? Defense { get; set; }

        [JsonPropertyName("stamina")]
        public int? Stamina { get; set; }

        [JsonPropertyName("height")]
        public double? Height { get; set; }

        [JsonPropertyName("weight")]
        public double? Weight { get; set; }

        [JsonPropertyName("flee_rate")]
        public double? FleeRate { get; set; }

        [JsonPropertyName("quick_moves")]
        public List<string> QuickMoves { get; set; }

        [JsonPropertyName("charged_moves")]
        public List<string> ChargedMoves { get; set; }

        [JsonPropertyName("candy_to_evolve")]
        public int? Candy { get; set; }

        [JsonPropertyName("buddy_distance")]
        public int? BuddyDistance { get; set; }

        [JsonPropertyName("legendary")]
        public bool Legendary { get; set; }

        [JsonPropertyName("mythic")]
        public bool Mythical { get; set; }

        [JsonPropertyName("buddy_distance_evolve")]
        public int? BuddyDistanceEvolve { get; set; }

        [JsonPropertyName("third_move_stardust")]
        public int ThirdMoveStardust { get; set; }

        [JsonPropertyName("third_move_candy")]
        public int ThirdMoveCandy { get; set; }

        [JsonPropertyName("gym_defender_eligible")]
        public bool GymDeployable { get; set; }

        [JsonPropertyName("gen_id")]
        public uint GenerationId { get; set; }

        [JsonPropertyName("generation")]
        public string Generation { get; set; }

        [JsonPropertyName("temp_evolutions")]
        public Dictionary<uint, PokedexPokemon> TempEvolutions { get; set; }

        [JsonPropertyName("little")]
        public bool Little { get; set; }

        public PokedexPokemon()
        {
            Forms = new Dictionary<uint, PokedexPokemon>();
            Evolutions = new List<PokedexPokemonEvolution>();
            QuickMoves = new List<string>();
            ChargedMoves = new List<string>();
            Types = new List<PokemonType>();
        }
    }
}