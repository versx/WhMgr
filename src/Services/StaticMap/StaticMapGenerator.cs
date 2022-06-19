namespace WhMgr.Services.StaticMap
{
    using System;
    using System.Text;

    using WhMgr.Common;
    using WhMgr.Extensions;
    using WhMgr.Utilities;

    public class StaticMapGenerator : IStaticMapGenerator
    {
        private readonly StaticMapOptions _options;

        public StaticMapGenerator(StaticMapOptions options)
        {
            _options = options;
        }

        public string GenerateLink()
        {
            var payload = new
            {
                url = _options.BaseUrl,
                lat = _options.Latitude,
                lon = _options.Longitude,
                latitude = _options.Latitude,
                longitude = _options.Longitude,
                pokestops = _options.Pokestops,
                gyms = _options.Gyms,
                url2 = _options.SecondaryImageUrl,
                team = _options.Team,
                team_id = Convert.ToInt32(_options.Team ?? PokemonTeam.Neutral),
                regeneratable = _options.Regeneratable,
                pregenerated = _options.Pregenerate,
                path = _options.PolygonPath,
            };
            var url = BuildUrl(_options.Pregenerate);
            if (_options.Pregenerate)
            {
                var payloadJson = payload.ToJson();
                var result = NetUtils.Post(url, payloadJson);
                var responseUrl = $"{_options.BaseUrl}/{_options.TemplateType.ToString().ToLower()}/pregenerated/{result}";
                return responseUrl;
            }
            return url;
        }

        private string BuildUrl(bool pregenerate)
        {
            var sb = new StringBuilder();
            sb.Append(_options.BaseUrl);
            sb.Append('/');
            sb.Append(_options.TemplateType.ToString().ToLower());
            sb.Append('/');
            sb.Append(_options.MapType.ToString().ToLower());
            sb.Append('?');
            sb.Append($"lat={_options.Latitude}");
            sb.Append('&');
            sb.Append($"lon={_options.Longitude}");
            sb.Append('&');
            sb.Append($"url2={_options.SecondaryImageUrl}");

            if (pregenerate)
            {
                sb.Append('&');
                sb.Append($"pregenerate={_options.Pregenerate.ToString().ToLower()}");
                sb.Append('&');
                sb.Append($"regeneratable={_options.Regeneratable.ToString().ToLower()}");
            }
            else
            {
                if (_options.Team != PokemonTeam.All)
                {
                    sb.Append('&');
                    sb.Append($"team={_options.Team}");
                    sb.Append('&');
                    sb.Append($"team_id={Convert.ToInt32(_options.Team)}");
                }
                if (!string.IsNullOrEmpty(_options.PolygonPath))
                {
                    sb.Append('&');
                    sb.Append($"polygon={_options.PolygonPath}");
                }
            }

            var url = sb.ToString();
            return url;
        }
    }
}