namespace WhMgr.Services.Subscriptions
{
    using WhMgr.Queues;

    public interface ISubscriptionProcessorQueueService
    {
        uint QueueLength { get; }

        void Add(NotificationItem item);
    }
}