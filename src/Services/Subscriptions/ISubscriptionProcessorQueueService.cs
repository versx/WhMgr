namespace WhMgr.Services.Subscriptions
{
    using WhMgr.Queues;

    public interface ISubscriptionProcessorQueueService
    {
        void Add(NotificationItem item);
    }
}