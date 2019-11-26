namespace WhMgr.Data
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using Newtonsoft.Json;

    using WhMgr.Data.Models;

    public class MasterFile
    {
        const string MasterFileName = "masterfile.json";
        const string CpMultipliersFileName = "cpMultipliers.json";

        #region Properties

        [JsonProperty("pokemon")]
        public IReadOnlyDictionary<int, PokedexPokemon> Pokedex { get; set; }

        [JsonProperty("moves")]
        public IReadOnlyDictionary<int, Moveset> Movesets { get; set; }

        [JsonProperty("items")]
        public IReadOnlyDictionary<string, string> ItemsText { get; set; }

        [JsonProperty("types")]
        public IReadOnlyDictionary<int, Net.Models.PokemonType> Types { get; set; }

        [JsonProperty("quest_condition")]
        public IReadOnlyDictionary<string, string> QuestConditions { get; set; }

        [JsonProperty("alignment")]
        public IReadOnlyDictionary<int, string> Alignment { get; set; }

        [JsonProperty("character_category")]
        public IReadOnlyDictionary<int, string> CharacterCategory { get; set; }

        [JsonProperty("throw_type")]
        public IReadOnlyDictionary<int, string> ThrowType { get; set; }

        [JsonProperty("item")]
        public IReadOnlyDictionary<int, string> Items { get; set; }

        [JsonProperty("lure")]
        public IReadOnlyDictionary<int, string> Lures { get; set; }

        [JsonProperty("cpMultipliers")]

        public IReadOnlyDictionary<double, double> CpMultipliers { get; }

        #region Singletons

        private static MasterFile _instance;
        public static MasterFile Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Database.LoadInit<MasterFile>(Path.Combine(Strings.DataFolder, MasterFileName), typeof(MasterFile));
                }

                return _instance;
            }
        }

        #endregion

        #endregion

        public MasterFile()
        {
            if (File.Exists(Path.Combine(Strings.DataFolder, CpMultipliersFileName)))
            {
                CpMultipliers = Database.LoadInit<Dictionary<double, double>>(Path.Combine(Strings.DataFolder, CpMultipliersFileName), typeof(Dictionary<double, double>));
            }
        }
    }
}