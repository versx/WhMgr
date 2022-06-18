namespace WhMgr.Services.StaticMap
{
    using System;
    using System.Collections.Generic;

    using WhMgr.Common;

    public class StaticMapOptions
    {
        public string BaseUrl { get; set; }

        public StaticMapType MapType { get; set; } = StaticMapType.Pokemon;

        public StaticMapTemplateType TemplateType { get; set; } = StaticMapTemplateType.StaticMap;

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public string SecondaryImageUrl { get; set; }

        public PokemonTeam? Team { get; set; }

        public string PolygonPath { get; set; }

        public List<dynamic> Gyms { get; set; } = new();

        public List<dynamic> Pokestops { get; set; } = new();

        public bool Pregenerate { get; set; } = true;

        public bool Regeneratable { get; set; } = true;
    }
}