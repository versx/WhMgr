namespace WhMgr.Data
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using Newtonsoft.Json;
    using ServiceStack.DataAnnotations;

    using WhMgr.Data.Models;
    using WhMgr.Diagnostics;
    using WhMgr.Net.Models;

    public class MasterFile
    {
        const string MasterFileName = "masterfile.json";
        const string CpMultipliersFileName = "cpMultipliers.json";
        const string GreatPvPLibFileName = "pvp_great_league_ranks.json";
        const string UltraPvPLibFileName = "pvp_ultra_league_ranks.json";
        const string EmojisFileName = "emojis.json";
        const string ShinyFileName = "shiny.json";
        const string GruntTypesFileName = "grunttype.json";

        private static readonly IEventLogger _logger = EventLogger.GetLogger("MASTER");

        #region Properties

        [JsonProperty("pokemon")]
        public IReadOnlyDictionary<int, PokedexPokemon> Pokedex { get; set; }

        [JsonProperty("moves")]
        public IReadOnlyDictionary<int, Moveset> Movesets { get; set; }

        [JsonProperty("items")]
        public IReadOnlyDictionary<string, string> ItemsText { get; set; }

        [JsonProperty("types")]
        public IReadOnlyDictionary<int, PokemonType> Types { get; set; }

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

        [JsonIgnore]
        public IReadOnlyDictionary<string, ulong> Emojis { get; set; }

        [JsonIgnore]
        public IReadOnlyDictionary<InvasionGruntType, TeamRocketInvasion> GruntTypes { get; set; }

        [JsonIgnore]
        public List<int> PossibleShinies { get; set; }

        [JsonIgnore]
        public GreatPvpRankLibrary GreatPvPLibrary { get; set; }

        [JsonIgnore]
        public UltraPvpRankLibrary UltraPvPLibrary { get; set; }

        #region Singletons

        private static MasterFile _instance;
        public static MasterFile Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = LoadInit<MasterFile>(Path.Combine(Strings.DataFolder, MasterFileName), typeof(MasterFile));
                }

                return _instance;
            }
        }

        #endregion

        #endregion

        public MasterFile()
        {
            CpMultipliers = LoadInit<Dictionary<double, double>>(
                Path.Combine(Strings.DataFolder, CpMultipliersFileName),
                typeof(Dictionary<double, double>)
            );

            GreatPvPLibrary = LoadInit<GreatPvpRankLibrary>(
                Path.Combine(Strings.DataFolder, GreatPvPLibFileName), 
                typeof(GreatPvpRankLibrary)
            );

            UltraPvPLibrary = LoadInit<UltraPvpRankLibrary>(
                Path.Combine(Strings.DataFolder, UltraPvPLibFileName), 
                typeof(UltraPvpRankLibrary)
            );

            Emojis = LoadInit<Dictionary<string, ulong>>(
                Path.Combine(Strings.DataFolder, EmojisFileName), 
                typeof(Dictionary<string, ulong>)
            );

            GruntTypes = LoadInit<Dictionary<InvasionGruntType, TeamRocketInvasion>>(
                Path.Combine(Strings.DataFolder, GruntTypesFileName),
                typeof(Dictionary<InvasionGruntType, TeamRocketInvasion>)
            );

            PossibleShinies = LoadInit<List<int>>(
                Path.Combine(Strings.DataFolder, ShinyFileName),
                typeof(List<int>)
            );
        }

        public static PokedexPokemon GetPokemon(int pokemonId, int formId)
        {
            if (!Instance.Pokedex.ContainsKey(pokemonId))
                return null;

            var pkmn = Instance.Pokedex[pokemonId];
            var useForm = !pkmn.Attack.HasValue && formId > 0 && pkmn.Forms.ContainsKey(formId);
            var pkmnForm = useForm ? pkmn.Forms[formId] : pkmn;
            pkmnForm.Name = pkmn.Name;
            return pkmnForm;
        }

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