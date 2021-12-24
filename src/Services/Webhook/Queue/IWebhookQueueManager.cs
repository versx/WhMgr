namespace WhMgr.Services.Webhook.Queue
{
    using System.Threading.Tasks;

    public interface IWebhookQueueManager
    {
        Task SendWebhook(string webhookUrl, string json);
    }
}