﻿namespace WhMgr.Data
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text.Json.Serialization;

    using InvasionCharacter = POGOProtos.Rpc.EnumWrapper.Types.InvasionCharacter;

    using WhMgr.Common;
    using WhMgr.Configuration;
    using WhMgr.Extensions;
    using WhMgr.Services.Webhook.Models;

    public class GameMaster
    {
        public const string MasterFileName = "masterfile.json";
        public const string CpMultipliersFileName = "cpMultipliers.json";
        public const string EmojisFileName = "emojis.json";
        public const string RarityFileName = "rarity.json";
        public const string EmbedColorsFileName = "embedColors.json";

        #region Properties

        [JsonPropertyName("pokemon")]
        public IReadOnlyDictionary<uint, PokedexPokemon> Pokedex { get; set; }

        //[JsonPropertyName("moves")]
        //public IReadOnlyDictionary<int, Moveset> Movesets { get; set; }

        [JsonPropertyName("quest_conditions")]
        public IReadOnlyDictionary<string, QuestConditionModel> QuestConditions { get; set; }

        [JsonPropertyName("quest_types")]
        public IReadOnlyDictionary<int, QuestTypeModel> QuestTypes { get; set; }

        [JsonPropertyName("quest_reward_types")]
        public IReadOnlyDictionary<int, QuestRewardTypeModel> QuestRewardTypes { get; set; }

        [JsonPropertyName("throw_types")]
        public IReadOnlyDictionary<int, string> ThrowTypes { get; set; }

        [JsonPropertyName("items")]
        public IReadOnlyDictionary<int, ItemModel> Items { get; set; }

        [JsonPropertyName("invasions")]
        public IReadOnlyDictionary<InvasionCharacter, TeamRocketInvasion> GruntTypes { get; set; }

        [JsonPropertyName("types")]
        public IReadOnlyDictionary<PokemonType, PokemonTypes> PokemonTypes { get; set; }

        [JsonIgnore]
        public IReadOnlyDictionary<double, double> CpMultipliers { get; }

        [JsonIgnore]
        public Dictionary<string, ulong> Emojis { get; set; }

        [JsonIgnore]
        public Dictionary<string, string> CustomEmojis { get; set; }

        [JsonIgnore]
        public IReadOnlyDictionary<PokemonRarity, List<uint>> PokemonRarity { get; set; }

        /// <summary>
        /// Gets or sets the Discord embed colors to use for each message type
        /// </summary>
        [JsonIgnore]
        public DiscordEmbedColorsConfig DiscordEmbedColors { get; set; }

        #region Singletons

        private static GameMaster _instance;
        public static GameMaster Instance
        {
            get
            {
                if (_instance == null)
                {
                    var path = Path.Combine(Strings.DataFolder, MasterFileName);
                    _instance = LoadInit<GameMaster>(path);
                }

                return _instance;
            }
        }

        #endregion

        #endregion

        public GameMaster()
        {
            CpMultipliers = LoadInit<Dictionary<double, double>>(Path.Combine(Strings.DataFolder, CpMultipliersFileName));
            PokemonRarity = LoadInit<Dictionary<PokemonRarity, List<uint>>>(Path.Combine(Strings.DataFolder, RarityFileName));
            Emojis = new Dictionary<string, ulong>();
            CustomEmojis = LoadInit<Dictionary<string, string>>(Path.Combine(Strings.DataFolder, EmojisFileName));
            DiscordEmbedColors = LoadInit<DiscordEmbedColorsConfig>(Path.Combine(Strings.DataFolder, EmbedColorsFileName));
        }

        public static PokedexPokemon GetPokemon(uint pokemonId, uint formId = 0)
        {
            if (pokemonId == 0)
                return null;

            if (!Instance.Pokedex.ContainsKey(pokemonId))
            {
                Console.WriteLine($"[Warning] Pokemon {pokemonId} does not exist in {MasterFileName}, please use an updated version.");
                return null;
            }

            var pkmn = Instance.Pokedex[pokemonId];
            var useForm = /*!pkmn.Attack.HasValue &&*/ formId > 0 && pkmn.Forms.ContainsKey(formId);
            var pkmnForm = useForm ? pkmn.Forms[formId] : pkmn;
            pkmnForm.Name = pkmn.Name;
            return pkmnForm;
        }

        public static void ReloadMasterFile()
        {
            var path = Path.Combine(Strings.DataFolder, MasterFileName);
            _instance = LoadInit<GameMaster>(path);
        }

        public static T LoadInit<T>(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"{filePath} file not found.", filePath);
            }

            var data = File.ReadAllText(filePath);
            if (string.IsNullOrEmpty(data))
            {
                Console.WriteLine($"{filePath} database is empty.");
                return default;
            }

            return data.FromJson<T>();
        }
    }

    public class PokemonTypes
    {
        [JsonPropertyName("immunes")]
        public List<PokemonType> Immune { get; set; }

        [JsonPropertyName("weaknesses")]
        public List<PokemonType> Weaknesses { get; set; }

        [JsonPropertyName("resistances")]
        public List<PokemonType> Resistances { get; set; }

        [JsonPropertyName("strengths")]
        public List<PokemonType> Strengths { get; set; }

        public PokemonTypes()
        {
            Immune = new List<PokemonType>();
            Weaknesses = new List<PokemonType>();
            Resistances = new List<PokemonType>();
            Strengths = new List<PokemonType>();
        }
    }

    public class Emoji
    {
        public ulong GuildId { get; set; }

        public string Name { get; set; }

        public ulong Id { get; set; }
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum PokemonRarity
    {
        Common,
        Rare
    }

    public class ItemModel
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("proto")]
        public string ProtoName { get; set; }

        [JsonPropertyName("min_trainer_level")]
        public int MinimumTrainerLevel { get; set; }
    }

    public class QuestTypeModel
    {
        [JsonPropertyName("prototext")]
        public string ProtoText { get; set; }

        [JsonPropertyName("text")]
        public string Text { get; set; }
    }

    public class QuestConditionModel
    {
        [JsonPropertyName("prototext")]
        public string ProtoText { get; set; }

        [JsonPropertyName("text")]
        public string Text { get; set; }
    }

    public class QuestRewardTypeModel
    {
        [JsonPropertyName("prototext")]
        public string ProtoText { get; set; }

        [JsonPropertyName("text")]
        public string Text { get; set; }
    }
}
