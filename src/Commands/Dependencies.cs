namespace WhMgr.Commands
{
    using DSharpPlus.Interactivity;

    using WhMgr.Configuration;
    using WhMgr.Data.Subscriptions;
    using WhMgr.Localization;
    using WhMgr.Net.Webhooks;
    using WhMgr.Osm;

    public class Dependencies
    {
        public InteractivityModule Interactivity;

        public WebhookController Whm { get; }

        public SubscriptionProcessor SubscriptionProcessor { get; }

        public WhConfig WhConfig { get; }

        public Translator Language { get; }

        public StripeService Stripe { get; }

        public OsmManager OsmManager { get; }

        public Dependencies(InteractivityModule interactivity, WebhookController whm, SubscriptionProcessor subProcessor, WhConfig whConfig, Translator language, StripeService stripe)
        {
            Interactivity = interactivity;
            Whm = whm;
            SubscriptionProcessor = subProcessor;
            WhConfig = whConfig;
            Language = language;
            Stripe = stripe;
            OsmManager = new OsmManager();
        }
    }
}