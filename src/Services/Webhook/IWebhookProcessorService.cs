namespace WhMgr.Services.Webhook
{
    using System.Collections.Generic;

    public interface IWebhookProcessorService
    {
        bool Enabled { get; }

        void Start();

        void Stop();

        void ParseData(List<WebhookPayload> payloads);
    }
}