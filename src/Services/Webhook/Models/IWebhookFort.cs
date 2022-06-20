namespace WhMgr.Services.Webhook.Models
{
    public interface IWebhookFort
    {
        string FortId { get; }

        string FortName { get; }

        string FortUrl { get; }
    }
}