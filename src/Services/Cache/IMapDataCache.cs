namespace WhMgr.Services.Cache
{
    using System.Threading.Tasks;

    using WhMgr.Data.Models;

    public interface IMapDataCache
    {
        Task<Pokestop> GetPokestop(string id);

        Task<Gym> GetGym(string id);

        Task<Weather> GetWeather(long id);

        //Task LoadMapData();
    }
}