namespace WhMgr.Services.Webhook.Queue
{
    using System.Threading.Tasks;

    public interface IWebhookQueueManager
    {
        void Start();

        void Stop();

        Task SendWebhook(string url, string json);
    }
}