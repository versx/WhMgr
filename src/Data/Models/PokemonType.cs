﻿namespace WhMgr.Data.Models
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    [JsonConverter(typeof(StringEnumConverter))]
    public enum PokemonType
    {
        None = 0,
        Normal = 1,
        Fighting = 2,
        Flying = 3,
        Poison = 4,
        Ground = 5,
        Rock = 6,
        Bug = 7,
        Ghost = 8,
        Steel = 9,
        Fire = 10,
        Water = 11,
        Grass = 12,
        Electric = 13,
        Psychic = 14,
        Ice = 15,
        Dragon = 16,
        Dark = 17,
        Fairy = 18,
    }
}