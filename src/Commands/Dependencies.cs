namespace WhMgr.Commands
{
    using WhMgr.Configuration;
    using WhMgr.Data;
    using WhMgr.Localization;

    public class Dependencies
    {
        public SubscriptionProcessor SubscriptionProcessor { get; }

        public WhConfig WhConfig { get; }

        public Translator Language { get; }

        public Dependencies(SubscriptionProcessor subProcessor, WhConfig whConfig, Translator language)
        {
            SubscriptionProcessor = subProcessor;
            WhConfig = whConfig;
            Language = language;
        }
    }
}