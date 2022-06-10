namespace WhMgr.Services.Webhook.Models
{
    using System.Text.Json.Serialization;
    using System.Threading.Tasks;

    using WhMgr.Services.Alarms;
    using WhMgr.Services.Discord.Models;

    public interface IWebhookData
    {
        Task<DiscordWebhookMessage> GenerateEmbedMessageAsync(AlarmMessageSettings settings);

        void SetTimes();
    }
}