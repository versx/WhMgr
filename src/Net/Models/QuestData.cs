namespace WhMgr.Net.Models
{
    using System.Collections.Generic;

    using Newtonsoft.Json;

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

        public QuestData()
        {
            Rewards = new List<QuestRewardMessage>();
            Conditions = new List<QuestConditionMessage>();
        }
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
        EvolveIntoPokemon
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
        DaysInARow
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
        UNKNOWN = 0,
        PokeBall = 1,
        GreatBall = 2,
        UltraBall = 3,
        MasterBall = 4,
        PremierBall = 5,
        Potion = 101,
        SuperPotion = 102,
        HyperPotion = 103,
        MaxPotion = 104,
        Revive = 201,
        MaxRevive = 202,
        LuckyEgg = 301,
        IncenseOrdinary= 401,
        IncenseSpicy = 402,
        IncenseCool = 403,
        IncenseFloral = 404,
        TroyDisk = 501,
        XAttack = 602,
        XDefense = 603,
        XMiracle = 604,
        RazzBerry = 701,
        BlukBerry = 702,
        NanabBerry = 703,
        WeparBerry = 704,
        PinapBerry = 705,
        GoldenRazzBerry = 706,
        GoldenNanabBerry = 707,
        GoldenPinapBerry = 708,
        SpecialCamera = 801,
        IncubatorBasicUlimited = 901,
        IncubatorBasic = 902,
        IncubatorSuper = 903,
        PokemonStorageUpgrade = 1001,
        ItemStorageUpgrade = 1002,
        SunStone = 1101,
        KingsRock = 1102,
        MetalCoat = 1103,
        DragonScale = 1104,
        Upgrade = 1105,
        MoveRerollFastAttack = 1201,
        MoveRerollSpecialAttack = 1202,
        RareCandy = 1301,
        FreeRaidTicket = 1401,
        PaidRaidTicket = 1402,
        LegendaryRaidTicket = 1403,
        StarPiece = 1404,
        FriendGiftBox = 1405
    }

    public enum ActivityType
    {
        Unknown = 0,
        CatchPokemon,
        CatchLegendPokemon,
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

    public sealed class QuestConditionMessage
    {
        [JsonProperty("type")]
        public QuestConditionType Type { get; set; }

        [JsonProperty("info")]
        public QuestCondition Info { get; set; }
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
    }

    public sealed class QuestRewardMessage
    {
        [JsonProperty("type")]
        public QuestRewardType Type { get; set; }

        [JsonProperty("info")]
        public QuestReward Info { get; set; }
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
    }
}