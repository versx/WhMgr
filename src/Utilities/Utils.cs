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

        public static Location GetAddress(string city, double lat, double lng, WhConfig config)
        {
            if (!string.IsNullOrEmpty(config.GoogleMapsKey))
                return GetGoogleAddress(city, lat, lng, config.GoogleMapsKey);

            if (!string.IsNullOrEmpty(config.NominatimEndpoint))
                return GetNominatimAddress(city, lat, lng, config.NominatimEndpoint);

            return null;
        }

        public static Location GetGoogleAddress(string city, double lat, double lng, string gmapsKey)
        {
            var apiKey = string.IsNullOrEmpty(gmapsKey) ? string.Empty : $"&key={gmapsKey}";
            var url = $"https://maps.googleapis.com/maps/api/geocode/json?latlng={lat},{lng}&sensor=true{apiKey}";
            var unknown = "Unknown";
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(url);
                var response = request.GetResponse();
                using (var responseStream = response.GetResponseStream())
                {
                    var reader = new StreamReader(responseStream, Encoding.UTF8);
                    var data = reader.ReadToEnd();
                    var parseJson = JObject.Parse(data);
                    var status = Convert.ToString(parseJson["status"]);
                    if (string.Compare(status, "OK", true) != 0)
                        return null;

                    var result = parseJson["results"].FirstOrDefault();
                    var address = Convert.ToString(result["formatted_address"]);
                    //var area = Convert.ToString(result["address_components"][2]["long_name"]);
                    return new Location(address, city ?? unknown, lat, lng);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
            return null;
        }

        public static Location GetNominatimAddress(string city, double lat, double lng, string endpoint)
        {
            var unknown = "Unknown";
            var url = $"{endpoint}/reverse?format=jsonv2&lat={lat}&lon={lng}";
            try
            {
                using (var wc = new WebClient())
                {
                    wc.Proxy = null;
                    wc.Headers.Add("User-Agent", Strings.BotName);
                    var json = wc.DownloadString(url);
                    dynamic obj = JsonConvert.DeserializeObject(json);
                    return new Location(Convert.ToString(obj.display_name), city ?? unknown, Convert.ToDouble(obj.lat), Convert.ToDouble(obj.lon));
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
            return null;
        }

        public static double GetUnixTimestamp()
        {
            return DateTime.Now.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
        }
    }
}