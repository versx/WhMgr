namespace WhMgr.Services.Webhook.Models
{
    using System;

    public interface IWebhookPowerLevel
    {
        uint PowerUpPoints { get; }

        ushort PowerUpLevel { get; }

        ulong PowerUpEndTimestamp { get; }

        public DateTime PowerUpEndTime { get; }
    }
}