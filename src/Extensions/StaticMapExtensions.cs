namespace WhMgr.Extensions
{
    using System;
    using System.Threading.Tasks;

    using WhMgr.Common;
    using WhMgr.Configuration;
    using WhMgr.Services.Cache;
    using WhMgr.Services.StaticMap;
    using WhMgr.Services.Webhook.Models;

    public static class StaticMapExtensions
    {
        public static string GenerateStaticMap(this StaticMapConfig config, StaticMapType mapType, IWebhookPoint coord, string imageUrl, IMapDataCache cache = null, PokemonTeam? team = null, string polygonPath = null)
        {
            var url = GenerateStaticMapAsync(config, mapType, coord, imageUrl, cache, team, polygonPath).Result;
            return url;
        }

        public static async Task<string> GenerateStaticMapAsync(this StaticMapConfig config, StaticMapType mapType, IWebhookPoint coord, string imageUrl, IMapDataCache cache = null, PokemonTeam? team = null, string polygonPath = null)
        {
            if (!(config?.Enabled ?? false))
            {
                return string.Empty;
            }

            if (string.IsNullOrEmpty(config.Url))
            {
                return string.Empty;
            }

            var staticMap = new StaticMapGenerator(new StaticMapOptions
            {
                BaseUrl = config.Url,
                MapType = mapType,
                TemplateType = config.Type,
                Latitude = coord.Latitude,
                Longitude = coord.Longitude,
                SecondaryImageUrl = imageUrl,
                Gyms = config.IncludeNearbyGyms && cache != null
                    // Fetch nearby gyms from MapDataCache
                    ? await cache?.GetGymsNearby(coord.Latitude, coord.Longitude)
                    : new(),
                Pokestops = config.IncludeNearbyPokestops && cache != null
                    // Fetch nearby pokestops from MapDataCache
                    ? await cache?.GetPokestopsNearby(coord.Latitude, coord.Longitude)
                    : new(),
                Team = team,
                PolygonPath = polygonPath,
                Pregenerate = config.Pregenerate,
                Regeneratable = true,
            });

            var url = staticMap.GenerateLink();
            return url ?? string.Empty;
        }
    }
}