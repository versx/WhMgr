namespace WhMgr.Services.Webhook.Queue
{
    public class WebhookQueueItem
    {
        public string Url { get; set; }

        public string Json { get; set; }

        public int RetryAfter { get; set; }
    }
}