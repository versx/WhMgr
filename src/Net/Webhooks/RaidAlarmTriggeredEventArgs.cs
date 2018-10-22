namespace WhMgr.Net.Webhooks
{
    using System;

    using WhMgr.Alarms.Models;
    using WhMgr.Net.Models;

    public class RaidAlarmTriggeredEventArgs : EventArgs
    {
        public AlarmObject Alarm { get; }

        public RaidData Raid { get; }

        public RaidAlarmTriggeredEventArgs(RaidData raid, AlarmObject alarm)
        {
            Raid = raid;
            Alarm = alarm;
        }
    }
}