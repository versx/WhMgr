namespace WhMgr.Services.Cache
{
    using System.Threading.Tasks;

    using WhMgr.Services.Webhook.Models;

    public interface IMapDataCache
    {
        Task<PokestopData> GetPokestop(string id);

        Task<GymDetailsData> GetGym(string id);

        Task<WeatherData> GetWeather(long id);

        bool ContainsPokestop(string id);

        bool ContainsGym(string id);

        bool ContainsWeather(long id);

        void UpdatePokestop(PokestopData pokestop);

        void UpdateGym(GymDetailsData gym);

        void UpdateWeather(WeatherData weather);

        //Task LoadMapData();
    }
}