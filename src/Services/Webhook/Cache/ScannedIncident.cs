namespace WhMgr.Services.Webhook.Cache
{
    using System;

    using InvasionCharacter = POGOProtos.Rpc.EnumWrapper.Types.InvasionCharacter;

    using WhMgr.Extensions;
    using WhMgr.Services.Webhook.Models;

    internal class ScannedIncident : IScannedItem
    {
        public double Latitude { get; }

        public double Longitude { get; }

        public InvasionCharacter Character { get; }

        public DateTime ExpireTime { get; }

        public bool IsExpired
        {
            get
            {
                var now = DateTime.UtcNow.ConvertTimeFromCoordinates(Latitude, Longitude);
                return now >= ExpireTime;
            }
        }

        public ScannedIncident(IncidentData incident)
        {
            Latitude = incident.Latitude;
            Longitude = incident.Longitude;
            Character = incident.Character;
            ExpireTime = incident.ExpirationTime;
        }
    }
}
