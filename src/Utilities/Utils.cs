namespace WhMgr.Utilities
{
    using System;
    using System.Collections.Generic;
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
    }
}