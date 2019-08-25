namespace WhMgr.Net.Webhooks
{
    using System;

    using WhMgr.Alarms.Models;

    public sealed class AlarmEventTriggeredEventArgs<T> : EventArgs
    {
        public AlarmObject Alarm { get; }

        public T Data { get; }

        public AlarmEventTriggeredEventArgs(T data, AlarmObject alarm)
        {
            Data = data;
            Alarm = alarm;
        }
    }
}