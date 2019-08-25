namespace WhMgr.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using WhMgr.Configuration;
    using WhMgr.Data;
    using WhMgr.Data.Models;
    using WhMgr.Net.Models;

    public static class QuestExtensions
    {
        public static string GetQuestMessage(this QuestData quest)
        {
            switch (quest.Type)
            {
                case QuestType.AddFriend:
                    return $"Add {quest.Target} Friends";
                case QuestType.AutoComplete:
                    return $"Autocomplete";
                case QuestType.BadgeRank:
                    return $"Get {quest.Target} Badge(s)";
                case QuestType.CatchPokemon:
                    return $"Catch {quest.Target} Pokemon";
                case QuestType.CompleteBattle:
                    return $"Complete {quest.Target} Battles";
                case QuestType.CompleteGymBattle:
                    return $"Complete {quest.Target} Gym Battles";
                case QuestType.CompleteQuest:
                    return $"Complete {quest.Target} Quests";
                case QuestType.CompleteRaidBattle:
                    return $"Complete {quest.Target} Raid Battles";
                case QuestType.EvolveIntoPokemon:
                    return $"Evolve {quest.Target} Into Specific Pokemon";
                case QuestType.EvolvePokemon:
                    return $"Evolve {quest.Target} Pokemon";
                case QuestType.FavoritePokemon:
                    return $"Favorite {quest.Target} Pokemon";
                case QuestType.FirstCatchOfTheDay:
                    return $"Catch first Pokemon of the day";
                case QuestType.FirstPokestopOfTheDay:
                    return $"Spin first pokestop of the day";
                case QuestType.GetBuddyCandy:
                    return $"Earn {quest.Target} candy walking with your buddy";
                case QuestType.HatchEgg:
                    return $"Hatch {quest.Target} Eggs";
                case QuestType.JoinRaid:
                    return $"Join {quest.Target} Raid Battles";
                case QuestType.LandThrow:
                    return $"Land {quest.Target} Throws";
                case QuestType.MultiPart:
                    return "Multi Part Quest";
                case QuestType.PlayerLevel:
                    return $"Reach level {quest.Target}"; ;
                case QuestType.SendGift:
                    return $"Send {quest.Target} Gifts";
                case QuestType.SpinPokestop:
                    return $"Spin {quest.Target} Pokestops";
                case QuestType.TradePokemon:
                    return $"Trade {quest.Target} Pokemon";
                case QuestType.TransferPokemon:
                    return $"Transfer {quest.Target} Pokemon";
                case QuestType.UpgradePokemon:
                    return $"Power up {quest.Target} Pokemon";
                case QuestType.UseBerryInEncounter:
                    return $"Use {quest.Target} Berries on Pokemon";
                case QuestType.Unknown:
                    return $"Unknown";
            }

            return quest.Type.ToString();
        }

        public static string GetQuestMessage(this QuestType type, int target)
        {
            switch (type)
            {
                case QuestType.AddFriend:
                    return $"Add {target} Friends";
                case QuestType.AutoComplete:
                    return $"Autocomplete";
                case QuestType.BadgeRank:
                    return $"Get {target} Badge(s)";
                case QuestType.CatchPokemon:
                    return $"Catch {target} Pokemon";
                case QuestType.CompleteBattle:
                    return $"Complete {target} Battles";
                case QuestType.CompleteGymBattle:
                    return $"Complete {target} Gym Battles";
                case QuestType.CompleteQuest:
                    return $"Complete {target} Quests";
                case QuestType.CompleteRaidBattle:
                    return $"Complete {target} Raid Battles";
                case QuestType.EvolveIntoPokemon:
                    return $"Evolve {target} Into Specific Pokemon";
                case QuestType.EvolvePokemon:
                    return $"Evolve {target} Pokemon";
                case QuestType.FavoritePokemon:
                    return $"Favorite {target} Pokemon";
                case QuestType.FirstCatchOfTheDay:
                    return $"Catch first Pokemon of the day";
                case QuestType.FirstPokestopOfTheDay:
                    return $"Spin first pokestop of the day";
                case QuestType.GetBuddyCandy:
                    return $"Earn {target} candy walking with your buddy";
                case QuestType.HatchEgg:
                    return $"Hatch {target} Eggs";
                case QuestType.JoinRaid:
                    return $"Join {target} Raid Battles";
                case QuestType.LandThrow:
                    return $"Land {target} Throws";
                case QuestType.MultiPart:
                    return "Multi Part Quest";
                case QuestType.PlayerLevel:
                    return $"Reach level {target}"; ;
                case QuestType.SendGift:
                    return $"Send {target} Gifts";
                case QuestType.SpinPokestop:
                    return $"Spin {target} Pokestops";
                case QuestType.TradePokemon:
                    return $"Trade {target} Pokemon";
                case QuestType.TransferPokemon:
                    return $"Transfer {target} Pokemon";
                case QuestType.UpgradePokemon:
                    return $"Power up {target} Pokemon";
                case QuestType.UseBerryInEncounter:
                    return $"Use {target} Berries on Pokemon";
                case QuestType.Unknown:
                    return $"Unknown";
            }

            return type.ToString();
        }

        public static string GetConditions(this QuestData quest)
        {
            return GetConditions(quest.Conditions);
        }

        public static string GetConditions(this List<QuestConditionMessage> conditions)
        {
            if (conditions == null)
                return null;

            var list = new List<string>();
            for (var i = 0; i < conditions.Count; i++)
            {
                try
                {
                    var condition = conditions[i].GetCondition();
                    if (string.IsNullOrEmpty(condition))
                        continue;

                    list.Add(condition);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    //list.Add(condition?.Type.ToString());
                }
            }

            return string.Join(", ", list);
        }

        public static string GetCondition(this QuestConditionMessage condition)
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
                    return "Days in a row";
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
                    return GetThrowName(condition.Info.ThrowTypeId);
                case QuestConditionType.ThrowTypeInARow:
                    return GetThrowName(condition.Info.ThrowTypeId) + " in a row";
                case QuestConditionType.UniquePokestop:
                    return "Unique";
                case QuestConditionType.WeatherBoost:
                    return "Weather boosted";
                case QuestConditionType.WinBattleStatus:
                    return "Win battle status";
                case QuestConditionType.WinGymBattleStatus:
                    return "Win gym battle";
                case QuestConditionType.WinRaidStatus:
                    return "Win raid";
            }

            return null;
        }

        public static string GetIconUrl(this QuestData quest, WhConfig config)
        {
            var iconIndex = 0;
            switch (quest.Rewards?[0].Type)
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
                    return string.Format(config.Urls.QuestImage, (int)quest.Rewards?[0].Info.Item);
                case QuestRewardType.PokemonEncounter:
                    return (quest.IsDitto ? 132 : quest.Rewards[0].Info.PokemonId).GetPokemonImage(config.Urls.PokemonImage, (PokemonGender)quest.Rewards?[0].Info.GenderId, quest.Rewards?[0].Info.FormId ?? 0, quest.Rewards?[0].Info.CostumeId ?? 0);
                case QuestRewardType.Quest:
                    break;
                case QuestRewardType.Stardust:
                    iconIndex = -1;
                    break;
                case QuestRewardType.Unset:
                    break;
            }

            return string.Format(config.Urls.QuestImage, iconIndex);
        }

        public static string GetReward(this QuestData quest)
        {
            return GetReward(quest.Rewards?[0]);
        }

        public static string GetReward(this QuestRewardMessage reward)
        {
            return GetReward(reward.Type, reward.Info.PokemonId, reward.Info.Amount, reward.Info.Item, reward.Info.Ditto, reward.Info.Shiny);
        }

        public static string GetReward(this Pokestop pokestop)
        {
            return pokestop.QuestRewards[0].GetReward();
        }

        public static string GetReward(this QuestRewardType type, int pokemonId, int amount, ItemId item, bool isDitto = false, bool isShiny = false)
        {
            switch (type)
            {
                case QuestRewardType.AvatarClothing:
                    return "Avatar Clothing";
                case QuestRewardType.Candy:
                    return $"{amount.ToString("N0")} Rare Candy";
                case QuestRewardType.Experience:
                    return $"{amount.ToString("N0")} XP";
                case QuestRewardType.Item:
                    return $"{amount.ToString("N0")} {item.ToString().Replace("_", " ")}";
                case QuestRewardType.PokemonEncounter:
                    return (isShiny ? $"**SHINY** " : "") + Database.Instance.Pokemon[isDitto ? 132 : pokemonId].Name;
                case QuestRewardType.Quest:
                    return "Quest";
                case QuestRewardType.Stardust:
                    return $"{amount.ToString("N0")} Stardust";
            }

            return "Unknown";
        }

        public static string GetThrowName(this ActivityType throwTypeId)
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
}