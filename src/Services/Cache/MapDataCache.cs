namespace WhMgr.Services.Cache
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    using WhMgr.Data.Contexts;
    using WhMgr.Extensions;
    using WhMgr.Services.Geofence;
    using WhMgr.Services.Webhook.Models;
    using WhMgr.Utilities;

    public class MapDataCache : IMapDataCache
    {
        private readonly ILogger<IMapDataCache> _logger;
        private readonly IDbContextFactory<MapDbContext> _dbFactory;

        private Dictionary<string, PokestopData> _pokestops;
        private Dictionary<string, GymDetailsData> _gyms;
        private Dictionary<long, WeatherData> _weather;

        public MapDataCache(
            ILogger<IMapDataCache> logger,
            IDbContextFactory<MapDbContext> dbFactory)
        {
            _logger = logger;
            _dbFactory = dbFactory;
        }

        #region Pokestops

        /// <summary>
        /// Get Pokestop from map data cache by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<PokestopData> GetPokestop(string id)
        {
            if (string.IsNullOrEmpty(id) || string.Compare(id, "None", true) == 0)
            {
                return null;
            }
            if (_pokestops == null)
            {
                await LoadMapData();
            }
            var pokestop = _pokestops?[id];
            return pokestop;
        }

        public bool ContainsPokestop(string id) =>
            _pokestops?.ContainsKey(id) ?? false;

        public void UpdatePokestop(PokestopData pokestop)
        {
            if (ContainsPokestop(pokestop.PokestopId))
            {
                _pokestops[pokestop.PokestopId] = pokestop;
            }
            else
            {
                _pokestops.Add(pokestop.PokestopId, pokestop);
            }
        }

        #endregion

        #region Gyms

        /// <summary>
        /// Get Gym from map data cache by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<GymDetailsData> GetGym(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }
            if (_gyms == null)
            {
                await LoadMapData();
            }
            var gym = _gyms?[id];
            return gym;
        }

        public bool ContainsGym(string id) =>
            _gyms?.ContainsKey(id) ?? false;

        public void UpdateGym(GymDetailsData gym)
        {
            if (ContainsGym(gym.GymId))
            {
                _gyms[gym.GymId] = gym;
            }
            else
            {
                _gyms.Add(gym.GymId, gym);
            }
        }

        #endregion

        #region Weather

        /// <summary>
        /// Get Weather from map data cache by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<WeatherData> GetWeather(long id)
        {
            if (id == 0)
            {
                return null;
            }
            if (_weather == null)
            {
                await LoadMapData();
            }
            var weather = _weather?[id];
            return weather;
        }

        public bool ContainsWeather(long id) =>
            _weather?.ContainsKey(id) ?? false;

        public void UpdateWeather(WeatherData weather)
        {
            if (ContainsWeather(weather.Id))
            {
                _weather[weather.Id] = weather;
            }
            else
            {
                _weather.Add(weather.Id, weather);
            }
        }

        #endregion

        /// <summary>
        /// Load pokestops gyms, and weather map data
        /// </summary>
        /// <returns></returns>
        private async Task LoadMapData()
        {
            if (_pokestops != null && _gyms != null && _weather != null)
            {
                // Already cached
                _logger.LogDebug($"Map data already cached");
                return;
            }

            using (var ctx = _dbFactory.CreateDbContext())
            {
                _pokestops = await ctx.Pokestops.ToDictionaryAsync(key => key.PokestopId, value => value);
                _gyms = await ctx.Gyms.ToDictionaryAsync(key => key.GymId, value => value);
                _weather = await ctx.Weather.ToDictionaryAsync(key => key.Id, value => value);
            }
        }

        public List<dynamic> GetPokestopsNearby(double latitude, double longitude, double radiusM = 100)
        {
            if (_pokestops == null)
            {
                LoadMapData(); // TODO: Await
            }

            var nearby = _pokestops.Values.Where(stop =>
            {
                var stopCoord = new Coordinate(stop.Latitude, stop.Longitude);
                var mapCoord = new Coordinate(latitude, longitude);
                var distance = stopCoord.DistanceTo(mapCoord);
                var isWithinRadius = distance <= radiusM;
                return isWithinRadius;
            }).Select(x => new
            {
                id = x.PokestopId,
                lat = x.Latitude,
                lon = x.Longitude,
                lure_id = Convert.ToInt32(x.LureType),
                lure = x.LureType,
                marker = x.HasInvasion
                    ? IconFetcher.Instance.GetInvasionIcon("Default", x.GruntType)
                    : IconFetcher.Instance.GetLureIcon("Default", x.LureType), // TODO: Get icon style
            }).ToList<dynamic>();
            return nearby;
        }

        public List<dynamic> GetGymsNearby(double latitude, double longitude, double radiusM = 100)
        {
            if (_gyms == null)
            {
                LoadMapData(); // TODO: Await
            }

            var nearby = _gyms.Values.Where(gym =>
            {
                var gymCoord = new Coordinate(gym.Latitude, gym.Longitude);
                var mapCoord = new Coordinate(latitude, longitude);
                var distance = gymCoord.DistanceTo(mapCoord);
                var isWithinRadius = distance <= radiusM;
                return isWithinRadius;
            }).Select(x => new
            {
                id = x.GymId,
                lat = x.Latitude,
                lon = x.Longitude,
                team_id = Convert.ToInt32(x.Team),
                team = x.Team,
                marker = IconFetcher.Instance.GetGymIcon("Default", x.Team), // TODO: Get icon style
            }).ToList<dynamic>();
            return nearby;
        }
    }
}