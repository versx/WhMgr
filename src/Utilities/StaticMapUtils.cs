namespace WhMgr.Utilities
{
    using System;
    using System.Collections.Generic;

    using WhMgr.Osm;
    using WhMgr.Osm.Models;
    using WhMgr.Services.Webhook.Models;

    public static class StaticMap
    {
        // TODO: Add support for multistaticmap templates
        public static string GetUrl(string staticMapUrl, string templateName, double latitude, double longitude, string imageUrl, PokemonTeam team = PokemonTeam.All, OsmFeature feature = null, MultiPolygon multiPolygon = null)
        {
            var baseUrl = $"{staticMapUrl}/staticmap/{templateName}?lat={latitude}&lon={longitude}&url2={imageUrl}";
            if (team != PokemonTeam.All)
            {
                baseUrl += $"&team_id={Convert.ToInt32(team)}";
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
}
