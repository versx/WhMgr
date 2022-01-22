namespace WhMgr.Services.Geofence.Geocoding
{
    using System.Threading.Tasks;

    public interface IReverseGeocodingLookup
    {
        Task<string> GetAddressAsync(Coordinate coord);
    }
}