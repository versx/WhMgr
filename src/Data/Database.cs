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
        #region Constants

        const string PokemonFileName = "pokemon.json";
        const string MovesetsFileName = "moves.json";
        const string DatabaseFileName = "database.json";

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

        public Dictionary<ulong, SubscriptionObject> Subscriptions { get; }

        #endregion

        #region Constructor

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

            if (!File.Exists(DatabaseFileName))
            {
                var json = JsonConvert.SerializeObject(new Dictionary<ulong, SubscriptionObject>(), Formatting.Indented);
                File.WriteAllText(DatabaseFileName, json);
            }

            data = File.ReadAllText(DatabaseFileName);
            if (string.IsNullOrEmpty(data))
            {
                _logger.Error("Subscriptions database is null.");
            }
            else
            {
                Subscriptions = JsonConvert.DeserializeObject<Dictionary<ulong, SubscriptionObject>>(data);
            }
        }

        #endregion

        #region Public Methods

        //public bool Exists(ulong userId)
        //{
        //    return this[userId] != null;
        //}

        //public bool RemoveAllPokemon(ulong userId)
        //{
        //    if (Exists(userId))
        //    {
        //        var sub = this[userId];
        //        sub.Pokemon.Clear();
        //        return true;
        //    }

        //    return false;
        //}

        //public bool RemoveAllRaids(ulong userId)
        //{
        //    if (Exists(userId))
        //    {
        //        var sub = this[userId];
        //        sub.Raids.Clear();
        //        return true;
        //    }

        //    return false;
        //}

        //public uint PokemonIdFromName(string name)
        //{
        //    return Convert.ToUInt32(Pokemon.FirstOrDefault(x => x.Value.Name.Contains(name)).Key);
        //}

        //public string PokemonNameFromId(int id)
        //{
        //    return Pokemon.FirstOrDefault(x => x.Key == id.ToString()).Value.Name;
        //}

        /// <summary>
        /// Saves the configuration file to the default path.
        /// </summary>
        public void Save()
        {
            Save(DatabaseFileName);
        }

        /// <summary>
        /// Saves the configuration file to the specified path.
        /// </summary>
        /// <param name="filePath">The full file path.</param>
        public void Save(string filePath)
        {
            var json = JsonConvert.SerializeObject(Subscriptions, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }

        public string ToJsonString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Loads the configuration file from the default path.
        /// </summary>
        /// <returns>Returns the deserialized Config object.</returns>
        public static Database Load()
        {
            return Load(DatabaseFileName);
        }

        /// <summary>
        /// Loads the configuration file from the specified path.
        /// </summary>
        /// <param name="filePath">The full file path.</param>
        /// <returns>Returns the deserialized Config object.</returns>
        public static Database Load(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    var data = File.ReadAllText(filePath);
                    return JsonConvert.DeserializeObject<Database>(data);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }

            return new Database();
        }

        #endregion
    }
}