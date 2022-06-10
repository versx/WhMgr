namespace WhMgr.Services.Webhook.Models
{
    public interface IWebhookPoint
    {
        double Latitude { get; }

        double Longitude { get; }
    }
}