namespace WhMgr.Services.Icons
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;

    using WhMgr.Common;
    using WhMgr.Configuration;
    using WhMgr.Extensions;
    using WhMgr.Services.Icons.Models;
    using WhMgr.Services.Webhook.Models;
    using WhMgr.Utilities;

    using Gender = POGOProtos.Rpc.PokemonDisplayProto.Types.Gender;
    using InvasionCharacter = POGOProtos.Rpc.EnumWrapper.Types.InvasionCharacter;
    using QuestRewardType = POGOProtos.Rpc.QuestRewardProto.Types.Type;

    /// <summary>
    /// 
    /// </summary>
    public class UIconService : IUIconService
    {
        // TODO: Get file names instead of specifying icon formats
        private const string DefaultIconFormat = "png";
        private const string IndexJson = "index.json";
        //private const string BaseUrl = "https://raw.githubusercontent.com/WatWowMap/wwm-uicons/main/";

        #region Variables

        private static IUIconService _instance;

        private readonly IconStyleCollection _iconStyles;
        private readonly Dictionary<QuestRewardType, string> _questRewardTypes;

        #endregion

        #region Properties

        public static IUIconService Instance =>
            _instance ??= new UIconService(
                Startup.Config.IconStyles,
                GetQuestRewardTypeNames(),
                DefaultIconFormat
            );

        /// <summary>
        /// 
        /// </summary>
        public string IconFormat { get; private set; } = DefaultIconFormat;

        #endregion

        #region Constructor(s)

        /// <summary>
        /// 
        /// </summary>
        /// <param name="icons"></param>
        /// <param name="questRewardTypes"></param>
        /// <param name="iconFormat"></param>
        public UIconService(IconStyleCollection icons, Dictionary<QuestRewardType, string> questRewardTypes, string iconFormat = DefaultIconFormat)
        {
            if (icons == null)
            {
                throw new ArgumentNullException(nameof(icons), "Icons collection cannot be null");
            }

            _iconStyles = new IconStyleCollection();
            _questRewardTypes = questRewardTypes;

            IconFormat = iconFormat;

            FetchIcons(icons);
        }

        #endregion

        #region Public Methods

        #region Get Icon Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pokemonId"></param>
        /// <param name="formId"></param>
        /// <param name="evolutionId"></param>
        /// <param name="gender"></param>
        /// <param name="costumeId"></param>
        /// <param name="shiny"></param>
        /// <returns></returns>
        public string GetPokemonIcon(string style, uint pokemonId, uint formId = 0, uint evolutionId = 0, Gender gender = 0, uint costumeId = 0, bool shiny = false)
        {
            if (!IsStyleSelected(style, IconType.Pokemon))
                return GetDefaultIcon();

            var iconStyle = _iconStyles[style][IconType.Pokemon];
            var baseUrl = iconStyle.Path;
            var evolutionSuffixes = (evolutionId > 0 ? new[] { "_e" + evolutionId, string.Empty } : new[] { string.Empty }).ToList();
            var formSuffixes = (formId > 0 ? new[] { "_f" + formId, string.Empty } : new[] { string.Empty }).ToList();
            var costumeSuffixes = (costumeId > 0 ? new[] { "_c" + costumeId, string.Empty } : new[] { string.Empty }).ToList();
            var genderSuffixes = (gender > 0 ? new[] { "_g" + (int)gender, string.Empty } : new[] { string.Empty }).ToList();
            var shinySuffixes = (shiny ? new[] { "_s", string.Empty } : new[] { string.Empty }).ToList();
            foreach (var evolutionSuffix in evolutionSuffixes)
            {
                foreach (var formSuffix in formSuffixes)
                {
                    foreach (var costumeSuffix in costumeSuffixes)
                    {
                        foreach (var genderSuffix in genderSuffixes)
                        {
                            foreach (var shinySuffix in shinySuffixes)
                            {
                                var result = $"{pokemonId}{evolutionSuffix}{formSuffix}{costumeSuffix}{genderSuffix}{shinySuffix}.{IconFormat}";
                                if (iconStyle.IndexList.Contains(result))
                                {
                                    var subFolder = GetSubFolder(IconType.Pokemon);
                                    return $"{baseUrl}/{subFolder}/{result}";
                                }
                            }
                        }
                    }
                }
            }
            return GetDefaultIcon(baseUrl); // Substitute Pokemon
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public string GetTypeIcon(string style, PokemonType type)
        {
            if (!IsStyleSelected(style, IconType.Type))
                return GetDefaultIcon();

            var iconStyle = _iconStyles[style][IconType.Type];
            var baseUrl = iconStyle.Path;
            var typeId = (uint)type;
            var result = $"{typeId}.{IconFormat}";
            if (iconStyle.IndexList.Contains(result))
            {
                return $"{baseUrl}/{result}";
            }
            return GetDefaultIcon(baseUrl);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lure"></param>
        /// <param name="invasionActive"></param>
        /// <param name="questActive"></param>
        /// <returns></returns>
        public string GetPokestopIcon(string style, PokestopLureType lure, bool invasionActive = false, bool questActive = false, bool ar = false)
        {
            if (!IsStyleSelected(style, IconType.Pokestop))
                return GetDefaultIcon();

            var iconStyle = _iconStyles[style][IconType.Pokestop];
            var baseUrl = iconStyle.Path;
            var lureId = (uint)lure;
            var invasionSuffixes = (invasionActive ? new[] { "_i", string.Empty } : new[] { string.Empty }).ToList();
            var questSuffixes = (questActive ? new[] { "_q", string.Empty } : new[] { string.Empty }).ToList();
            var arSuffixes = (ar ? new[] { "_ar", string.Empty } : new[] { string.Empty }).ToList();
            foreach (var invasionSuffix in invasionSuffixes)
            {
                foreach (var questSuffix in questSuffixes)
                {
                    foreach (var arSuffix in arSuffixes)
                    {
                        var result = $"{lureId}{questSuffix}{invasionSuffix}{arSuffix}.{IconFormat}";
                        if (iconStyle.IndexList.Contains(result))
                        {
                            return $"{baseUrl}/{result}";
                        }
                    }
                }
            }
            return GetDefaultIcon(baseUrl);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rewardType"></param>
        /// <param name="id"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public string GetRewardIcon(string style, QuestRewardType rewardType, uint id = 0, uint amount = 0)
        {
            if (!IsStyleSelected(style, IconType.Reward))
                return GetDefaultIcon();

            var category = _questRewardTypes[rewardType];
            var iconStyle = _iconStyles[style][IconType.Reward];
            var baseUrl = $"{iconStyle.Path}/{category}";
            var amountSuffixes = (amount > 1 ? new[] { "_a" + amount } : new[] { string.Empty }).ToList();
            foreach (var amountSuffix in amountSuffixes)
            {
                var idString = id > 0 ? id.ToString() : string.Empty;
                var result = $"{idString}{amountSuffix}.{IconFormat}";
                if (!iconStyle.BaseIndexList.Rewards.ContainsKey(category))
                    continue;

                if (category == _questRewardTypes[QuestRewardType.PokemonEncounter])
                {
                    if (iconStyle.BaseIndexList.Pokemon.Contains(result))
                    {
                        return $"{baseUrl}/{result}";
                    }
                }
                else
                {
                    if (iconStyle.BaseIndexList.Rewards.ContainsKey(category))
                    {
                        if (iconStyle.BaseIndexList.Rewards[category].Contains(result))
                        {
                            return $"{baseUrl}/{result}";
                        }
                    }
                }
                /*
                var list = category == "pokemon_encounter"
                    ? iconStyle.BaseIndexList.Pokemon
                    : iconStyle.BaseIndexList.Rewards[category];
                if (list.Contains(result))
                {
                    return $"{baseUrl}/{result}";
                }
                */
            }
            return GetDefaultIcon(baseUrl);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="style"></param>
        /// <param name="quest"></param>
        /// <returns></returns>
        public string GetRewardIcon(string style, QuestData quest)
        {
            if (!IsStyleSelected(style, IconType.Reward))
                return GetDefaultIcon();

            var reward = quest.Rewards.FirstOrDefault();
            var category = _questRewardTypes[reward.Type];
            var iconStyle = _iconStyles[style][IconType.Reward];
            var baseUrl = $"{iconStyle.Path}/{category}";

            bool appendExt;
            var sb = new StringBuilder();
            //sb.Append("reward/");
            switch (reward.Type)
            {
                case QuestRewardType.Candy:
                case QuestRewardType.Item:
                    return GetRewardIcon(style, reward.Type, (uint)reward.Info.Item, (uint)(reward?.Info?.Amount ?? 0));
                case QuestRewardType.MegaResource:
                    return GetRewardIcon(style, reward.Type, reward.Info.PokemonId, (uint)(reward?.Info?.Amount ?? 0));
                case QuestRewardType.PokemonEncounter:
                    return GetPokemonIcon
                    (
                        style,
                        quest.IsDitto ? 132 : reward.Info.PokemonId,
                        reward?.Info.FormId ?? 0,
                        0, //reward.Info.EvolutionId ?? 0,
                        Gender.Unset,
                        reward?.Info.CostumeId ?? 0,
                        reward?.Info.Shiny ?? false
                    );
                case QuestRewardType.Stardust:
                    sb.Append(reward.Info.Amount);
                    appendExt = true;
                    break;
                case QuestRewardType.LevelCap:
                case QuestRewardType.Incident:
                case QuestRewardType.XlCandy:
                case QuestRewardType.AvatarClothing:
                case QuestRewardType.Experience:
                case QuestRewardType.Quest:
                case QuestRewardType.Sticker:
                    sb.Append((int)reward.Type);
                    appendExt = true;
                    break;
                case QuestRewardType.Unset:
                default:
                    return null;
            }
            if (appendExt)
            {
                sb.Append(".");
                sb.Append(DefaultIconFormat);
            }
            var result = sb.ToString();
            var list = iconStyle.BaseIndexList.Rewards[category];
            if (list.Contains(result))
            {
                return $"{baseUrl}/{result}";
            }
            return GetDefaultIcon(baseUrl);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gruntType"></param>
        /// <returns></returns>
        public string GetInvasionIcon(string style, InvasionCharacter gruntType)
        {
            if (!IsStyleSelected(style, IconType.Invasion))
                return GetDefaultIcon();

            var iconStyle = _iconStyles[style][IconType.Invasion];
            var baseUrl = iconStyle.Path;
            var gruntTypeId = (uint)gruntType;
            var result = $"{gruntTypeId}.{IconFormat}";
            if (iconStyle.IndexList.Contains(result))
            {
                return $"{baseUrl}/{result}";
            }
            return GetDefaultIcon(baseUrl);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="team"></param>
        /// <param name="trainerCount"></param>
        /// <param name="inBattle"></param>
        /// <param name="ex"></param>
        /// <returns></returns>
        public string GetGymIcon(string style, PokemonTeam team = PokemonTeam.Neutral, uint trainerCount = 0, bool inBattle = false, bool ex = false, bool ar = false)
        {
            if (!IsStyleSelected(style, IconType.Gym))
                return GetDefaultIcon();

            var iconStyle = _iconStyles[style][IconType.Gym];
            var baseUrl = iconStyle.Path;
            var teamId = (uint)team;
            var trainerSuffixes = (trainerCount > 0 ? new[] { "_t" + trainerCount, string.Empty } : new[] { string.Empty }).ToList();
            var inBattleSuffixes = (inBattle ? new[] { "_b", string.Empty } : new[] { string.Empty }).ToList();
            var exSuffixes = (ex ? new[] { "_ex", string.Empty } : new[] { string.Empty }).ToList();
            var arSuffixes = (ar ? new[] { "_ar", string.Empty } : new[] { string.Empty }).ToList();
            foreach (var trainerSuffix in trainerSuffixes)
            {
                foreach (var inBattleSuffix in inBattleSuffixes)
                {
                    foreach (var exSuffix in exSuffixes)
                    {
                        foreach (var arSuffix in arSuffixes)
                        {
                            var result = $"{teamId}{trainerSuffix}{inBattleSuffix}{exSuffix}{arSuffix}.{IconFormat}";
                            if (iconStyle.IndexList.Contains(result))
                            {
                                return $"{baseUrl}/{result}";
                            }
                        }
                    }
                }
            }
            return GetDefaultIcon(baseUrl);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="level"></param>
        /// <param name="hatched"></param>
        /// <param name="ex"></param>
        /// <returns></returns>
        public string GetEggIcon(string style, uint level, bool hatched = false, bool ex = false)
        {
            if (!IsStyleSelected(style, IconType.Egg))
                return GetDefaultIcon();

            var iconStyle = _iconStyles[style][IconType.Egg];
            var baseUrl = iconStyle.Path;
            var hatchedSuffixes = (hatched ? new[] { "_h", string.Empty } : new[] { string.Empty }).ToList();
            var exSuffixes = (ex ? new[] { "_ex", string.Empty } : new[] { string.Empty }).ToList();
            foreach (var hatchedSuffix in hatchedSuffixes)
            {
                foreach (var exSuffix in exSuffixes)
                {
                    var result = $"{level}{hatchedSuffix}{exSuffix}.{IconFormat}";
                    if (iconStyle.IndexList.Contains(result))
                    {
                        return $"{baseUrl}/{result}";
                    }
                }
            }
            return GetDefaultIcon(baseUrl);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="team"></param>
        /// <returns></returns>
        public string GetTeamIcon(string style, PokemonTeam team = PokemonTeam.Neutral)
        {
            if (!IsStyleSelected(style, IconType.Team))
                return GetDefaultIcon();

            var iconStyle = _iconStyles[style][IconType.Team];
            var baseUrl = iconStyle.Path;
            var teamId = (uint)team;
            var result = $"{teamId}.{IconFormat}";
            if (iconStyle.IndexList.Contains(result))
            {
                return $"{baseUrl}/{result}";
            }
            return GetDefaultIcon(baseUrl);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="weatherCondition"></param>
        /// <returns></returns>
        public string GetWeatherIcon(string style, WeatherCondition weatherCondition)
        {
            if (!IsStyleSelected(style, IconType.Weather))
                return GetDefaultIcon();

            var iconStyle = _iconStyles[style][IconType.Weather];
            var baseUrl = iconStyle.Path;
            var weatherId = (uint)weatherCondition;
            var result = $"{weatherId}.{IconFormat}";
            if (iconStyle.IndexList.Contains(result))
            {
                return $"{baseUrl}/{result}";
            }
            return GetDefaultIcon(baseUrl);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public string GetNestIcon(string style, PokemonType type)
        {
            if (!IsStyleSelected(style, IconType.Nest))
                return GetDefaultIcon();

            var iconStyle = _iconStyles[style][IconType.Nest];
            var baseUrl = iconStyle.Path;
            var typeId = (uint)type;
            var result = $"{typeId}.{IconFormat}";
            if (iconStyle.IndexList.Contains(result))
            {
                return $"{baseUrl}/{result}";
            }
            return GetDefaultIcon(baseUrl);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public string GetMiscellaneousIcon(string style, string fileName)
        {
            if (!IsStyleSelected(style, IconType.Misc))
                return GetDefaultIcon();

            var iconStyle = _iconStyles[style][IconType.Misc];
            var baseUrl = iconStyle.Path;
            var result = $"{fileName}.{IconFormat}";
            if (iconStyle.IndexList.Contains(result))
            {
                return $"{baseUrl}/{result}";
            }
            return GetDefaultIcon(baseUrl);
        }

        #endregion

        public static Dictionary<QuestRewardType, string> GetQuestRewardTypeNames()
        {
            return new Dictionary<QuestRewardType, string>
            {
                { QuestRewardType.Unset, "unset" },
                { QuestRewardType.Experience, "experience" },
                { QuestRewardType.Item, "item" },
                { QuestRewardType.Stardust, "stardust" },
                { QuestRewardType.Candy, "candy" },
                { QuestRewardType.AvatarClothing, "avatar_clothing" },
                { QuestRewardType.Quest, "quest" },
                { QuestRewardType.PokemonEncounter, "pokemon_encounter" },
                { QuestRewardType.Pokecoin, "pokecoin" },
                { QuestRewardType.XlCandy, "xl_candy" },
                { QuestRewardType.LevelCap, "level_cap" },
                { QuestRewardType.Sticker, "sticker" },
                { QuestRewardType.MegaResource, "mega_resource" },
            };
        }

        #endregion

        #region Private Methods

        private void FetchIcons(IconStyleCollection icons)
        {
            foreach (var (styleName, styleConfig) in icons)
            {
                BuildIndexManifests(styleName, styleConfig);
            }
        }

        private void BuildIndexManifests(string styleName, Dictionary<IconType, IconStyleConfig> iconStyles)
        {
            // Get available forms from remote icons repo to build form list for each icon style
            var keys = iconStyles.Keys.ToList();
            for (var i = 0; i < keys.Count; i++)
            {
                var iconType = keys[i];
                var styleConfig = iconStyles[iconType];
                if (!_iconStyles.ContainsKey(styleName))
                {
                    // Style not in global icon styles
                    _iconStyles.Add(styleName, new Dictionary<IconType, IconStyleConfig>());
                }
                // Check if style icon type already checked, if so skip
                //if (_iconStyles[styleName].ContainsKey(iconType))
                //    continue;

                // Get the remote form index file from the icon repository
                var iconStyleSubFolder = GetSubFolder(iconType);
                var indexPath = Path.Combine(
                    styleConfig.Path,
                    iconStyleSubFolder,
                    IndexJson
                );
                var formsListJson = NetUtils.Get(indexPath);
                if (string.IsNullOrEmpty(formsListJson))
                {
                    // Failed to get form list, skip...
                    Console.WriteLine("Failed to download index.json or index was empty");
                    continue;
                }
                try
                {
                    //Console.WriteLine($"IndexList: {formsListJson}");

                    // Deserialize json list to hash set
                    if (iconType == IconType.Base)
                    {
                        var manifest = formsListJson.FromJson<BaseIndexManifest>();
                        styleConfig.BaseIndexList = manifest;
                        // Set all iconTypes to Base manifest values
                        foreach (var iconTypeValue in Enum.GetValues(typeof(IconType)))
                        {
                            var iconTypeBase = (IconType)iconTypeValue;
                            if (iconTypeBase == IconType.Base)
                                continue;

                            if (!iconStyles.ContainsKey(iconTypeBase))
                            {
                                iconStyles.Add(iconTypeBase, styleConfig);
                            }
                            var indexConfig = iconStyles[iconTypeBase];
                            var indexBase = new HashSet<string>();
                            //dynamic indexBase = null;
                            switch (iconTypeBase)
                            {
                                case IconType.Egg:
                                    //indexBase = manifest.Raids?.GetProperty("egg");
                                    indexBase = manifest.Raids?.Eggs;
                                    break;
                                case IconType.Gym:
                                    indexBase = manifest.Gyms;
                                    break;
                                case IconType.Invasion:
                                    indexBase = manifest.Invasions;
                                    break;
                                case IconType.Misc:
                                    indexBase = manifest.Miscellaneous;
                                    break;
                                case IconType.Nest:
                                    indexBase = manifest.Nests;
                                    break;
                                case IconType.Pokemon:
                                    indexBase = manifest.Pokemon;
                                    break;
                                case IconType.Pokestop:
                                    indexBase = manifest.Pokestops;
                                    break;
                                case IconType.Raid:
                                    //indexBase = manifest.Raids;
                                    break;
                                case IconType.Reward:
                                    //indexBase = manifest.Rewards;
                                    // TODO: Reward types
                                    break;
                                case IconType.Team:
                                    indexBase = manifest.Teams;
                                    break;
                                case IconType.Type:
                                    indexBase = manifest.Types;
                                    break;
                                case IconType.Weather:
                                    indexBase = manifest.Weather;
                                    break;
                            }
                            var config = new IconStyleConfig
                            {
                                Name = styleConfig.Name,
                                Path = Path.Combine(
                                    styleConfig.Path,
                                    GetSubFolder(iconTypeBase)
                                ),
                                IndexList = indexBase,
                                BaseIndexList = manifest,
                            };
                            _iconStyles[styleName].Add(iconTypeBase, config);
                        }
                    }
                    else
                    {
                        var formsList = formsListJson.FromJson<HashSet<string>>();
                        // Add style and form list
                        styleConfig.IndexList = formsList;
                        if (!_iconStyles[styleName].ContainsKey(iconType))
                        {
                            _iconStyles[styleName].Add(iconType, styleConfig);
                        }
                        else
                        {
                            _iconStyles[styleName][iconType] = styleConfig;
                        }
                    }
                }
                catch (Exception ex)
                {
                    //Console.WriteLine($"Failed to parse {IndexJson} for style {styleName}: {formsListJson}\nError: {ex}");
                    Console.WriteLine($"Failed to parse {IndexJson} for style {styleName}\nError: {ex}");
                }
            }
        }

        private bool IsStyleSelected(string styleName, IconType iconType)
        {
            if (!_iconStyles.ContainsKey(styleName))
            {
                // Style does not exist or styles not loaded
                return false;
            }

            if (!_iconStyles[styleName].ContainsKey(iconType) && iconType == IconType.Base)
            {
                // Style does not contain icon type style config
                return false;
            }

            return true;
        }

        private static string GetSubFolder(IconType type)
        {
            return type switch
            {
                IconType.Egg => "raid/egg",
                IconType.Gym
                    or IconType.Invasion
                    or IconType.Misc
                    or IconType.Nest
                    or IconType.Pokemon
                    or IconType.Pokestop
                    or IconType.Raid
                    or IconType.Reward
                    or IconType.Team
                    or IconType.Type
                    or IconType.Weather => type.ToString().ToLower(),
                _ => string.Empty,
            };
        }

        private string GetDefaultIcon(string baseUrl = null)
        {
            if (string.IsNullOrEmpty(baseUrl))
            {
                return $"0.{IconFormat}";
            }
            return $"{baseUrl}/0.{IconFormat}";
        }

        #endregion
    }
}