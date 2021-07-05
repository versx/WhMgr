namespace WhMgr.Services.Webhook.Cache
{
    using System;

    using WhMgr.Extensions;
    using WhMgr.Services.Webhook.Models;

    internal class ScannedRaid : IScannedItem
    {
        public double Latitude { get; }

        public double Longitude { get; }

        public ushort Level { get; }

        public uint PokemonId { get; }

        public uint FormId { get; }

        public uint CostumeId { get; }

        public DateTime ExpireTime { get; }

        public bool IsExpired
        {
            get
            {
                var now = DateTime.UtcNow.ConvertTimeFromCoordinates(Latitude, Longitude);
                return now > ExpireTime;
            }
        }

        public ScannedRaid(RaidData raid)
        {
            Latitude = raid.Latitude;
            Longitude = raid.Longitude;
            Level = raid.Level;
            PokemonId = raid.PokemonId;
            FormId = raid.Form;
            CostumeId = raid.Costume;
            ExpireTime = raid.EndTime;
        }
    }
}