namespace WhMgr.Comparers
{
    using System;
    using System.Collections.Generic;

    using WhMgr.Net.Models;

    class QuestRewardEqualityComparer : IEqualityComparer<QuestRewardMessage>
    {
        public bool Equals(QuestRewardMessage reward1, QuestRewardMessage reward2)
        {
            if (reward1 == null && reward2 == null)
                return true;
            if (reward1 == null || reward2 == null)
                return false;

            return reward1.Type == reward2.Type;
        }

        public int GetHashCode(QuestRewardMessage reward)
        {
            return reward.Type.GetHashCode();
        }
    }
}