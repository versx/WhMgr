namespace WhMgr.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Text;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    using WhMgr.Diagnostics;
    using WhMgr.Geofence;
    using WhMgr.Osm;
    using WhMgr.Osm.Models;

    public static class Utils
    {
        private static readonly IEventLogger _logger = EventLogger.GetLogger("UTILS");

        public static Location GetGoogleAddress(double lat, double lng, string gmapsKey)
        {
            var url = $"https://maps.googleapis.com/maps/api/geocode/json?latlng={lat},{lng}&sensor=true&key={gmapsKey}";
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

                    if (Convert.ToString(parseJson["status"]) != "OK") return null;

                    var jsonres = parseJson["results"][0];
                    var address = Convert.ToString(jsonres["formatted_address"]);
                    var addrComponents = jsonres["address_components"];
                    var city = unknown;

                    var items = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(addrComponents.ToString());

                    foreach (var item in items)
                    {
                        foreach (var key in item)
                        {
                            if (key.Key == "types")
                            {
                                if (key.Value is JArray types)
                                {
                                    foreach (var type in types)
                                    {
                                        var t = type.ToString();
                                        if (string.Compare(t, "locality", true) == 0)
                                        {
                                            city = Convert.ToString(item["short_name"]);
                                            break;
                                        }
                                    }
                                }
                            }

                            if (city != unknown) break;
                        }

                        if (city != unknown) break;
                    }

                    return new Location(address, city, lat, lng);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }

            return null;
        }

        public static string PrepareStaticMapUrl(string staticMapUrl, string marker, double lat, double lng, OsmFeature feature = null)
        {
            var url = string.Format(staticMapUrl, lat, lng);
            var markerKey = "?markers=";
            var markerUrl = "[{\"url\":\"<marker>\",\"height\":32,\"width\":32,\"x_offset\":0,\"y_offset\":0,\"latitude\":<lat>,\"longitude\":<lng>}]";
            markerUrl = markerUrl
                .Replace("<marker>", marker)
                .Replace("<lat>", lat.ToString())
                .Replace("<lng>", lng.ToString());
            markerUrl = Uri.EscapeDataString(markerUrl);
            url += markerKey + markerUrl;

            if (feature != null)
            {
                var latlng = OsmManager.MultiPolygonToLatLng(feature.Geometry?.Coordinates);
                var pathKey = "\"path\":";
                var pathUrl = pathKey + latlng;
                var polygonKey = "&polygons=";
                var polygonUrl = "[{\"fill_color\":\"rgba(100.0%,0.0%,0.0%,0.5)\",\"stroke_color\":\"black\",\"stroke_width\":1," + pathUrl + "}]";
                polygonUrl = Uri.EscapeDataString(polygonUrl);
                url += polygonKey + polygonUrl;
            }

            return url;
        }
    }
}