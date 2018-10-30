namespace WhMgr
{
    internal static class Strings
    {
        public const string BotName = "Brock";

        public const string GoogleMaps = "http://maps.google.com/maps?q={0},{1}";
        public const string GoogleMapsStaticImage = "https://maps.googleapis.com/maps/api/staticmap?center={0},{1}&markers=color:red%7C{0},{1}&maptype=roadmap&size=300x175&zoom=14";

        public const string PokemonImage = "https://ver.sx/pogo/monsters/{0:D3}_{1:D3}.png";
        public const string EggImage = "https://ver.sx/pogo/eggs/{0}.png";
        public const string QuestImage = "https://ver.sx/pogo/quests/{0}.png";

        public const string DataFolder = "Data";
        public const string GeofenceFolder = "Geofences";
        public const string LibrariesFolder = "Libs";
        public const string LogsFolder = "Logs";

        public const string DefaultResponseMessage = "WH Test Running!";
        public static readonly string[] LocalEndPoint = { "localhost", "127.0.0.1" };

        public const string AlarmsFileName = "alarms.json";
        public const string ConfigFileName = "config.json";
        public const string DebugLogFileName = "debug.log";

        public const string All = "All";

        public const int MaxPokemonDisplayed = 70;
        public const int MaxPokemonSubscriptions = 25;
        public const int MaxRaidSubscriptions = 5;
        public const int CommonTypeMinimumIV = 97;

        public static string GuildIconUrl { get; set; }
    }
}