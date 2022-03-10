namespace WhMgr
{
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    using WeatherCondition = POGOProtos.Rpc.GameplayWeatherProto.Types.WeatherCondition;

    using WhMgr.Common;

    public class Defaults
    {
        // Default Pokemon settings
        [JsonPropertyName("min_iv")]
        public sbyte MinimumIV { get; set; }

        [JsonPropertyName("max_iv")]
        public sbyte MaximumIV { get; set; }

        [JsonPropertyName("min_lvl")]
        public sbyte MinimumLevel { get; set; }

        [JsonPropertyName("max_lvl")]
        public sbyte MaximumLevel { get; set; }

        [JsonPropertyName("min_cp")]
        public uint MinimumCP { get; set; }

        [JsonPropertyName("max_cp")]
        public uint MaximumCP { get; set; }

        [JsonPropertyName("min_rank")]
        public sbyte MinimumRank { get; set; }

        [JsonPropertyName("max_rank")]
        public sbyte MaximumRank { get; set; }

        [JsonPropertyName("min_percent")]
        public double MinimumPercent { get; set; }

        [JsonPropertyName("max_percent")]
        public double MaximumPercent { get; set; }

        [JsonPropertyName("min_great_league_cp")]
        public ushort MinimumGreatLeagueCP { get; set; }

        [JsonPropertyName("max_great_league_cp")]
        public ushort MaximumGreatLeagueCP { get; set; }

        [JsonPropertyName("min_ultra_league_cp")]
        public ushort MinimumUltraLeagueCP { get; set; }

        [JsonPropertyName("max_ultra_league_cp")]
        public ushort MaximumUltraLeagueCP { get; set; }


        // Webhook and subscription queue settings
        [JsonPropertyName("max_queue_batch_size")]
        public ushort MaximumQueueBatchSize { get; set; }

        [JsonPropertyName("max_queue_size_warning")]
        public ushort MaximumQueueSizeWarning { get; set; }


        // Location map format strings
        [JsonPropertyName("google_maps")]
        public string GoogleMaps { get; set; }

        [JsonPropertyName("apple_maps")]
        public string AppleMaps { get; set; }

        [JsonPropertyName("waze_maps")]
        public string WazeMaps { get; set; }

        [JsonPropertyName("emojis_list")]
        public List<string> EmojisList { get; set; }

        [JsonPropertyName("pokemon_generation_ranges")]
        public IReadOnlyDictionary<int, PokemonGenerationRange> PokemonGenerationRanges { get; set; }

        [JsonPropertyName("weather_boosts")]
        public IReadOnlyDictionary<WeatherCondition, IReadOnlyList<PokemonType>> WeatherBoosts { get; set; }

        public Defaults()
        {
            MinimumIV = 0;
            MaximumIV = 100;
            MinimumLevel = 0;
            MaximumLevel = 35;
            MinimumCP = 0;
            MaximumCP = 99999;
            MinimumRank = 0;
            MaximumRank = 100;
            MinimumPercent = 0;
            MaximumPercent = 100;
            MinimumGreatLeagueCP = 1400;
            MaximumGreatLeagueCP = 1500;
            MinimumUltraLeagueCP = 2400;
            MaximumUltraLeagueCP = 2500;

            MaximumQueueBatchSize = 10;
            MaximumQueueSizeWarning = 100;

            GoogleMaps = "https://maps.google.com/maps?q={0},{1}";
            AppleMaps = "https://maps.apple.com/maps?daddr={0},{1}";
            WazeMaps = "https://waze.com/ul?ll={0},{1}&navigate=yes";

            EmojisList = new List<string>
            {
                // Team emojis
                "neutral",
                "valor",
                "mystic",
                "instinct",

                // Capture rate emojis
                "capture_1",
                "capture_2",
                "capture_3",

                // Weather emojis
                "weather_1", // Clear
                "weather_2", // Rain
                "weather_3", // PartlyCloudy/Overcast
                "weather_4", // Cloudy
                "weather_5", // Windy
                "weather_6", // Snow
                "weather_7", // Fog

                // Ex gym emoji
                "ex",

                // Type emojis
                "types_fire",
                "types_grass",
                "types_ground",
                "types_rock",
                "types_water",
                "types_ghost",
                "types_ice",
                "types_dragon",
                "types_fairy",
                "types_fighting",
                "types_bug",
                "types_psychic",
                "types_electric",
                "types_steel",
                "types_dark",
                "types_normal",
                "types_flying",
                "types_poison",

                // PVP league emojis
                "league_great",
                "league_ultra",

                // Gender emojis
                "gender_male",
                "gender_female",
                "gender_less",
            };

            PokemonGenerationRanges = new Dictionary<int, PokemonGenerationRange>
            {
                { 1, new PokemonGenerationRange { Generation = 1, Start = 1, End = 151 } },
                { 2, new PokemonGenerationRange { Generation = 2, Start = 152, End = 251 } },
                { 3, new PokemonGenerationRange { Generation = 3, Start = 252, End = 385 } },
                { 4, new PokemonGenerationRange { Generation = 4, Start = 386, End = 493 } },
                { 5, new PokemonGenerationRange { Generation = 5, Start = 494, End = 649 } },
                { 6, new PokemonGenerationRange { Generation = 6, Start = 650, End = 721 } },
                { 7, new PokemonGenerationRange { Generation = 7, Start = 722, End = 809 } },
                { 8, new PokemonGenerationRange { Generation = 8, Start = 810, End = 898 } },
            };

            WeatherBoosts = new Dictionary<WeatherCondition, IReadOnlyList<PokemonType>>
            {
                { WeatherCondition.None,         new List<PokemonType> { } },
                { WeatherCondition.Clear,        new List<PokemonType> { PokemonType.Fire,   PokemonType.Grass,    PokemonType.Ground } },
                { WeatherCondition.Rainy,        new List<PokemonType> { PokemonType.Water,  PokemonType.Electric, PokemonType.Bug } },
                { WeatherCondition.PartlyCloudy, new List<PokemonType> { PokemonType.Normal, PokemonType.Rock } },
                { WeatherCondition.Overcast,     new List<PokemonType> { PokemonType.Fairy,  PokemonType.Fighting, PokemonType.Poison } },
                { WeatherCondition.Windy,        new List<PokemonType> { PokemonType.Dragon, PokemonType.Flying,   PokemonType.Psychic } },
                { WeatherCondition.Snow,         new List<PokemonType> { PokemonType.Ice,    PokemonType.Steel } },
                { WeatherCondition.Fog,          new List<PokemonType> { PokemonType.Dark,   PokemonType.Ghost } }
            };
        }
    }
}