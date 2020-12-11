namespace WhMgr.Comparers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using WhMgr.Net.Models;

    class QuestConditionEqualityComparer : IEqualityComparer<QuestConditionMessage>
    {
        public bool Equals(QuestConditionMessage condition1, QuestConditionMessage condition2)
        {
            if (condition1 == null && condition2 == null)
                return true;
            if (condition1 == null || condition2 == null)
                return false;

            return condition1.Type == condition2.Type &&
                   condition1.Info.AlignmentIds == condition2.Info.AlignmentIds &&
                   condition1.Info.CategoryName == condition2.Info.CategoryName &&
                   condition1.Info.CharacterCategoryIds == condition2.Info.CharacterCategoryIds &&
                   condition1.Info.Hit == condition2.Info.Hit &&
                   condition1.Info.PokemonIds == condition2.Info.PokemonIds &&
                   condition1.Info.PokemonTypeIds == condition2.Info.PokemonTypeIds &&
                   condition1.Info.RaidLevels == condition2.Info.RaidLevels &&
                   condition1.Info.RaidPokemonEvolutions == condition2.Info.RaidPokemonEvolutions &&
                   condition1.Info.ThrowTypeId == condition2.Info.ThrowTypeId;
        }

        public int GetHashCode(QuestConditionMessage condition)
        {
            var hashCode = condition.Info.AlignmentIds?.Sum() ^
                           condition.Info.CategoryName?.Length ^
                           condition.Info.CharacterCategoryIds?.Sum() ^
                           Convert.ToInt32(condition.Info.Hit) ^
                           condition.Info.PokemonIds?.Sum() ^
                           condition.Info.PokemonTypeIds?.Sum() ^
                           condition.Info.RaidLevels?.Sum() ^
                           condition.Info.RaidPokemonEvolutions?.Sum() ^
                           Convert.ToInt32(condition.Info.ThrowTypeId);
            return hashCode.GetHashCode();
        }
    }
}