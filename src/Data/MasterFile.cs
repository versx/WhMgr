namespace WhMgr.Data
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using WhMgr.Data.Models;

    public class MasterFile
    {
        const string PokedexFileName = "pokedex.json";
        const string CpMultipliersFileName = "cpMultipliers.json";

        #region Properties

        public IReadOnlyDictionary<int, PokedexPokemon> Pokedex { get; }

        public IReadOnlyDictionary<double, double> CpMultipliers { get; }

        #region Singletons

        private static MasterFile _instance;
        public static MasterFile Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new MasterFile();
                }

                return _instance;
            }
        }

        #endregion

        #endregion

        public MasterFile()
        {
            Pokedex = Database.LoadInit<Dictionary<int, PokedexPokemon>>(Path.Combine(Strings.DataFolder, PokedexFileName), typeof(Dictionary<int, PokedexPokemon>));
            CpMultipliers = Database.LoadInit<Dictionary<double, double>>(Path.Combine(Strings.DataFolder, CpMultipliersFileName), typeof(Dictionary<double, double>));
        }
    }
}