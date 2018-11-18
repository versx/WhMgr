namespace WhMgr.Net.Webhooks
{
    using System;

    using WhMgr.Alarms.Models;
    using WhMgr.Net.Models;

    public class GymAlarmTriggeredEventArgs : EventArgs
    {
        public GymData Gym { get; }

        public AlarmObject Alarm { get; }

        public GymAlarmTriggeredEventArgs(GymData gym, AlarmObject alarm)
        {
            Gym = gym;
            Alarm = alarm;
        }
    }
}