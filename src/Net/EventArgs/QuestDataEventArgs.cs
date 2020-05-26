namespace WhMgr.Net
{
    using System;

    using WhMgr.Net.Models;

    public sealed class QuestDataEventArgs : EventArgs
    {
        public QuestData Quest { get; }

        public QuestDataEventArgs(QuestData quest)
        {
            Quest = quest;
        }
    }
}