namespace WhMgr.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Twilio;
    using Twilio.Rest.Api.V2010.Account;

    using WhMgr.Configuration;
    using WhMgr.Diagnostics;
    using WhMgr.Geofence;
    using WhMgr.Net.Models;
    using WhMgr.Osm;
    using WhMgr.Osm.Models;

    public static class StaticMap
    {
        private static readonly IEventLogger _logger = EventLogger.GetLogger("STATICMAP", Program.LogLevel);

        public static string GetUrl(string staticMapUrl, string templateName, double latitude, double longitude, string imageUrl, PokemonTeam team = PokemonTeam.All, OsmFeature feature = null, MultiPolygon multiPolygon = null)
        {
            var baseUrl = $"{staticMapUrl}/staticmap/{templateName}?lat={latitude}&lon={longitude}&url2={imageUrl}";
            if (team != PokemonTeam.All)
            {
                baseUrl += $"&team_id={team}";
            }
            if (feature != null)
            {
                var latlng = OsmManager.MultiPolygonToLatLng(feature.Geometry?.Coordinates, true);
                baseUrl += $"&path={latlng}";
            }
            if (multiPolygon != null)
            {
                var latlng = OsmManager.MultiPolygonToLatLng(new List<MultiPolygon> { multiPolygon }, false);
                baseUrl += $"&path={latlng}";
            }
            return baseUrl;
        }
    }

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
                from: new Twilio.Types.PhoneNumber($"+1{config.FromNumber}"),
                to: new Twilio.Types.PhoneNumber($"+1{toPhoneNumber}")
            );
            //Console.WriteLine($"Response: {message}");
            return message.ErrorCode == null;
        }

        public static double GetUnixTimestamp()
        {
            return DateTime.Now.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
        }
    }
}