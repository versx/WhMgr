namespace WhMgr.Utilities
{
    using System.Text.Json.Serialization;

    using WhMgr.Common;
    using WhMgr.Services;

    public interface IStaticMapGenerator
    {
        string GenerateLink();
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum StaticMapType
    {
        Pokemon,
        Raids,
        Gyms,
        Quests,
        Invasions,
        Lures,
        Weather,
        Nests,
    }

    public class StaticMapOptions
    {
        public string BaseUrl { get; set; }

        public string TemplateName { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public PokemonTeam? Team { get; set; }

        public string PolygonPath { get; set; }

        public string SecondaryImageUrl { get; set; }
    }

    public class StaticMapGenerator : IStaticMapGenerator
    {
        private readonly StaticMapOptions _options;

        public StaticMapGenerator(StaticMapOptions options)
        {
            _options = options;
        }

        public string GenerateLink()
        {
            var model = new
            {
                lat = _options.Latitude,
                lon = _options.Longitude,
                team_id = _options.Team != PokemonTeam.All ? (uint?)_options.Team : 0,
                polygon = _options.PolygonPath,
                template_name = _options.TemplateName,
                url2 = _options.SecondaryImageUrl,
            };

            // Parse static map template string with values
            var rendered = TemplateRenderer.Parse(_options.BaseUrl, model);
            return rendered;
        }
    }
}