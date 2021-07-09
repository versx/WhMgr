namespace WhMgr.Services.Cache
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using WhMgr.Services.Webhook.Models;

    public interface IMapDataCache
    {
        List<dynamic> GetPokestopsNearby(double latitude, double longitude, double radiusM = 500);

        Task<PokestopData> GetPokestop(string id);

        List<dynamic> GetGymsNearby(double latitude, double longitude, double radiusM = 500);

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