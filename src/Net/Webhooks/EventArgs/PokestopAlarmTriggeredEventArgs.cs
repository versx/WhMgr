namespace WhMgr.Net.Webhooks
{
    using System;

    using WhMgr.Alarms.Models;
    using WhMgr.Net.Models;

    public sealed class PokestopAlarmTriggeredEventArgs : EventArgs
    {
        public PokestopData Pokestop { get; }

        public AlarmObject Alarm { get; }

        public PokestopAlarmTriggeredEventArgs(PokestopData pokestop, AlarmObject alarm)
        {
            Pokestop = pokestop;
            Alarm = alarm;
        }
    }
}