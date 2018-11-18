namespace WhMgr.Net.Webhooks
{
    using System;

    using WhMgr.Alarms.Models;
    using WhMgr.Net.Models;

    public sealed class QuestAlarmTriggeredEventArgs
    {
        public QuestData Quest { get; }

        public AlarmObject Alarm { get; }

        public QuestAlarmTriggeredEventArgs(QuestData quest, AlarmObject alarm)
        {
            Quest = quest;
            Alarm = alarm;
        }
    }
}