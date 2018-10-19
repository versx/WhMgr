namespace T.Data
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using Newtonsoft.Json;

    using T.Data.Models;

    public class Database
    {
        const string PokemonFileName = "pokemon.json";
        const string MovesetsFileName = "moves.json";

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
            //TODO: Check if null.
            Pokemon = JsonConvert.DeserializeObject<Dictionary<int, PokemonModel>>(data);

            if (!File.Exists(MovesetsFileName))
            {
                throw new FileNotFoundException("moves.json file not found.", MovesetsFileName);
            }

            data = File.ReadAllText(MovesetsFileName);
            //TODO: Check if null.
            Movesets = JsonConvert.DeserializeObject<Dictionary<int, MovesetModel>>(data);
        }
    }
}