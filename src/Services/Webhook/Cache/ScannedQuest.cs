namespace WhMgr.Services.Webhook.Cache
{
    using System;

    using POGOProtos.Rpc;

    using WhMgr.Extensions;
    using WhMgr.Services.Webhook.Models;

    internal class ScannedQuest : IScannedItem
    {
        public double Latitude { get; }

        public double Longitude { get; }

        public QuestType Type { get; }

        public DateTime Added { get; }

        public bool IsExpired
        {
            get
            {
                var now = DateTime.UtcNow.ConvertTimeFromCoordinates(Latitude, Longitude);
                return now.Day != Added.Day;
            }
        }

        public ScannedQuest(QuestData quest)
        {
            Latitude = quest.Latitude;
            Longitude = quest.Longitude;
            Type = quest.Type;
            Added = DateTime.UtcNow.ConvertTimeFromCoordinates(Latitude, Longitude);
        }
    }
}