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
            return GetQuestMessage(quest.Type, quest.Target);
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
                case QuestType.CompleteCombat:
                    return $"Complete {target} Combat(s)";
                case QuestType.TakeSnapshot:
                    return $"Take {target} Snapshot(s)";
                case QuestType.BattleTeamRocket:
                    return $"Battle {target} Team Rocket Battle(s)";
                case QuestType.PurifyPokemon:
                    return $"Purify {target} Pokemon";
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
                    return string.Join(", ", condition.Info.PokemonIds?.Select(x => MasterFile.GetPokemon(x, 0)?.Name).ToList());
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
                case QuestConditionType.UniquePokemon:
                    return "Unique Pokemon";
                case QuestConditionType.NpcCombat:
                    return "NPC Combat";
                case QuestConditionType.PvpCombat:
                    return "PvP Combat";
                case QuestConditionType.Location:
                    return "Location";
                case QuestConditionType.Distance:
                    return "Distance";
                case QuestConditionType.PokemonAlignment:
                    return "Pokemon Alignment"; //TODO: Finish Pokemon Alignment(s): %{alignments}
                case QuestConditionType.InvasionsCharacter:
                    return "Invasion Category"; //TODO: Finish Invasion Category(s): %{categories}
            }

            return null;
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
                    return $"{amount:N0} Rare Candy";
                case QuestRewardType.Experience:
                    return $"{amount:N0} XP";
                case QuestRewardType.Item:
                    return $"{amount:N0} {item.ToString().Replace("_", " ")}";
                case QuestRewardType.PokemonEncounter:
                    return (isShiny ? $"**SHINY** " : "") + MasterFile.GetPokemon(isDitto ? 132 : pokemonId, 0)?.Name;
                case QuestRewardType.Quest:
                    return "Quest";
                case QuestRewardType.Stardust:
                    return $"{amount:N0} Stardust";
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