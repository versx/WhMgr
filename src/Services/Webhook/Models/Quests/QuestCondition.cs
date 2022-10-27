﻿namespace WhMgr.Services.Webhook.Models.Quests
{
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    using ActivityType = POGOProtos.Rpc.HoloActivityType;

    public sealed class QuestCondition
    {
        [JsonPropertyName("pokemon_ids")]
        public List<uint> PokemonIds { get; set; }

        [JsonPropertyName("category_name")]
        public string CategoryName { get; set; }

        [JsonPropertyName("pokemon_type_ids")]
        public List<int> PokemonTypeIds { get; set; }

        [JsonPropertyName("throw_type_id")]
        public ActivityType ThrowTypeId { get; set; }

        [JsonPropertyName("hit")]
        public bool Hit { get; set; }

        [JsonPropertyName("raid_levels")]
        public List<int> RaidLevels { get; set; }

        [JsonPropertyName("alignment_ids")]
        public List<int> AlignmentIds { get; set; }

        [JsonPropertyName("character_category_ids")]
        public List<int> CharacterCategoryIds { get; set; }

        [JsonPropertyName("raid_pokemon_evolutions")]
        public List<int> RaidPokemonEvolutions { get; set; }

        [JsonPropertyName("must_be_max_level")]
        public ushort MaxLevel { get; set; }

        [JsonPropertyName("with_max_cp")]
        public uint MaxCp { get; set; }

        [JsonPropertyName("with_league_rank")]
        public uint GblRank { get; set; }

        [JsonPropertyName("encounter_type")]
        public List<int> EncounterType { get; set; }

        [JsonPropertyName("combat_type")]
        public List<int> CombatType { get; set; }

        [JsonPropertyName("level_cap")]
        public uint LevelCap { get; set; }

        public QuestCondition()
        {
            ThrowTypeId = ActivityType.ActivityUnknown;
        }
    }
}