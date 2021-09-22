namespace WhMgr.Services.Alarms.Models
{
    using WhMgr.Services.Webhook.Models;

    public class AlarmTaskItem
    {
        public ulong GuildId { get; set; }

        public ChannelAlarm Alarm { get; set; }

        public IWebhookData Data { get; set; }

        public string City { get; set; }
    }
}