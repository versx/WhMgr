namespace WhMgr.Geofence
{
    public class Location
    {
        public string Address { get; }

        public string City { get; }

        public double Latitude { get; }

        public double Longitude { get; }

        public Location(double lat, double lng)
        {
            Latitude = lat;
            Longitude = lng;
        }

        public Location(string address, string city, double lat, double lng)
            : this(lat, lng)
        {
            Address = address;
            City = city;
        }

        public override string ToString()
        {
            return $"{Latitude}, {Longitude}";
        }
    }
}