namespace WhMgr.Net.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Newtonsoft.Json;

    using WhMgr.Data;
    using WhMgr.Diagnostics;
    using WhMgr.Extensions;

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

        [JsonIgnore]
        public TimeSpan TimeLeft => DateTime.Today.AddDays(1) - DateTime.Now;

        [JsonIgnore]
        public bool IsDitto => Rewards?[0]?.Info?.Ditto ?? false;

        [JsonIgnore]
        public bool IsShiny => Rewards?[0]?.Info?.Shiny ?? false;

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
                    return $"Add {Target} Friends";
                case QuestType.AutoComplete:
                    return $"Autocomplete";
                case QuestType.BadgeRank:
                    return $"Get {Target} Badge(s)";
                case QuestType.CatchPokemon:
                    return $"Catch {Target} Pokemon";
                case QuestType.CompleteBattle:
                    return $"Complete {Target} Battles";
                case QuestType.CompleteGymBattle:
                    return $"Complete {Target} Gym Battles";
                case QuestType.CompleteQuest:
                    return $"Complete {Target} Quests";
                case QuestType.CompleteRaidBattle:
                    return $"Complete {Target} Raid Battles";
                case QuestType.EvolveIntoPokemon:
                    return $"Evolve {Target} Into Specific Pokemon";
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
                    return $"Hatch {Target} Eggs";
                case QuestType.JoinRaid:
                    return $"Join {Target} Raid Battles";
                case QuestType.LandThrow:
                    return $"Land {Target} Throws";
                case QuestType.MultiPart:
                    return "Multi Part Quest";
                case QuestType.PlayerLevel:
                    return $"Reach level {Target}"; ;
                case QuestType.SendGift:
                    return $"Send {Target} Gifts";
                case QuestType.SpinPokestop:
                    return $"Spin {Target} Pokestops";
                case QuestType.TradePokemon:
                    return $"Trade {Target} Pokemon";
                case QuestType.TransferPokemon:
                    return $"Transfer {Target} Pokemon";
                case QuestType.UpgradePokemon:
                    return $"Power up {Target} Pokemon";
                case QuestType.UseBerryInEncounter:
                    return $"Use {Target} Berries on Pokemon";
                case QuestType.Unknown:
                    return $"Unknown";
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
                    return (IsDitto ? 132 : Rewards[0].Info.PokemonId).GetPokemonImage((PokemonGender)Rewards?[0].Info.GenderId, Rewards?[0].Info.FormId.ToString());
                //string.Format(Strings.PokemonImage, Rewards[0].Info.Ditto ? 132 : Rewards[0].Info.PokemonId, 0);
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

            var list = new List<string>();
            for (var i = 0; i < Conditions.Count; i++)
            {
                var condition = Conditions[i];
                try
                {
                    switch (condition.Type)
                    {
                        case QuestConditionType.BadgeType:
                            break;
                        case QuestConditionType.CurveBall:
                            list.Add("Curve ball");
                            break;
                        case QuestConditionType.DailyCaptureBonus:
                            list.Add("Daily catch");
                            break;
                        case QuestConditionType.DailySpinBonus:
                            list.Add("Daily spin");
                            break;
                        case QuestConditionType.DaysInARow:
                            break;
                        case QuestConditionType.Item:
                            list.Add("Use item");
                            break;
                        case QuestConditionType.NewFriend:
                            list.Add("Make new friend");
                            break;
                        case QuestConditionType.PlayerLevel:
                            list.Add("Reach level");
                            break;
                        case QuestConditionType.PokemonCategory:
                            list.Add(string.Join(", ", condition.Info.PokemonIds?.Select(x => Database.Instance.Pokemon[x].Name).ToList()));
                            break;
                        case QuestConditionType.PokemonType:
                            list.Add(string.Join(", ", condition.Info.PokemonTypeIds?.Select(x => Convert.ToString((PokemonType)x))) + "-type");
                            break;
                        case QuestConditionType.QuestContext:
                            break;
                        case QuestConditionType.RaidLevel:
                            list.Add("Level " + string.Join(", ", condition.Info.RaidLevels));
                            break;
                        case QuestConditionType.SuperEffectiveCharge:
                            list.Add("Super effective charge move");
                            break;
                        case QuestConditionType.ThrowType:
                            list.Add(GetThrowName(condition.Info.ThrowTypeId));
                            break;
                        case QuestConditionType.ThrowTypeInARow:
                            list.Add(GetThrowName(condition.Info.ThrowTypeId) + " in a row");
                            break;
                        case QuestConditionType.UniquePokestop:
                            list.Add("Unique");
                            break;
                        case QuestConditionType.WeatherBoost:
                            list.Add("Weather boosted");
                            break;
                        case QuestConditionType.WinBattleStatus:
                            list.Add("Win battle status");
                            break;
                        case QuestConditionType.WinGymBattleStatus:
                            list.Add("Win gym battle");
                            break;
                        case QuestConditionType.WinRaidStatus:
                            list.Add("Win raid");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                    list.Add(condition?.Type.ToString());
                }
            }

            return string.Join(", ", list);
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
                    return $"{Rewards[0]?.Info.Amount.ToString("N0")} XP";
                case QuestRewardType.Item:
                    return $"{Rewards[0]?.Info.Amount.ToString("N0")} {Rewards[0]?.Info.Item.ToString().Replace("_", " ")}";
                case QuestRewardType.PokemonEncounter:
                    return IsShiny ? $"**SHINY** " : Database.Instance.Pokemon[IsDitto ? 132 : Rewards[0].Info.PokemonId].Name;
                case QuestRewardType.Quest:
                    return "Quest";
                case QuestRewardType.Stardust:
                    return $"{Rewards[0]?.Info.Amount.ToString("N0")} Stardust";
            }

            return "Unknown";
        }

        private string GetThrowName(ActivityType throwTypeId)
        {
            switch (throwTypeId)
            {
                case ActivityType.CatchCurveThrow:
                    return "Curve throw";
                case ActivityType.CatchExcellentThrow:
                    return "Excellent throw";
                case ActivityType.CatchFirstThrow:
                    return "First throw";
                case ActivityType.CatchGreatThrow:
                    return "Great throw";
                case ActivityType.CatchNiceThrow:
                    return "Nice throw";
            }

            return throwTypeId.ToString();
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