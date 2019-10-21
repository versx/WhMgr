namespace WhMgr.Commands
{
    using DSharpPlus.Interactivity;

    using WhMgr.Configuration;
    using WhMgr.Data.Subscriptions;
    using WhMgr.Localization;
    using WhMgr.Net.Webhooks;

    public class Dependencies
    {
        public InteractivityModule Interactivity;

        public WebhookManager Whm { get; }

        public SubscriptionProcessor SubscriptionProcessor { get; }

        public WhConfig WhConfig { get; }

        public Translator Language { get; }

        public StripeService Stripe { get; }

        public Dependencies(InteractivityModule interactivity, WebhookManager whm, SubscriptionProcessor subProcessor, WhConfig whConfig, Translator language, StripeService stripe)
        {
            Interactivity = interactivity;
            Whm = whm;
            SubscriptionProcessor = subProcessor;
            WhConfig = whConfig;
            Language = language;
            Stripe = stripe;
        }
    }
}