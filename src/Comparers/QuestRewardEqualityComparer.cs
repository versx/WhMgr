namespace WhMgr.Comparers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using WhMgr.Net.Models;

    class QuestRewardEqualityComparer : IEqualityComparer<QuestRewardMessage>
    {
        public bool Equals(QuestRewardMessage reward1, QuestRewardMessage reward2)
        {
            if (reward1 == null && reward2 == null)
                return true;
            if (reward1 == null || reward2 == null)
                return false;

            return reward1.Type == reward2.Type &&
                   reward1.Info.Amount == reward2.Info.Amount &&
                   reward1.Info.CostumeId == reward2.Info.CostumeId &&
                   reward1.Info.Ditto == reward2.Info.Ditto &&
                   reward1.Info.FormId == reward2.Info.FormId &&
                   reward1.Info.GenderId == reward2.Info.GenderId &&
                   reward1.Info.Item == reward2.Info.Item &&
                   reward1.Info.PokemonId == reward2.Info.PokemonId &&
                   reward1.Info.RaidLevels == reward2.Info.RaidLevels &&
                   reward1.Info.Shiny == reward2.Info.Shiny;
        }

        public int GetHashCode(QuestRewardMessage reward)
        {
            var hashCode = Convert.ToInt32(reward.Type) ^
                           reward.Info.Amount ^
                           reward.Info.CostumeId ^
                           Convert.ToInt32(reward.Info.Ditto) ^
                           reward.Info.FormId ^
                           reward.Info.GenderId ^
                           Convert.ToInt32(reward.Info.Item) ^
                           reward.Info.PokemonId ^
                           reward.Info.RaidLevels?.Sum() ^
                           Convert.ToInt32(reward.Info.Shiny);
            return hashCode.GetHashCode();
        }
    }
}