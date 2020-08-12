namespace WhMgr.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using Newtonsoft.Json;

    using WhMgr.Diagnostics;

    public class StaticMaps
    {
        private static readonly IEventLogger _logger = EventLogger.GetLogger("STATICMAPS", Program.LogLevel);

        [JsonProperty("pokemon")]
        public StaticMapTemplateConfig Pokemon { get; set; }

        [JsonProperty("raids")]
        public StaticMapTemplateConfig Raids { get; set; }

        [JsonProperty("quests")]
        public StaticMapTemplateConfig Quests { get; set; }

        [JsonProperty("invasions")]
        public StaticMapTemplateConfig Invasions { get; set; }

        [JsonProperty("lures")]
        public StaticMapTemplateConfig Lures { get; set; }

        [JsonProperty("gyms")]
        public StaticMapTemplateConfig Gyms { get; set; }

        [JsonProperty("nests")]
        public StaticMapTemplateConfig Nests { get; set; }

        [JsonProperty("weather")]
        public StaticMapTemplateConfig Weather { get; set; }

        [JsonIgnore]
        public string TemplatesFolder => Path.Combine(Directory.GetCurrentDirectory(), Strings.TemplatesFolder);

        public void LoadConfigs()
        {
            if (!Directory.Exists(TemplatesFolder))
            {
                _logger.Error($"Templates folder for static maps does not exist: {TemplatesFolder}");
                return;
            }
        }
    }

    public class StaticMapTemplateConfig
    {
        [JsonProperty("file")]
        public string TemplateFile { get; set; }

        [JsonProperty("zoom")]
        public int ZoomLevel { get; set; }
    }

    public class StaticMapConfig
    {
        [JsonProperty("staticmap_url")]
        public string StaticMapUrl { get; set; }

        [JsonProperty("markers")]
        public List<StaticMapMarker> Markers { get; set; }

        //[JsonProperty("polygons")]
        //public List<MultiPolygon> Polygons { get; set; }
    }

    public class StaticMapMarker
    {
        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("height")]
        public int Height { get; set; }

        [JsonProperty("width")]
        public int Width { get; set; }

        [JsonProperty("x_offset")]
        public int XOffset { get; set; }

        [JsonProperty("y_offset")]
        public int YOffset { get; set; }

        [JsonProperty("latitude")]
        public double Latitude { get; set; }

        [JsonProperty("longitude")]
        public double Longitude { get; set; }

        public StaticMapMarker(int height, int width, int xOffset, int yOffset, double latitude, double longitude)
        {
            Height = height;
            Width = width;
            XOffset = xOffset;
            YOffset = yOffset;
            Latitude = latitude;
            Longitude = longitude;
        }
    }

    /*
    public class StaticMapPolygon
    {
        [JsonProperty("fill_color")]
        public string FillColor { get; set; }

        [JsonProperty("stroke_color")]
        public string StrokeColor { get; set; }

        [JsonProperty("stroke_width")]
        public int StrokeWidth { get; set; }

        [JsonProperty("path")]
        public MultiPolygon Polygon { get; set; }

        public StaticMapPolygon(string fillColor, string strokeColor, int strokeWidth, MultiPolygon polygon)
        {
            FillColor = fillColor;
            StrokeColor = strokeColor;
            StrokeWidth = strokeWidth;
            Polygon = polygon;
        }
    }
    */
}