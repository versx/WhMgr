namespace WhMgr.Net.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Newtonsoft.Json;

    using WhMgr.Data;
    using WhMgr.Diagnostics;

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

    /** Quest Filtering
     * QuestFilterType
     *  - By Reward
     *    - Reward type
     *  - By Quest
     *  - Pokemon reward list to include or exclude
     *  - Item type
     *  
     */

    public sealed class QuestData
    {
        public const string WebHookHeader = "quest";

        private static readonly IEventLogger _logger = EventLogger.GetLogger();

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

        public string GetMessage()
        {
            switch (Type)
            {
                case QuestType.AddFriend:
                    return $"Add {Target} new friends";
                case QuestType.AutoComplete:
                    break;
                case QuestType.BadgeRank:
                    break;
                case QuestType.CatchPokemon:
                    return $"Catch {Target} Pokemon";
                case QuestType.CompleteBattle:
                    break;
                case QuestType.CompleteGymBattle:
                    return $"Complete {Target} gym battles";
                case QuestType.CompleteQuest:
                    return $"Complete {Target} quests";
                case QuestType.CompleteRaidBattle:
                    return $"Complete {Target} raid battles";
                case QuestType.EvolveIntoPokemon:
                    break;
                case QuestType.EvolvePokemon:
                    return $"Evolve {Target} Pokemon";
                case QuestType.FavoritePokemon:
                    return $"Favorite {Target} Pokemon";
                case QuestType.FirstCatchOfTheDay:
                    return $"Catch first Pokemon of the day";
                case QuestType.FirstPokestopOfTheDay:
                    return $"Spin first pokestop of the day";
                case QuestType.GetBuddyCandy:
                    return $"Earn {Target} candy walking with your buddy";
                case QuestType.HatchEgg:
                    return $"Hatch {Target} eggs";
                case QuestType.JoinRaid:
                    return $"Participate in {Target} raid battles";
                case QuestType.LandThrow:
                    return $"Land {Target} throws";
                case QuestType.MultiPart:
                    break;
                case QuestType.PlayerLevel:
                    return $"Reach level {Target}"; ;
                case QuestType.SendGift:
                    return $"Send {Target} gifts to friends";
                case QuestType.SpinPokestop:
                    return $"Spin {Target} Pokestops";
                case QuestType.TradePokemon:
                    return $"Trade {Target} Pokemon";
                case QuestType.TransferPokemon:
                    return $"Transfer {Target} Pokemon";
                case QuestType.UpgradePokemon:
                    return $"Power up a Pokemon {Target} times";
                case QuestType.UseBerryInEncounter:
                    return $"Use {Target} berries on Pokemon";
                case QuestType.Unknown:
                    break;
            }

            return Type.ToString();
        }

        public string GetIconUrl()
        {
            var iconIndex = 0;
            switch (Rewards[0].Type)
            {
                case QuestRewardType.AvatarClothing:
                    break;
                case QuestRewardType.Candy:
                    iconIndex = 1301;
                    break;
                case QuestRewardType.Experience:
                    iconIndex = -2;
                    break;
                case QuestRewardType.Item:
                    return string.Format(Strings.QuestImage, (int)Rewards[0].Info.Item);
                case QuestRewardType.PokemonEncounter:
                    return string.Format(Strings.PokemonImage, Rewards[0].Info.PokemonId, 0);
                case QuestRewardType.Quest:
                    break;
                case QuestRewardType.Stardust:
                    iconIndex = -1;
                    break;
                case QuestRewardType.Unset:
                    break;
            }

            return string.Format(Strings.QuestImage, iconIndex);
        }

        public string GetConditionName()
        {
            if (Conditions == null)
                return null;

            var condition = Conditions[0];
            try
            {
                switch (condition.Type)
                {
                    case QuestConditionType.BadgeType:
                        break;
                    case QuestConditionType.CurveBall:
                        return "Curve ball";
                    case QuestConditionType.DailyCaptureBonus:
                        return "Daily catch";
                    case QuestConditionType.DailySpinBonus:
                        return "Daily spin";
                    case QuestConditionType.DaysInARow:
                        break;
                    case QuestConditionType.Item:
                        return "Use item";
                    case QuestConditionType.NewFriend:
                        return "Make new friend";
                    case QuestConditionType.PlayerLevel:
                        return "Reach level";
                    case QuestConditionType.PokemonCategory:
                        return string.Join(", ", condition.Info.PokemonIds?.Select(x => Database.Instance.Pokemon[x].Name).ToList());
                    case QuestConditionType.PokemonType:
                        return string.Join(", ", condition.Info.PokemonTypeIds?.Select(x => Convert.ToString((PokemonType)x))) + "-type";
                    case QuestConditionType.QuestContext:
                        break;
                    case QuestConditionType.RaidLevel:
                        return "Level " + string.Join(", ", condition.Info.RaidLevels);
                    case QuestConditionType.SuperEffectiveCharge:
                        return "Super effective charge move";
                    case QuestConditionType.ThrowType:
                        return condition.Info.ThrowTypeId.ToString();
                    case QuestConditionType.ThrowTypeInARow:
                        return condition.Info.ThrowTypeId.ToString();
                    case QuestConditionType.UniquePokestop:
                        return "Unique";
                    case QuestConditionType.WeatherBoost:
                        return "Weather bosted";
                    case QuestConditionType.WinBattleStatus:
                        break;
                    case QuestConditionType.WinGymBattleStatus:
                        return "Win gym battle";
                    case QuestConditionType.WinRaidStatus:
                        return "Win raid";
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }

            return condition?.Type.ToString();
        }

        public string GetRewardString()
        {
            switch (Rewards[0].Type)
            {
                case QuestRewardType.AvatarClothing:
                    return "Avatar Clothing";
                case QuestRewardType.Candy:
                    return $"{Rewards[0]?.Info.Amount.ToString("N0")} Rare Candy";
                case QuestRewardType.Experience:
                    return $"{Rewards[0].Info.Amount.ToString("N0")} XP";
                case QuestRewardType.Item:
                    return $"{Rewards[0].Info.Amount.ToString("N0")} {Rewards[0].Info.Item}";
                case QuestRewardType.PokemonEncounter:
                    return Database.Instance.Pokemon[Rewards[0].Info.PokemonId].Name;
                case QuestRewardType.Quest:
                    return "Quest";
                case QuestRewardType.Stardust:
                    return $"{Rewards[0].Info.Amount.ToString("N0")} Stardust";
            }

            return "Unknown";
        }
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
        Unknown = 0,
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
        IncenseOrdinary = 401,
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
}