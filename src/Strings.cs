namespace WhMgr
{
    using System;
    using System.Collections.Generic;

    using WhMgr.Net.Models;

    internal static class Strings
    {
        public const string BotName = "Brock";

        public const string GoogleMaps = "https://maps.google.com/maps?q={0},{1}";
        public const string AppleMaps = "https://maps.apple.com/maps?daddr={0},{1}";
        //public const string GoogleMapsStaticImage = "https://maps.googleapis.com/maps/api/staticmap?center={0},{1}&markers=color:red%7C{0},{1}&maptype=roadmap&size=300x175&zoom=14";
        //public const string GoogleMapsStaticImage = "https://api.mapbox.com/styles/v1/mapbox/streets-v11/static/pin-s+FF0000({1},{0})/{1},{0},14/300x175?access_token=pk.eyJ1IjoidmVyc3giLCJhIjoiY2p3dXNzYmR0MDFmNzRicXNlNHJ4YjJucSJ9.tBti0YjkEb98_hxhswsSOw";

        public const string DataFolder = "Data";
        public const string GeofenceFolder = "Geofences";
        public const string AlertsFolder = "Alerts";
        public const string FiltersFolder = "Filters";
        public const string LibrariesFolder = "Libs";
        public static readonly string LocaleFolder = "static" + System.IO.Path.DirectorySeparatorChar + "locale";
        public static readonly string EmojisFolder = "static" + System.IO.Path.DirectorySeparatorChar + "emojis";
        public const string StatsFolder = "Stats";
        public const string LogsFolder = "Logs";

        public const string DefaultResponseMessage = "WH Test Running!";
        public static readonly string[] LocalEndPoint = { "localhost", "127.0.0.1" };

        public const string AlarmsFileName = "alarms.json";
        public const string ConfigFileName = "config.json";
        public const string DebugLogFileName = "debug.log";
        public const string ErrorLogFileName = "error.log";
        public const string StatsFileName = "notifications_{0}.csv";

        public const int MaxPokemonIds = 649;

        public const string All = "All";

        public const int MaxPokemonDisplayed = 70;
        public const int MaxPokemonSubscriptions = 25;
        public const int MaxRaidSubscriptions = 5;
        public const int MaxQuestSubscriptions = 2;
        public const int MaxInvasionSubscriptions = 1;
        public const int CommonTypeMinimumIV = 95;

        public const string TypeEmojiSchema = "<:types_{0}:{1}>";

        public const string CrashMessage = "WHM JUST CRASHED!";

        public static readonly Dictionary<int, PokemonGenerationRange> PokemonGenerationRanges = new Dictionary<int, PokemonGenerationRange>
        {
            { 1, new PokemonGenerationRange { Generation = 1, Start = 1, End = 151 } },
            { 2, new PokemonGenerationRange { Generation = 2, Start = 152, End = 251 } },
            { 3, new PokemonGenerationRange { Generation = 3, Start = 252, End = 385 } },
            { 4, new PokemonGenerationRange { Generation = 4, Start = 386, End = 493 } },
            { 5, new PokemonGenerationRange { Generation = 5, Start = 495, End = 649 } }
        };

        public static readonly string[] EmojiList =
        {
            //Team emojis
            "valor",
            "mystic",
            "instinct",

            //Weather emojis
            "weather_1", //Clear
            "weather_2", //Rain
            "weather_3", //PartlyCloudy/Overcast
            "weather_4", //Cloudy
            "weather_5", //Windy
            "weather_6", //Snow
            "weather_7", //Fog

            //Catch chances emojis

            //Ex gym emoji
            "ex",

            //Type emojis
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
            "types_poison"
        };

        public static IReadOnlyDictionary<WeatherType, string> WeatherEmojis => new Dictionary<WeatherType, string>
        {
            { WeatherType.None, "" },
            { WeatherType.Clear, ":weather_1:" },//":sunny:" }, //☀️
            { WeatherType.Rain, ":weather_2:" },//":umbrella:" }, //☔️
            { WeatherType.PartlyCloudy, ":weather_3:" },//":partly_sunny:" }, //⛅
            { WeatherType.Cloudy, ":weather_4:" },//":cloud:" }, //☁️
            { WeatherType.Windy, ":weather_5:" },//":dash:" }, //💨
            { WeatherType.Snow, ":weather_6:" },//":snowman:" }, //⛄️
            { WeatherType.Fog, ":weather_7:" },//":foggy:" } //🌁
        };

        public static IReadOnlyDictionary<WeatherType, List<PokemonType>> WeatherBoosts => new Dictionary<WeatherType, List<PokemonType>>
        {
            { WeatherType.None,         new List<PokemonType> { } },
            { WeatherType.Clear,        new List<PokemonType> { PokemonType.Fire,   PokemonType.Grass,    PokemonType.Ground } },
            { WeatherType.Rain,         new List<PokemonType> { PokemonType.Water,  PokemonType.Electric, PokemonType.Bug } },
            { WeatherType.PartlyCloudy, new List<PokemonType> { PokemonType.Normal, PokemonType.Rock } },
            { WeatherType.Cloudy,       new List<PokemonType> { PokemonType.Fairy,  PokemonType.Fighting, PokemonType.Poison } },
            { WeatherType.Windy,        new List<PokemonType> { PokemonType.Dragon, PokemonType.Flying,   PokemonType.Psychic } },
            { WeatherType.Snow,         new List<PokemonType> { PokemonType.Ice,    PokemonType.Steel } },
            { WeatherType.Fog,          new List<PokemonType> { PokemonType.Dark,   PokemonType.Ghost } }
        };
    }

    public class PokemonGenerationRange
    {
        public int Generation { get; set; }

        public int Start { get; set; }

        public int End { get; set; }
    }
}