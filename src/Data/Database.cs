namespace WhMgr.Data
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using Newtonsoft.Json;
    using ServiceStack.DataAnnotations;

    using WhMgr.Data.Models;
    using WhMgr.Diagnostics;

    public class Database
    {
        #region Constants

        const string PokemonFileName = "pokemon.json";
        const string GreatPvPLibFileName = "pvp_great_league_ranks.json";
        const string UltraPvPLibFileName = "pvp_ultra_league_ranks.json";

        #endregion

        #region Variables

        private static readonly IEventLogger _logger = EventLogger.GetLogger("DATABASE");

        #endregion

        #region Singleton

        private static Database _instance;
        public static Database Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Database();
                }

                return _instance;
            }
        }

        #endregion

        #region Properties

        public Dictionary<int, PokemonInfo> Pokemon { get; set; }

        public GreatPvpRankLibrary GreatPvPLibrary { get; set; }

        public UltraPvpRankLibrary UltraPvPLibrary { get; set; }

        #endregion

        #region Constructor

        public Database()
        {
            Pokemon = LoadInit<Dictionary<int, PokemonInfo>>(Path.Combine(Strings.DataFolder, PokemonFileName), typeof(Dictionary<int, PokemonInfo>));
            //GreatPvPLibrary = LoadInit<GreatPvpRankLibrary>(Path.Combine(Strings.DataFolder, GreatPvPLibFileName), typeof(GreatPvpRankLibrary));
            //UltraPvPLibrary = LoadInit<UltraPvpRankLibrary>(Path.Combine(Strings.DataFolder, UltraPvPLibFileName), typeof(UltraPvpRankLibrary));
        }

        #endregion

        public static T LoadInit<T>(string filePath, Type type)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"{filePath} file not found.", filePath);
            }

            var data = File.ReadAllText(filePath);
            if (string.IsNullOrEmpty(data))
            {
                _logger.Error($"{filePath} database is empty.");
                return default;
            }

            return (T)JsonConvert.DeserializeObject(data, type);
        }
    }

    #region PvP

    public abstract class PvPRank
    {
        [
            JsonProperty("pokemon_id"),
            Alias("pokemon_id")
        ]
        public int PokemonId { get; set; }

        [
            JsonProperty("form"),
            Alias("form")
        ]
        public int FormId { get; set; }

        [
            JsonProperty("attack"),
            Alias("attack")
        ]
        public int Attack { get; set; }

        [
            JsonProperty("defense"),
            Alias("defense")
        ]
        public int Defense { get; set; }

        [
            JsonProperty("stamina"),
            Alias("stamina")
        ]
        public int Stamina { get; set; }

        [
            JsonProperty("value"),
            Alias("value")
        ]
        public int Value { get; set; }

        [
            JsonProperty("level"),
            Alias("level")
        ]
        public double Level { get; set; }

        [
            JsonProperty("CP"),
            Alias("CP")
        ]
        public int CP { get; set; }

        [
            JsonProperty("percent"),
            Alias("percent")
        ]
        public double Percent { get; set; }

        [
            JsonProperty("rank"),
            Alias("rank")
        ]
        public int Rank { get; set; }
    }

    [Alias("great_league")]
    public class GreatPvPRank : PvPRank { }

    [Alias("ultra_league")]
    public class UltraPvPRank : PvPRank { }

    public class GreatPvpRankLibrary : Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, GreatPvPRank>>>>> { }

    public class UltraPvpRankLibrary : Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, UltraPvPRank>>>>> { }

    #endregion
}