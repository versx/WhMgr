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
            var throwTypes = new[]
            {
                HoloActivityType.ActivityCatchFirstThrow,
                HoloActivityType.ActivityCatchNiceThrow,
                HoloActivityType.ActivityCatchGreatThrow,
                HoloActivityType.ActivityCatchExcellentThrow,
            };
            var conditionKey = "quest_condition_" + Convert.ToInt32(condition.Type);
            switch (condition.Type)
            {
                case QuestConditionType.WithPokemonCategory:
                    if (condition.Info.PokemonIds?.Any() ?? false)
                    {
                        var pokemon = condition.Info.PokemonIds.Select(Translator.Instance.GetPokemonName);
                        return Translator.Instance.Translate(conditionKey + "_formatted").FormatText(new
                        {
                            pokemon = string.Join(", ", pokemon),
                        });
                    }
                    return Translator.Instance.Translate(conditionKey);
                case QuestConditionType.WithPokemonType:
                    if (condition.Info.PokemonTypeIds?.Any() ?? false)
                    {
                        var types = condition.Info.PokemonTypeIds.Select(typeId => Convert.ToString((PokemonType)typeId));
                        return Translator.Instance.Translate(conditionKey + "_formatted").FormatText(new
                        {
                            types = string.Join(", ", types),
                        });
                    }
                    return Translator.Instance.Translate(conditionKey);
                case QuestConditionType.WithRaidLevel:
                    if (condition.Info.RaidLevels?.Any() ?? false)
                    {
                        return Translator.Instance.Translate(conditionKey + "_formatted").FormatText(new
                        {
                            levels = string.Join(", ", condition.Info.RaidLevels),
                        });
                    }
                    return Translator.Instance.Translate(conditionKey);
                case QuestConditionType.WithThrowType:
                case QuestConditionType.WithThrowTypeInARow:
                    if (throwTypes.Contains(condition.Info.ThrowTypeId))
                    {
                        return Translator.Instance.Translate(conditionKey + "_formatted").FormatText(new
                        {
                            throw_type = Translator.Instance.GetThrowName(condition.Info.ThrowTypeId),
                        });
                    }
                    return Translator.Instance.Translate(conditionKey);
                case QuestConditionType.WithPokemonAlignment:
                    if (condition.Info.AlignmentIds?.Any() ?? false)
                    {
                        var alignments = condition.Info.AlignmentIds.Select(alignmentId => Translator.Instance.GetAlignmentName((AlignmentId)alignmentId));
                        return Translator.Instance.Translate(conditionKey + "_formatted").FormatText(new
                        {
                            alignments = string.Join(", ", alignments),
                        });
                    }
                    return Translator.Instance.Translate(conditionKey);
                case QuestConditionType.WithInvasionCharacter:
                    if (condition.Info.CharacterCategoryIds?.Any() ?? false)
                    {
                        var categories = condition.Info.CharacterCategoryIds.Select(categoryId => Translator.Instance.GetCharacterCategoryName((CharacterCategory)categoryId));
                        return Translator.Instance.Translate(conditionKey + "_formatted").FormatText(new
                        {
                            categories = string.Join(", ", categories),
                        });
                    }
                    return Translator.Instance.Translate(conditionKey);
                case QuestConditionType.WithTempEvoPokemon: // Mega evo
                    if (condition.Info.RaidPokemonEvolutions?.Any() ?? false)
                    {
                        var evolutions = condition.Info.RaidPokemonEvolutions.Select(evolutionId => Translator.Instance.GetEvolutionName((TemporaryEvolutionId)evolutionId));
                        return Translator.Instance.Translate(conditionKey + "_formatted").FormatText(new
                        {
                            evolutions = string.Join(", ", evolutions),
                        });
                    }
                    return Translator.Instance.Translate(conditionKey);
                case QuestConditionType.WithPokemonLevel:
                    if (condition.Info.MaxLevel > 0)
                    {
                        return Translator.Instance.Translate(conditionKey + "_formatted").FormatText(new
                        {
                            must_be_max_level = condition.Info.MaxLevel,
                        });
                    }
                    return Translator.Instance.Translate(conditionKey);
                case QuestConditionType.WithMaxCp:
                    if (condition.Info.MaxCp > 0)
                    {
                        return Translator.Instance.Translate(conditionKey + "_formatted").FormatText(new
                        {
                            max_cp = condition.Info.MaxCp,
                        });
                    }
                    return Translator.Instance.Translate(conditionKey);
                case QuestConditionType.WithGblRank:
                    if (condition.Info.GblRank > 0)
                    {
                        return Translator.Instance.Translate(conditionKey + "_formatted").FormatText(new
                        {
                            rank = condition.Info.GblRank,
                        });
                    }
                    return Translator.Instance.Translate(conditionKey);
                case QuestConditionType.WithEncounterType:
                    //if (condition.Info.EncounterType?.Any() ?? false)
                    //{
                    //    return Translator.Instance.Translate(conditionKey + "_formatted", new { encounter_type = string.Join(", ", condition.Info.EncounterType) });
                    //}
                    //return Translator.Instance.Translate(conditionKey);
                case QuestConditionType.WithLuckyPokemon:
                case QuestConditionType.WithQuestContext:
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
                case QuestConditionType.WithCombatType:
                case QuestConditionType.WithLocation:
                case QuestConditionType.WithDistance:
                case QuestConditionType.WithBuddy:
                case QuestConditionType.WithSingleDay:
                case QuestConditionType.WithUniquePokemonTeam:
                case QuestConditionType.WithLegendaryPokemon:
                case QuestConditionType.WithGeotargetedPoi:
                case QuestConditionType.WithFriendLevel:
                case QuestConditionType.WithSticker:
                case QuestConditionType.WithBuddyInterestingPoi:
                default:
                    return Translator.Instance.Translate(conditionKey);
            }
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