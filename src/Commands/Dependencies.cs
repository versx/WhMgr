namespace WhMgr.Commands
{
    using DSharpPlus.Interactivity;

    using WhMgr.Configuration;
    using WhMgr.Data.Subscriptions;
    using WhMgr.Net.Webhooks;
    using WhMgr.Osm;

    public class Dependencies
    {
        public InteractivityModule Interactivity;

        public WebhookController Whm { get; }

        public SubscriptionProcessor SubscriptionProcessor { get; }

        public WhConfig WhConfig { get; }

        public StripeService Stripe { get; }

        public OsmManager OsmManager { get; }

        public Dependencies(InteractivityModule interactivity, WebhookController whm, SubscriptionProcessor subProcessor, WhConfig whConfig, StripeService stripe)
        {
            Interactivity = interactivity;
            Whm = whm;
            SubscriptionProcessor = subProcessor;
            WhConfig = whConfig;
            Stripe = stripe;
            OsmManager = new OsmManager();
        }
    }
}