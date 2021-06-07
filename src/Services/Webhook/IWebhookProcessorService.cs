namespace WhMgr.Services.Webhook
{
    using System.Collections.Generic;

    public interface IWebhookProcessorService
    {
        void Start();

        void Stop();

        void ParseData(List<WebhookPayload> payloads);
    }
}