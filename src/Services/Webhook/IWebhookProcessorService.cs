namespace WhMgr.Services.Webhook
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IWebhookProcessorService
    {
        bool Enabled { get; }

        void Start();

        void Stop();

        Task ParseDataAsync(List<WebhookPayload> payloads);
    }
}