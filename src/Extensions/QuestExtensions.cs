namespace WhMgr.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;

    using POGOProtos.Rpc;
    using AlignmentId = POGOProtos.Rpc.PokemonDisplayProto.Types.Alignment;
    using CharacterCategory = POGOProtos.Rpc.EnumWrapper.Types.CharacterCategory;
    using TemporaryEvolutionId = POGOProtos.Rpc.HoloTemporaryEvolutionId;
    using QuestConditionType = POGOProtos.Rpc.QuestConditionProto.Types.ConditionType;
    using QuestRewardType = POGOProtos.Rpc.QuestRewardProto.Types.Type;

    using WhMgr.Common;
    using WhMgr.Localization;
    using WhMgr.Services.Webhook.Models;
    using WhMgr.Services.Webhook.Models.Quests;

    public static class QuestExtensions
    {
        #region Quest Message

        public static string GetQuestMessage(this QuestData quest)
        {
            return GetQuestMessage(quest.Type, quest.Target);
        }

        public static string GetQuestMessage(this QuestType type, int target)
        {
            return Translator.Instance.Translate("quest_" + Convert.ToInt32(type)).FormatText(new
            {
                amount = target,
            });
        }

        #endregion

        #region Quest Conditions

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
                    return string.Join(", ", condition.Info.PokemonIds?.Select(pokemonId => Translator.Instance.GetPokemonName(pokemonId)).ToList());
                case QuestConditionType.WithPokemonType:
                    return string.Join(", ", condition.Info.PokemonTypeIds?.Select(typeId => Convert.ToString((PokemonType)typeId))) + "-type";
                case QuestConditionType.WithQuestContext:
                    break;
                case QuestConditionType.WithRaidLevel:
                    return Translator.Instance.Translate(conditionKey, string.Join(", ", condition.Info.RaidLevels));
                case QuestConditionType.WithThrowType:
                    return Translator.Instance.GetThrowName(condition.Info.ThrowTypeId);
                case QuestConditionType.WithThrowTypeInARow:
                    return Translator.Instance.Translate(conditionKey, Translator.Instance.GetThrowName(condition.Info.ThrowTypeId));
                case QuestConditionType.WithPokemonAlignment:
                    return string.Join(", ", condition.Info.AlignmentIds?.Select(alignmentId => Translator.Instance.GetAlignmentName((AlignmentId)alignmentId)));
                case QuestConditionType.WithInvasionCharacter:
                    return string.Join(", ", condition.Info.CharacterCategoryIds?.Select(characterId => Translator.Instance.GetCharacterCategoryName((CharacterCategory)characterId)));
                case QuestConditionType.WithTempEvoPokemon: // Mega evo
                    return string.Join(", ", condition.Info.RaidPokemonEvolutions?.Select(evolutionId => Translator.Instance.GetEvolutionName((TemporaryEvolutionId)evolutionId)));
                case QuestConditionType.WithBadgeType:
                case QuestConditionType.WithCurveBall:
                case QuestConditionType.WithDailyCaptureBonus:
                case QuestConditionType.WithDailySpinBonus:
                case QuestConditionType.WithDaysInARow:
                case QuestConditionType.WithItem:
                case QuestConditionType.WithSuperEffectiveCharge:
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
                default:
                    return Translator.Instance.Translate(conditionKey);
            }

            return null;
        }

        #endregion

        #region Quest Reward

        public static string GetReward(this QuestData quest)
        {
            var encoded = GetReward(quest.Rewards?.FirstOrDefault());
            var decoded = HttpUtility.HtmlDecode(encoded);
            return decoded;
        }

        public static string GetReward(this QuestRewardMessage reward)
        {
            var encoded = GetReward(reward.Type, reward.Info);
            var decoded = HttpUtility.HtmlDecode(encoded);
            return decoded;
        }

        public static string GetReward(this QuestRewardType type, QuestReward info)
        {
            var rewardKey = $"quest_reward_{Convert.ToInt32(type)}_formatted";
            switch (type)
            {
                case QuestRewardType.AvatarClothing:
                case QuestRewardType.Quest:
                    return Translator.Instance.Translate(rewardKey);
                case QuestRewardType.Candy:
                    var name = Translator.Instance.GetPokemonName(info.Ditto ? 132 : info.PokemonId);
                    return Translator.Instance.Translate(rewardKey).FormatText(new
                    {
                        amount = info.Amount,
                        pokemon = name,
                    });
                case QuestRewardType.Experience:
                    return Translator.Instance.Translate(rewardKey).FormatText(new
                    {
                        amount = info.Amount,
                    });
                case QuestRewardType.Item:
                    var itemName = Translator.Instance.GetItem(info.Item);
                    return Translator.Instance.Translate(rewardKey).FormatText(new
                    {
                        amount = info.Amount,
                        item = itemName,
                    });
                case QuestRewardType.PokemonEncounter:
                    var formName = info.FormId > 0 ? Translator.Instance.GetFormName(info.FormId) : null;
                    // TODO: Localize **SHINY**
                    var pkmnName = (info.Shiny ? $"**SHINY** " : "") + Translator.Instance.GetPokemonName(info.Ditto ? 132 : info.PokemonId);
                    return $"{pkmnName} {formName}";
                case QuestRewardType.Stardust:
                    return Translator.Instance.Translate(rewardKey).FormatText(new
                    {
                        amount = info.Amount,
                    });
                case QuestRewardType.MegaResource:
                    return Translator.Instance.Translate(rewardKey).FormatText(new
                    {
                        pokemon = Translator.Instance.GetPokemonName(info.PokemonId),
                        amount = info.Amount,
                    });
                case QuestRewardType.XlCandy:
                    if (info.PokemonId > 0)
                    {
                        return Translator.Instance.Translate(rewardKey).FormatText(new
                        {
                            pokemon = Translator.Instance.GetPokemonName(info.PokemonId),
                            amount = info.Amount,
                        });
                    }
                    return Translator.Instance.Translate(rewardKey).FormatText(new
                    {
                        amount = info.Amount,
                    });
                case QuestRewardType.Sticker:
                    return Translator.Instance.Translate(rewardKey).FormatText(new
                    {
                        sticker_id = info.StickerId,
                        amount = info.Amount,
                    });
                case QuestRewardType.Pokecoin:
                    return Translator.Instance.Translate(rewardKey).FormatText(new
                    {
                        amount = info.Amount,
                    });
                case QuestRewardType.LevelCap:
                    return type.ToString();
            }

            return "Unknown";
        }

        #endregion
    }
}