namespace WhMgr.Services.Cache
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    using WhMgr.Data.Contexts;
    using WhMgr.Data.Models;

    public class MapDataCache : IMapDataCache
    {
        private readonly ILogger<IMapDataCache> _logger;
        private readonly IDbContextFactory<MapDbContext> _dbFactory;

        private IReadOnlyList<Pokestop> _pokestops;
        private IReadOnlyList<Gym> _gyms;
        private IReadOnlyList<Weather> _weather;

        public MapDataCache(
            ILogger<IMapDataCache> logger,
            IDbContextFactory<MapDbContext> dbFactory)
        {
            _logger = logger;
            _dbFactory = dbFactory;
        }

        /// <summary>
        /// Get Pokestop from map data cache by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Pokestop> GetPokestop(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }
            if (_pokestops == null)
            {
                await LoadMapData();
            }
            var pokestop = _pokestops?.FirstOrDefault(x => x.Id == id);
            return pokestop;
        }

        /// <summary>
        /// Get Gym from map data cache by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Gym> GetGym(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }
            if (_gyms == null)
            {
                await LoadMapData();
            }
            var gym = _gyms?.FirstOrDefault(x => x.Id == id);
            return gym;
        }

        /// <summary>
        /// Get Weather from map data cache by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Weather> GetWeather(long id)
        {
            if (id == 0)
            {
                return null;
            }
            if (_weather == null)
            {
                await LoadMapData();
            }
            var weather = _weather?.FirstOrDefault(x => x.Id == id);
            return weather;
        }

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
                var pokestops = await ctx.Pokestops.ToListAsync();
                _pokestops = pokestops;

                var gyms = await ctx.Gyms.ToListAsync();
                _gyms = gyms;

                var weather = await ctx.Weather.ToListAsync();
                _weather = weather;
            }
        }
    }
}