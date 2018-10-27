namespace WhMgr.Commands
{
    using WhMgr.Configuration;
    using WhMgr.Data;

    public class Dependencies
    {
        public SubscriptionManager SubscriptionManager { get; }

        public WhConfig WhConfig { get; }

        public Dependencies(SubscriptionManager subMgr, WhConfig whConfig)
        {
            SubscriptionManager = subMgr;
            WhConfig = whConfig;
        }
    }
}