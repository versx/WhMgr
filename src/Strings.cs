namespace WhMgr
{
    internal static class Strings
    {
        public const string GoogleMaps = "http://maps.google.com/maps?q={0},{1}";
        public const string GoogleMapsStaticImage = "https://maps.googleapis.com/maps/api/staticmap?center={0},{1}&markers=color:red%7C{0},{1}&maptype=roadmap&size=300x175&zoom=14";

        public const string PokemonImage = "https://ver.sx/pogo/monsters/{0:D3}_{1:D3}.png";
        public const string EggImage = "https://ver.sx/pogo/eggs/{0}.png";

        public const string GeofenceFolder = "Geofences";
        public const string DataFolder = "Data";
        public const string AlarmsFileName = "alarms.json";

        public const string DefaultResponseMessage = "WH Test Running!";
        public static readonly string[] LocalEndPoint = { "localhost", "127.0.0.1" };

        public const string DebugLogFileName = "debug.log";
    }
}