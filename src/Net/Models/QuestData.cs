namespace WhMgr.Net.Models
{
    using System;
    using System.Collections.Generic;

    using DSharpPlus.Entities;
    using Newtonsoft.Json;

    using WhMgr.Alarms.Alerts;
    using WhMgr.Alarms.Models;
    using WhMgr.Configuration;
    using WhMgr.Extensions;
    using WhMgr.Utilities;

    /*
[
	{
		"type":"quest",
		"message":
		{
			"conditions":[],
			"latitude":34.072844,
			"template":"challenge_gym_try_easy_pkmn",
			"rewards":
			[
				{
					"info":
					{
						"pokemon_id":56,
						"costume_id":0,
						"shiny":false,
						"ditto":false,
						"gender_id":0,
						"form_id":0
					},
					"type":7
				}
			],
			"pokestop_id":"b47856e4583849c2a494691b654b40f1.16",
			"pokestop_url":"http://lh5.ggpht.com/l_pBhHYciTp9yCjC6idcLUNaie9pNXn4j89oODbNVm7BZZg22PimZ3YUPtNcgS4RLJFz3_W56PihRRaLRGEm",
			"pokestop_name":"Business Center Fountain",
			"target":1,
			"updated":1540863910,
			"type":7,
			"longitude":-117.562695
		}
	}
]

{
    "message":
    {
        "rewards":
        [
            {
                "info":
                {
                    "costume_id":0,
                    "form_id":0,
                    "gender_id":0,
                    "shiny":false,
                    "pokemon_id":327,
                    "ditto":false
                },
                "type":7
            }
        ],
        "pokestop_id":"73d7a307b0264316b104470fd37cb4f5.16",
        "updated":1541350000,
        "longitude":-117.667454,
        "pokestop_url":"",
        "type":16,
        "pokestop_name":"Unknown",
        "latitude":34.103494,
        "template":"challenge_november_land_nice_curveball_plural",
        "conditions":
        [
            {
                "info":
                {
                    "throw_type_id":10,
                    "hit":false
                },
                "type":8
            },
            {
                "type":15
            }
        ],
        "target":3
    },
    "type":"quest"
}

"conditions":
[{
    "info":
    {
        "pokemon_type_ids":[8]
    },
    "type":1
}],
{
"conditions":
[{
    "info":
    {
        "pokemon_ids":[355,353]
    },
    "type":2
}]
     */

    public sealed class QuestData
    {
        public const string WebHookHeader = "quest";

        //private static readonly IEventLogger _logger = EventLogger.GetLogger();

        #region Properties

        [JsonProperty("pokestop_id")]
        public string PokestopId { get; set; }

        [JsonProperty("latitude")]
        public double Latitude { get; set; }

        [JsonProperty("longitude")]
        public double Longitude { get; set; }

        [JsonProperty("pokestop_name")]
        public string PokestopName { get; set; }

        [JsonProperty("pokestop_url")]
        public string PokestopUrl { get; set; }

        [JsonProperty("type")]
        public QuestType Type { get; set; }

        [JsonProperty("target")]
        public int Target { get; set; }

        [JsonProperty("template")]
        public string Template { get; set; }

        [JsonProperty("updated")]
        public long Updated { get; set; }

        [JsonProperty("rewards")]
        public List<QuestRewardMessage> Rewards { get; set; }

        [JsonProperty("conditions")]
        public List<QuestConditionMessage> Conditions { get; set; }

        [JsonIgnore]
        public TimeSpan TimeLeft => DateTime.Today.AddDays(1) - DateTime.Now;

        [JsonIgnore]
        public bool IsDitto => Rewards?[0]?.Info?.Ditto ?? false;

        [JsonIgnore]
        public bool IsShiny => Rewards?[0]?.Info?.Shiny ?? false;

        #endregion

        public QuestData()
        {
            Rewards = new List<QuestRewardMessage>();
            Conditions = new List<QuestConditionMessage>();
        }

        public DiscordEmbed GenerateQuestMessage(WhConfig whConfig, AlarmObject alarm, string city)
        {
            var alertType = AlertMessageType.Quests;
            var alert = alarm?.Alerts[alertType] ?? AlertMessage.Defaults[alertType];
            var properties = GetProperties(whConfig, city);
            var eb = new DiscordEmbedBuilder
            {
                Title = DynamicReplacementEngine.ReplaceText(alert.Title, properties),
                Url = DynamicReplacementEngine.ReplaceText(alert.Url, properties),
                ImageUrl = properties["tilemaps_url"],
                ThumbnailUrl = PokestopUrl,
                Description = DynamicReplacementEngine.ReplaceText(alert.Content, properties),
                Color = DiscordColor.Orange
            };
            return eb.Build();
        }

        private IReadOnlyDictionary<string, string> GetProperties(WhConfig whConfig, string city)
        {
            var questMessage = this.GetQuestMessage();
            var questConditions = this.GetConditions();
            var questReward = this.GetReward();
            var gmapsLink = string.Format(Strings.GoogleMaps, Latitude, Longitude);
            var appleMapsLink = string.Format(Strings.AppleMaps, Latitude, Longitude);
            var staticMapLink = string.Format(whConfig.Urls.StaticMap, Latitude, Longitude);
            var gmapsLocationLink = string.IsNullOrEmpty(whConfig.ShortUrlApiUrl) ? gmapsLink : NetUtil.CreateShortUrl(whConfig.ShortUrlApiUrl, gmapsLink);
            var appleMapsLocationLink = string.IsNullOrEmpty(whConfig.ShortUrlApiUrl) ? appleMapsLink : NetUtil.CreateShortUrl(whConfig.ShortUrlApiUrl, appleMapsLink);
            var gmapsStaticMapLink = string.IsNullOrEmpty(whConfig.ShortUrlApiUrl) ? staticMapLink : NetUtil.CreateShortUrl(whConfig.ShortUrlApiUrl, staticMapLink);

            const string defaultMissingValue = "?";
            var dict = new Dictionary<string, string>
            {
                //Main properties
                { "quest_task", questMessage },
                { "quest_conditions", questConditions },
                { "quest_reward", questReward },
                { "is_ditto", IsDitto ? "Yes" : "No" },
                { "is_shiny", IsShiny ? "Yes" : "No" },

                //Location properties
                { "geofence", city ?? defaultMissingValue },
                { "lat", Latitude.ToString() },
                { "lng", Longitude.ToString() },
                { "lat_5", Math.Round(Latitude, 5).ToString() },
                { "lng_5", Math.Round(Longitude, 5).ToString() },

                //Location links
                { "tilemaps_url", gmapsStaticMapLink },
                { "gmaps_url", gmapsLocationLink },
                { "applemaps_url", appleMapsLocationLink },

                //Pokestop properties
                { "pokestop_id", PokestopId ?? defaultMissingValue },
                { "pokestop_name", PokestopName ?? defaultMissingValue },
                { "pokestop_url", PokestopUrl ?? defaultMissingValue },

                //Misc properties
                { "br", "\r\n" }
            };
            return dict;
        }
    }

    public sealed class QuestConditionMessage
    {
        [JsonProperty("type")]
        public QuestConditionType Type { get; set; }

        [JsonProperty("info")]
        public QuestCondition Info { get; set; }

        public QuestConditionMessage()
        {
            Type = QuestConditionType.Unset;
        }
    }

    public sealed class QuestCondition
    {
        [JsonProperty("pokemon_ids")]
        public List<int> PokemonIds { get; set; }

        [JsonProperty("category_name")]
        public string CategoryName { get; set; }

        [JsonProperty("pokemon_type_ids")]
        public List<int> PokemonTypeIds { get; set; }

        [JsonProperty("throw_type_id")]
        public ActivityType ThrowTypeId { get; set; }

        [JsonProperty("hit")]
        public bool Hit { get; set; }

        [JsonProperty("raid_levels")]
        public List<int> RaidLevels { get; set; }

        public QuestCondition()
        {
            ThrowTypeId = ActivityType.Unknown;
        }
    }

    public sealed class QuestRewardMessage
    {
        [JsonProperty("type")]
        public QuestRewardType Type { get; set; }

        [JsonProperty("info")]
        public QuestReward Info { get; set; }

        public QuestRewardMessage()
        {
            Type = QuestRewardType.Unset;
        }
    }

    public sealed class QuestReward
    {
        [JsonProperty("pokemon_id")]
        public int PokemonId { get; set; }

        [JsonProperty("costume_id")]
        public int CostumeId { get; set; }

        [JsonProperty("form_id")]
        public int FormId { get; set; }

        [JsonProperty("gender_id")]
        public int GenderId { get; set; }

        [JsonProperty("ditto")]
        public bool Ditto { get; set; }

        [JsonProperty("shiny")]
        public bool Shiny { get; set; }

        [JsonProperty("amount")]
        public int Amount { get; set; }

        [JsonProperty("item_id")]
        public ItemId Item { get; set; }

        [JsonProperty("raid_levels")]
        public List<int> RaidLevels { get; set; }
    }

    public enum QuestType
    {
        Unknown = 0,
        FirstCatchOfTheDay,
        FirstPokestopOfTheDay,
        MultiPart,
        CatchPokemon,
        SpinPokestop,
        HatchEgg,
        CompleteGymBattle,
        CompleteRaidBattle,
        CompleteQuest,
        TransferPokemon,
        FavoritePokemon,
        AutoComplete,
        UseBerryInEncounter,
        UpgradePokemon,
        EvolvePokemon,
        LandThrow,
        GetBuddyCandy,
        BadgeRank,
        PlayerLevel,
        JoinRaid,
        CompleteBattle,
        AddFriend,
        TradePokemon,
        SendGift,
        EvolveIntoPokemon,
        Quest26NotKnown,
        CompleteCombat,
        TakeSnapshot,
        BattleTeamRocket,
        PurifyPokemon
    }

    public enum QuestRewardType
    {
        Unset = 0,
        Experience,
        Item,
        Stardust,
        Candy,
        AvatarClothing,
        Quest,
        PokemonEncounter
    }

    public enum QuestConditionType
    {
        Unset = 0,
        PokemonType,
        PokemonCategory,
        WeatherBoost,
        DailyCaptureBonus,
        DailySpinBonus,
        WinRaidStatus,
        RaidLevel,
        ThrowType,
        WinGymBattleStatus,
        SuperEffectiveCharge,
        Item,
        UniquePokestop,
        QuestContext,
        ThrowTypeInARow,
        CurveBall,
        BadgeType,
        PlayerLevel,
        WinBattleStatus,
        NewFriend,
        DaysInARow,
        UniquePokemon,
        NpcCombat,
        PvpCombat,
        Location,
        Distance,
        PokemonAlignment,
        InvasionsCharacter
    }

    public enum PokemonType
    {
        None = 0,
        Normal,
        Fighting,
        Flying,
        Poison,
        Ground,
        Rock,
        Bug,
        Ghost,
        Steel,
        Fire,
        Water,
        Grass,
        Electric,
        Psychic,
        Ice,
        Dragon,
        Dark,
        Fairy
    }

    public enum ItemId
    {
        Unknown = 0,
        Poke_Ball = 1,
        Great_Ball = 2,
        Ultra_Ball = 3,
        Master_Ball = 4,
        Premier_Ball = 5,
        Potion = 101,
        Super_Potion = 102,
        Hyper_Potion = 103,
        Max_Potion = 104,
        Revive = 201,
        Max_Revive = 202,
        Lucky_Egg = 301,
        Incense_Ordinary = 401,
        Incense_Spicy = 402,
        Incense_Cool = 403,
        Incense_Floral = 404,
        Troy_Disk = 501,
        X_Attack = 602,
        X_Defense = 603,
        X_Miracle = 604,
        Razz_Berry = 701,
        Bluk_Berry = 702,
        Nanab_Berry = 703,
        Wepar_Berry = 704,
        Pinap_Berry = 705,
        Golden_Razz_Berry = 706,
        Golden_Nanab_Berry = 707,
        Golden_Pinap_Berry = 708,
        Special_Camera = 801,
        Incubator_Basic_Unlimited = 901,
        Incubator_Basic = 902,
        Incubator_Super = 903,
        Pokemon_Storage_Upgrade = 1001,
        Item_Storage_Upgrade = 1002,
        Sun_Stone = 1101,
        Kings_Rock = 1102,
        Metal_Coat = 1103,
        Dragon_Scale = 1104,
        Upgrade = 1105,
        Move_Reroll_Fast_Attack = 1201,
        Move_Reroll_Special_Attack = 1202,
        Rare_Candy = 1301,
        Free_Raid_Ticket = 1401,
        Paid_Raid_Ticket = 1402,
        Legendary_Raid_Ticket = 1403,
        Star_Piece = 1404,
        Friend_Gift_Box = 1405
    }

    public enum ActivityType
    {
        Unknown = 0,
        CatchPokemon,
        CatchLegendyPokemon,
        FleePokemon,
        DefeatFort,
        EvolvePokemon,
        HatchEgg,
        WalkKm,
        PokedexEntryNew,
        CatchFirstThrow,
        CatchNiceThrow,
        CatchGreatThrow,
        CatchExcellentThrow,
        CatchCurveThrow,
        CatchFirstCatchOfDay,
        CatchMilestone,
        TrainPokemon,
        SearchFort,
        ReleasePokemon,
        HatchEggSmallBonus,
        HatchEggMediumBonus,
        HatchEggLargeBonus,
        DEFEAT_GYM_DEFENDER,
        DEFEAT_GYM_LEADER,
        CatchFirstCatchStreakBonus,
        SearchFortFirstOfTheDay,
        SearchFortStreakBonus,
        DefeatRaidPokemon,
        FeedBerry,
        SearchGym,
        NewPokestop,
        GymBattleLoss,
        CatchARPlusBonus,
        CatchQuestPokemonEncounter,
        FriendshipLevelUp0,
        FriendshipLevelUp1,
        FriendshipLevelUp2,
        FriendshipLevelUp3,
        FriendshipLevelUp4,
        SendGift,
        ShareExRaidPass,
        RraidLevel1AdditionalXP,
        RraidLevel2AdditionalXP,
        RraidLevel3AdditionalXP,
        RraidLevel4AdditionalXP,
        RraidLevel5AdditionalXP
    }

    public enum PokemonAlignment
    {
        Shadow = 1, //alignment_1
        Purified //alignment_2
    }

    public enum CharacterCategory
    {
        TeamLeader = 1, //character_category_1
        Grunt //character_category_2
    }
}