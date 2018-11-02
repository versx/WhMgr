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

        private static readonly IEventLogger _logger = EventLogger.GetLogger();

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

        public Dictionary<int, PokemonModel> Pokemon { get; }

        public Dictionary<int, MovesetModel> Movesets { get; }

        #endregion

        #region Constructor

        public Database()
        {
            Pokemon = LoadInit<Dictionary<int, PokemonModel>>(Path.Combine(Strings.DataFolder, PokemonFileName), typeof(Dictionary<int, PokemonModel>));
            Movesets = LoadInit<Dictionary<int, MovesetModel>>(Path.Combine(Strings.DataFolder, MovesetsFileName), typeof(Dictionary<int, MovesetModel>));
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
                _logger.Error($"{filePath} database is null.");
                return default(T);
            }

            return (T)JsonConvert.DeserializeObject(data, type);
        }
    }
}