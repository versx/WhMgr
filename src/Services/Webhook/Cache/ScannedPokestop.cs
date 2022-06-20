namespace WhMgr.Services.Webhook.Cache
{
    using System;

    using WhMgr.Common;
    using WhMgr.Extensions;
    using WhMgr.Services.Webhook.Models;

    internal class ScannedPokestop : IScannedItem
    {
        public double Latitude { get; }

        public double Longitude { get; }

        public PokestopLureType LureType { get; }

        public DateTime LureExpireTime { get; }

        public bool IsExpired
        {
            get
            {
                var now = DateTime.UtcNow.ConvertTimeFromCoordinates(Latitude, Longitude);
                return now > LureExpireTime;
            }
        }

        public ScannedPokestop(PokestopData pokestop)
        {
            Latitude = pokestop.Latitude;
            Longitude = pokestop.Longitude;
            LureType = pokestop.LureType;
            LureExpireTime = pokestop.LureExpireTime;
        }
    }
}