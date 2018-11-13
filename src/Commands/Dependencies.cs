namespace WhMgr.Commands
{
    using System.Collections.Generic;

    using WhMgr.Configuration;
    using WhMgr.Data;
    using WhMgr.Localization;

    public class Dependencies
    {
        public SubscriptionManager SubscriptionManager { get; }

        public WhConfig WhConfig { get; }

        public Translator Language { get; }

        public Dependencies(SubscriptionManager subMgr, WhConfig whConfig, Translator language)
        {
            SubscriptionManager = subMgr;
            WhConfig = whConfig;
            Language = language;
        }
    }
}