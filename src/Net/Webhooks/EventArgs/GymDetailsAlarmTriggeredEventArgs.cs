namespace WhMgr.Net.Webhooks
{
    using System;

    using WhMgr.Alarms.Models;
    using WhMgr.Net.Models;

    public sealed class GymDetailsAlarmTriggeredEventArgs : EventArgs
    {
        public GymDetailsData GymDetails { get; }

        public AlarmObject Alarm { get; }

        public GymDetailsAlarmTriggeredEventArgs(GymDetailsData gymDetails, AlarmObject alarm)
        {
            GymDetails = gymDetails;
            Alarm = alarm;
        }
    }
}