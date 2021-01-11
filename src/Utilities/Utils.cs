namespace WhMgr.Utilities
{
    using Twilio;
    using Twilio.Rest.Api.V2010.Account;
    using Twilio.Types;

    using WhMgr.Configuration;
    using WhMgr.Diagnostics;

    public static class Utils
    {
        private static readonly IEventLogger _logger = EventLogger.GetLogger("UTILS", Program.LogLevel);

        public static bool SendSmsMessage(string body, TwilioConfig config, string toPhoneNumber)
        {
            if (!config.Enabled)
            {
                // Twilio text message notifications not enabled
                return false;
            }

            TwilioClient.Init(config.AccountSid, config.AuthToken);
            var message = MessageResource.Create(
                body: body,
                from: new PhoneNumber($"+1{config.FromNumber}"),
                to: new PhoneNumber($"+1{toPhoneNumber}")
            );
            //_logger.Debug($"Response: {message}");
            return message.ErrorCode == null;
        }
    }
}