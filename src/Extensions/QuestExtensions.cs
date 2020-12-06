namespace WhMgr.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using POGOProtos.Data;
    using POGOProtos.Enums;
    using POGOProtos.Inventory.Item;
    using QuestConditionType = POGOProtos.Data.Quests.QuestCondition.Types.ConditionType;
    using QuestRewardType = POGOProtos.Data.Quests.QuestReward.Types.Type;
    using CharacterCategory = POGOProtos.Enums.EnumWrapper.Types.CharacterCategory;

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
                case QuestConditionType.WithPokemonCategory:
                    return string.Join(", ", condition.Info.PokemonIds?.Select(x => Translator.Instance.GetPokemonName(x)).ToList());
                case QuestConditionType.WithPokemonType:
                    return string.Join(", ", condition.Info.PokemonTypeIds?.Select(x => Convert.ToString((PokemonType)x))) + "-type";
                case QuestConditionType.WithQuestContext:
                    break;
                case QuestConditionType.WithRaidLevel:
                    return Translator.Instance.Translate(conditionKey, string.Join(", ", condition.Info.RaidLevels));
                case QuestConditionType.WithSuperEffectiveCharge:
                case QuestConditionType.WithThrowType:
                    return Translator.Instance.GetThrowName(condition.Info.ThrowTypeId);
                case QuestConditionType.WithThrowTypeInARow:
                    return Translator.Instance.Translate(conditionKey, Translator.Instance.GetThrowName(condition.Info.ThrowTypeId));
                case QuestConditionType.WithBadgeType:
                case QuestConditionType.WithCurveBall:
                case QuestConditionType.WithDailyCaptureBonus:
                case QuestConditionType.WithDailySpinBonus:
                case QuestConditionType.WithDaysInARow:
                case QuestConditionType.WithItem:
                case QuestConditionType.WithNewFriend:
                case QuestConditionType.WithPlayerLevel:
                case QuestConditionType.WithUniquePokestop:
                case QuestConditionType.WithWeatherBoost:
                case QuestConditionType.WithWinBattleStatus:
                case QuestConditionType.WithWinGymBattleStatus:
                case QuestConditionType.WithWinRaidStatus:
                case QuestConditionType.WithUniquePokemon:
                case QuestConditionType.WithNpcCombat:
                case QuestConditionType.WithPvpCombat:
                case QuestConditionType.WithLocation:
                case QuestConditionType.WithDistance:
                case QuestConditionType.WithBuddy:
                    return Translator.Instance.Translate(conditionKey);
                case QuestConditionType.WithPokemonAlignment:
                    return string.Join(", ", condition.Info.AlignmentIds?.Select(x => Translator.Instance.GetAlignmentName((PokemonDisplay.Types.Alignment)x)));
                case QuestConditionType.WithInvasionCharacter:
                    return string.Join(", ", condition.Info.CharacterCategoryIds?.Select(x => Translator.Instance.GetCharacterCategoryName((CharacterCategory)x)));
                case QuestConditionType.WithTempEvoPokemon: // Mega evo
                    return string.Join(", ", condition.Info.RaidPokemonEvolutions?.Select(x => Translator.Instance.GetEvolutionName((TemporaryEvolutionId)x)));
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
                case QuestRewardType.MegaResource:
                    return string.Empty; // TODO: Mega quests
                case QuestRewardType.XlCandy:
                    return string.Empty; // TODO: XL Candy
                case QuestRewardType.Sticker:
                    return string.Empty; // TODO: Sticker
                case QuestRewardType.Pokecoin:
                    return string.Empty; // TODO: Pokecoin
            }

            return "Unknown";
        }
    }
}