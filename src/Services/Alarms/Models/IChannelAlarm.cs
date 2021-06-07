namespace WhMgr.Services.Alarms.Models
{
    using System.Collections.Generic;

    public interface IChannelAlarm
    {
        string Name { get; }

        string Description { get; }

        string FiltersFile { get; }

        string EmbedsFile { get; }

        List<string> Geofences { get; }

        string Webhook { get; }
    }
}