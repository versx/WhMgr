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
            var pokemonPath = Path.Combine(Strings.DataFolder, PokemonFileName);
            if (!File.Exists(pokemonPath))
            {
                throw new FileNotFoundException($"{pokemonPath} file not found.", PokemonFileName);
            }

            var data = File.ReadAllText(pokemonPath);
            if (data == null)
            {
                _logger.Error("Pokemon database is null.");
            }
            else
            {
                Pokemon = JsonConvert.DeserializeObject<Dictionary<int, PokemonModel>>(data);
            }

            var movesetPath = Path.Combine(Strings.DataFolder, MovesetsFileName);
            if (!File.Exists(movesetPath))
            {
                throw new FileNotFoundException($"{movesetPath} file not found.", MovesetsFileName);
            }

            data = File.ReadAllText(movesetPath);
            if (string.IsNullOrEmpty(data))
            {
                _logger.Error("Moveset database is null.");
            }
            else
            {
                Movesets = JsonConvert.DeserializeObject<Dictionary<int, MovesetModel>>(data);
            }
        }

        #endregion
    }
}