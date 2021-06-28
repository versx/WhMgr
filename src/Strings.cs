namespace WhMgr
{
    using System.Collections.Generic;
    using System.IO;

    using WhMgr.Data;
    using WeatherCondition = POGOProtos.Rpc.GameplayWeatherProto.Types.WeatherCondition;

    public static class Strings
    {
        public const string BotName = "Webhook Manager";
        public const string BotVersion = "v5.0.0-b1";

        public const string GoogleMaps = "https://maps.google.com/maps?q={0},{1}";
        public const string AppleMaps = "https://maps.apple.com/maps?daddr={0},{1}";
        public const string WazeMaps = "https://waze.com/ul?ll={0},{1}&navigate=yes";

        public const string BasePath = "bin/debug/";
        public const string GeofenceFolder = BasePath + "geofences";
        public const string AlarmsFolder = BasePath + "alarms"; // TODO: Fix path
        public const string EmbedsFolder = BasePath + "embeds";
        public const string DiscordsFolder = BasePath + "discords";
        public const string FiltersFolder = BasePath + "filters";
        public const string LibrariesFolder = "libs";
        public const string StaticFolder = BasePath + "static";
        public const string TemplatesFolder = BasePath + "templates";
        public const string MigrationsFolder = "migrations";
        public static readonly string AppFolder = StaticFolder + Path.DirectorySeparatorChar + "app";
        public static readonly string DataFolder = StaticFolder + Path.DirectorySeparatorChar + "data";
        public static readonly string LocaleFolder = StaticFolder + Path.DirectorySeparatorChar + "locale";
        public static readonly string EmojisFolder = StaticFolder + Path.DirectorySeparatorChar + "emojis";
        public static readonly string OsmNestFilePath = StaticFolder + Path.DirectorySeparatorChar + OsmNestFileName;

        public const string ConfigFileName = "config.json";
        public const string OsmNestFileName = "nest.json";
        public const string DebugLogFileName = "debug.log";
        public const string ErrorLogFileName = "error.log";

        // Default filter settings for alarms and subscriptions
        public const int MinimumIV = 0;
        public const int MaximumIV = 100;
        public const int MinimumLevel = 0;
        public const int MaximumLevel = 35;
        public const int MinimumCP = 0;
        public const int MaximumCP = 99999;
        public const int MinimumRank = 0;
        public const int MaximumRank = 100;
        public const int MinimumPercent = 0;
        public const int MaximumPercent = 100;
        public const int MinimumGreatLeagueCP = 1400;
        public const int MaximumGreatLeagueCP = 1500;
        public const int MinimumUltraLeagueCP = 2400;
        public const int MaximumUltraLeagueCP = 2500;

        public const string All = "All";

        public const int MaxQueueCountWarning = 30;

        public const string EmojiSchema = "<:{0}:{1}>";
        public const string TypeEmojiSchema = "<:types_{0}:{1}>";

        public static readonly Dictionary<int, PokemonGenerationRange> PokemonGenerationRanges = new()
        {
            { 1, new PokemonGenerationRange { Generation = 1, Start = 1, End = 151 } },
            { 2, new PokemonGenerationRange { Generation = 2, Start = 152, End = 251 } },
            { 3, new PokemonGenerationRange { Generation = 3, Start = 252, End = 385 } },
            { 4, new PokemonGenerationRange { Generation = 4, Start = 386, End = 493 } },
            { 5, new PokemonGenerationRange { Generation = 5, Start = 495, End = 649 } },
            { 6, new PokemonGenerationRange { Generation = 6, Start = 650, End = 721 } },
            { 7, new PokemonGenerationRange { Generation = 7, Start = 722, End = 809 } },
            { 8, new PokemonGenerationRange { Generation = 8, Start = 810, End = 890 } },
        };

        // Required emoji list
        public static readonly string[] EmojiList =
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
            "gender_less"
        };

        public static IReadOnlyDictionary<WeatherCondition, List<PokemonType>> WeatherBoosts => new Dictionary<WeatherCondition, List<PokemonType>>
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