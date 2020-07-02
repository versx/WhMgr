namespace WhMgr.Utilities
{
    using System;
    using System.Collections.Generic;

    using Newtonsoft.Json;

    using WhMgr.Configuration;
    using WhMgr.Osm;
    using WhMgr.Osm.Models;

    public static class Utils
    {
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
                var polygonKey = "&polygons=";
                var polygonUrl = "[{\"fill_color\":\"rgba(100.0%,0.0%,0.0%,0.5)\",\"stroke_color\":\"black\",\"stroke_width\":1,\"path\":" + latlng + "}]";
                polygonUrl = Uri.EscapeDataString(polygonUrl);
                url += polygonKey + polygonUrl;
            }

            return url;
        }

        public static string PrepareWeatherStaticMapUrl(string staticMapUrl, string marker, double lat, double lng, MultiPolygon polygon = null)
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

            if (polygon != null)
            {
                var latlng = OsmManager.MultiPolygonToLatLng(new List<MultiPolygon> { polygon });
                var polygonKey = "&polygons=";
                var polygonUrl = "[{\"fill_color\":\"rgba(100.0%,0.0%,0.0%,0.5)\",\"stroke_color\":\"black\",\"stroke_width\":1,\"path\":" + latlng + "}]";
                polygonUrl = Uri.EscapeDataString(polygonUrl);
                url += polygonKey + polygonUrl;
            }

            return url;
        }

        public static string GetStaticMapsUrl(string templateFileName, string staticMapUrl, double latitude, double longitude, string markerImageUrl, OsmFeature feature = null, MultiPolygon multiPolygon = null)
        {
            var staticMapData = Renderer.Parse(templateFileName, new
            {
                lat = latitude,
                lon = longitude,
                marker = markerImageUrl,
                pkmn_img_url = markerImageUrl,
                quest_reward_img_url = markerImageUrl,
                weather_img_url = markerImageUrl,
                tilemaps_url = staticMapUrl
            });
            StaticMapConfig staticMap = JsonConvert.DeserializeObject<StaticMapConfig>(staticMapData);

            var url = string.Format(staticMapUrl, latitude, longitude);
            var markerUrl = staticMap.Markers.Count > 0 ? url + "?markers=" + Uri.EscapeDataString(JsonConvert.SerializeObject(staticMap.Markers)) : string.Empty;

            if (feature != null)
            {
                var latlng = OsmManager.MultiPolygonToLatLng(feature.Geometry?.Coordinates);
                var polygonKey = "&polygons=";
                var polygonUrl = @"[{""fill_color"":""rgba(100.0%,0.0%,0.0%,0.5)"",""stroke_color"":""black"",""stroke_width"":1,""path"":" + latlng + "}]";
                markerUrl += polygonKey + Uri.EscapeDataString(polygonUrl);
            }

            if (multiPolygon != null)
            {
                var latlng = OsmManager.MultiPolygonToLatLng(new List<MultiPolygon> { multiPolygon });
                var polygonKey = "&polygons=";
                var polygonUrl = @"[{""fill_color"":""rgba(100.0%,0.0%,0.0%,0.5)"",""stroke_color"":""black"",""stroke_width"":1,""path"":" + latlng + "}]";
                markerUrl += polygonKey + Uri.EscapeDataString(polygonUrl);
            }

            return markerUrl;
        }
    }
}