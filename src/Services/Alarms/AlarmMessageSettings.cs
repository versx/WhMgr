namespace WhMgr.Services.Alarms
{
    using DSharpPlus;

    using WhMgr.Configuration;
    using WhMgr.Services.Alarms.Models;

    public class AlarmMessageSettings
    {
        public ulong GuildId { get; set; }

        public DiscordClient Client { get; set; }

        public ConfigHolder Config { get; set; }

        public ChannelAlarm Alarm { get; set; }

        public string ImageUrl { get; set; }

        public string City { get; set; }
    }
}