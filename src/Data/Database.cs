namespace T.Data
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using Newtonsoft.Json;

    using T.Data.Models;
    using T.Diagnostics;

    public class Database
    {
        const string PokemonFileName = "pokemon.json";
        const string MovesetsFileName = "moves.json";

        private static readonly IEventLogger _logger = EventLogger.GetLogger();

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

        public Dictionary<int, PokemonModel> Pokemon { get; }

        public Dictionary<int, MovesetModel> Movesets { get; }

        public Database()
        {
            if (!File.Exists(PokemonFileName))
            {
                throw new FileNotFoundException("pokemon.json file not found.", PokemonFileName);
            }

            var data = File.ReadAllText(PokemonFileName);
            if (data == null)
            {
                _logger.Error("Pokemon database is null.");
            }
            else
            {
                Pokemon = JsonConvert.DeserializeObject<Dictionary<int, PokemonModel>>(data);
            }

            if (!File.Exists(MovesetsFileName))
            {
                throw new FileNotFoundException("moves.json file not found.", MovesetsFileName);
            }

            data = File.ReadAllText(MovesetsFileName);
            if (string.IsNullOrEmpty(data))
            {
                _logger.Error("Moveset database is null.");
            }
            else
            {
                Movesets = JsonConvert.DeserializeObject<Dictionary<int, MovesetModel>>(data);
            }
        }
    }
}