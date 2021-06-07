namespace WhMgr.Services.Webhook.Models
{
    using WhMgr.Services.Alarms;
    using WhMgr.Services.Discord.Models;

    public interface IWebhookData
    {
        DiscordWebhookMessage GenerateEmbedMessage(AlarmMessageSettings settings);
    }
}