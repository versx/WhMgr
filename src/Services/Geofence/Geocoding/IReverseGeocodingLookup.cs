namespace WhMgr.Services.Geofence.Geocoding
{
    public interface IReverseGeocodingLookup
    {
        string GetAddress(Coordinate coord);
    }
}