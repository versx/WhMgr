namespace WhMgr.Services.Cache
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using WhMgr.Data.Models;

    public interface IMapDataCache
    {
        List<Pokestop> Pokestops { get; }

        List<Gym> Gyms { get; }

        Task LoadMapData();
    }
}