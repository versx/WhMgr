namespace WhMgr.Comparers
{
    using System;
    using System.Collections.Generic;

    using WhMgr.Net.Models;

    class QuestConditionEqualityComparer : IEqualityComparer<QuestConditionMessage>
    {
        public bool Equals(QuestConditionMessage condition1, QuestConditionMessage condition2)
        {
            if (condition1 == null && condition2 == null)
                return true;
            if (condition1 == null || condition2 == null)
                return false;

            return condition1.Type == condition2.Type;
        }

        public int GetHashCode(QuestConditionMessage condition)
        {
            return condition.Type.GetHashCode();
        }
    }
}