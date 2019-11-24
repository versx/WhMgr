namespace WhMgr.Data
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using Newtonsoft.Json;

    using WhMgr.Data.Models;
    using WhMgr.Diagnostics;

    public class Database
    {
        #region Constants

        const string PokemonFileName = "pokemon.json";
        const string MovesetsFileName = "moves.json";

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

        public Dictionary<int, Moveset> Movesets { get; }

        #endregion

        #region Constructor

        public Database()
        {
            Pokemon = LoadInit<Dictionary<int, PokemonInfo>>(Path.Combine(Strings.DataFolder, PokemonFileName), typeof(Dictionary<int, PokemonInfo>));
            Movesets = LoadInit<Dictionary<int, Moveset>>(Path.Combine(Strings.DataFolder, MovesetsFileName), typeof(Dictionary<int, Moveset>));
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
}