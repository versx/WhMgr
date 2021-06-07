namespace WhMgr.Services.Webhook
{
    public interface IWebhookPayload
    {
        string Type { get; }

        dynamic Message { get; }
    }
}