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

        public List<Pokestop> Pokestops { get; set; } = new();

        public List<Gym> Gyms { get; set; } = new();

        public MapDataCache(
            ILogger<IMapDataCache> logger,
            IDbContextFactory<MapDbContext> dbFactory)
        {
            _logger = logger;
            _dbFactory = dbFactory;
        }

        /// <summary>
        /// Load pokestops and gyms map data
        /// </summary>
        /// <returns></returns>
        public async Task LoadMapData()
        {
            if (Pokestops.Any() && Gyms.Any())
            {
                // Already cached
                return;
            }

            using (var ctx = _dbFactory.CreateDbContext())
            {
                Pokestops = await ctx.Pokestops.ToListAsync();
                Gyms = await ctx.Gyms.ToListAsync();
            }
        }
    }
}