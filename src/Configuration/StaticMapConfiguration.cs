namespace WhMgr.Configuration
{
    using System;
    using System.Collections.Generic;

    using Newtonsoft.Json;

    public class StaticMapConfiguration
    {
        [JsonProperty("pokemon")]
        public StaticMap Pokemon { get; set; }

        [JsonProperty("raids")]
        public StaticMap Raids { get; set; }

        [JsonProperty("quests")]
        public StaticMap Quests { get; set; }

        [JsonProperty("lures")]
        public StaticMap Lures { get; set; }

        [JsonProperty("invasions")]
        public StaticMap Invasions { get; set; }

        [JsonProperty("gyms")]
        public StaticMap Gyms { get; set; }
    }

    public class StaticMap
    {
        [JsonProperty("enabled")]
        public bool Enabled { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("markers")]
        public List<MarkerConfiguration> Markers { get; set; }

        //public string BuildStaticMapUrl(double lat, double lng)
        //{
        //    var url = string.Format(Url, lat, lng);
        //    var markersKey = "?markers=";
        //    var obj = Markers;
        //    obj.ForEach(x => { x.Latitude = lat; x.Longitude = lng; });
        //    var json = JsonConvert.SerializeObject(Markers);
            
        //    var markerUrl = "?markers=[{\"url\":\"<marker>\",\"height\":32,\"width\":32,\"x_offset\":0,\"y_offset\":0,\"latitude\":<lat>,\"longitude\":<lng>}]";
        //    markerUrl = markerUrl
        //        .Replace("<marker>", marker)
        //        .Replace("<lat>", lat.ToString())
        //        .Replace("<lng>", lng.ToString());

        //    return url + Uri.EscapeUriString(markerUrl);
        //}
    }

    //"url\":\"<marker>\",\"height\":32,\"width\":32,\"x_offset\":0,\"y_offset\":0,\"latitude\":<lat>,\"longitude\":<lng>
    public class MarkerConfiguration
    {
        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("height")]
        public int Height { get; set; }

        [JsonProperty("width")]
        public int Width { get; set; }

        [JsonProperty("x_offset")]
        public int OffsetX { get; set; }

        [JsonProperty("y_offset")]
        public int OffsetY { get; set; }

        [JsonProperty("latitude")]
        public double Latitude { get; set; }

        [JsonProperty("longitude")]
        public double Longitude { get; set; }
    }
}