namespace WhMgr.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using WhMgr.Data.Models;
    using WhMgr.Localization;
    using WhMgr.Net.Models;

    public static class QuestExtensions
    {
        public static string GetQuestMessage(this QuestData quest)
        {
            return GetQuestMessage(quest.Type, quest.Target);
        }

        public static string GetQuestMessage(this QuestType type, int target)
        {
            return Translator.Instance.Translate("quest_" + Convert.ToInt32(type), target);
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
                }
            }

            return string.Join(", ", list);
        }

        public static string GetCondition(this QuestConditionMessage condition)
        {
            var conditionKey = "quest_condition_" + Convert.ToInt32(condition.Type);
            switch (condition.Type)
            {
                case QuestConditionType.PokemonCategory:
                    return string.Join(", ", condition.Info.PokemonIds?.Select(x => Translator.Instance.GetPokemonName(x)).ToList());
                case QuestConditionType.PokemonType:
                    return string.Join(", ", condition.Info.PokemonTypeIds?.Select(x => Convert.ToString((PokemonType)x))) + "-type";
                case QuestConditionType.QuestContext:
                    break;
                case QuestConditionType.RaidLevel:
                    return Translator.Instance.Translate(conditionKey, string.Join(", ", condition.Info.RaidLevels));
                case QuestConditionType.SuperEffectiveCharge:
                case QuestConditionType.ThrowType:
                    return Translator.Instance.GetThrowName(condition.Info.ThrowTypeId);
                case QuestConditionType.ThrowTypeInARow:
                    return Translator.Instance.Translate(conditionKey, Translator.Instance.GetThrowName(condition.Info.ThrowTypeId));
                case QuestConditionType.BadgeType:
                case QuestConditionType.CurveBall:
                case QuestConditionType.DailyCaptureBonus:
                case QuestConditionType.DailySpinBonus:
                case QuestConditionType.DaysInARow:
                case QuestConditionType.Item:
                case QuestConditionType.NewFriend:
                case QuestConditionType.PlayerLevel:
                case QuestConditionType.UniquePokestop:
                case QuestConditionType.WeatherBoost:
                case QuestConditionType.WinBattleStatus:
                case QuestConditionType.WinGymBattleStatus:
                case QuestConditionType.WinRaidStatus:
                case QuestConditionType.UniquePokemon:
                case QuestConditionType.NpcCombat:
                case QuestConditionType.PvpCombat:
                case QuestConditionType.Location:
                case QuestConditionType.Distance:
                case QuestConditionType.WithBuddy:
                    return Translator.Instance.Translate(conditionKey);
                case QuestConditionType.PokemonAlignment:
                    return string.Join(", ", condition.Info.AlignmentIds?.Select(x => Translator.Instance.GetAlignmentName((PokemonAlignment)x)));
                case QuestConditionType.InvasionsCharacter:
                    return string.Join(", ", condition.Info.CharacterCategoryIds?.Select(x => Translator.Instance.GetCharacterCategoryName((CharacterCategory)x)));
                case QuestConditionType.MegaEvolution:
                    return string.Join(", ", condition.Info.RaidPokemonEvolutions?.Select(x => Translator.Instance.GetEvolutionName((MegaEvolution)x)));
            }

            return null;
        }

        public static string GetReward(this QuestData quest)
        {
            return GetReward(quest.Rewards?.FirstOrDefault());
        }

        public static string GetReward(this QuestRewardMessage reward)
        {
            return GetReward(reward.Type, reward.Info.PokemonId, reward.Info.Amount, reward.Info.Item, reward.Info.Ditto, reward.Info.Shiny);
        }

        public static string GetReward(this Pokestop pokestop)
        {
            return pokestop.QuestRewards?.FirstOrDefault()?.GetReward();
        }

        public static string GetReward(this QuestRewardType type, int pokemonId, int amount, ItemId item, bool isDitto = false, bool isShiny = false)
        {
            var rewardKey = "quest_reward_" + Convert.ToInt32(type);
            switch (type)
            {
                case QuestRewardType.AvatarClothing:
                case QuestRewardType.Quest:
                    return Translator.Instance.Translate(rewardKey);
                case QuestRewardType.Candy:
                    return Translator.Instance.Translate(rewardKey, amount);
                case QuestRewardType.Experience:
                    return Translator.Instance.Translate(rewardKey, amount);
                case QuestRewardType.Item:
                    var itemName = Translator.Instance.GetItem(item);
                    return Translator.Instance.Translate(rewardKey, amount, itemName);
                case QuestRewardType.PokemonEncounter:
                    return (isShiny ? $"**SHINY** " : "") + Translator.Instance.GetPokemonName(isDitto ? 132 : pokemonId);
                case QuestRewardType.Stardust:
                    return Translator.Instance.Translate(rewardKey, amount);
            }

            return "Unknown";
        }
    }
}