namespace WhMgr
{
    using System.IO;

    using WhMgr.Extensions;

    public static class Strings
    {
        public static Defaults Defaults => Path.Combine(DataFolder, DefaultsFileName)
                                               .LoadFromFile<Defaults>() ?? new();

        public const string BotName = "Webhook Manager";
        public static readonly string BotVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        public const string Creator = "versx";

        public const string ClientBuildFolder = "ClientApp/build";
        public const string AdminDashboardEndpoint = "/dashboard";

        public const string WwwRoot = BasePath + "wwwroot";

        public const string BasePath = "../bin/";
        public const string ViewsFolder = "Views";
        public const string TemplateExt = ".hbs";
        public const string ConfigsFolder = BasePath + "configs";
        public const string GeofencesFolder = BasePath + "geofences";
        public const string AlarmsFolder = BasePath + "alarms";
        public const string EmbedsFolder = BasePath + "embeds";
        public const string DiscordsFolder = BasePath + "discords";
        public const string FiltersFolder = BasePath + "filters";
        public const string LibrariesFolder = "libs";
        public const string StaticFolder = "static";
        public const string TemplatesFolder = BasePath + "templates";
        public const string MigrationsFolder = "migrations";
        public static readonly string AppFolder = StaticFolder + Path.DirectorySeparatorChar + "app";
        public static readonly string DataFolder = BasePath + StaticFolder + Path.DirectorySeparatorChar + "data";
        public static readonly string LocaleFolder = StaticFolder + Path.DirectorySeparatorChar + "locales";
        public static readonly string EmojisFolder = StaticFolder + Path.DirectorySeparatorChar + "emojis";
        public static readonly string OsmNestFilePath = StaticFolder + Path.DirectorySeparatorChar + OsmNestFileName;
        public const string DiscordAuthFilePath = BasePath + "discord_auth.json";
        public const string DiscordAvatarUrlFormat = "https://cdn.discordapp.com/avatars/{0}/{1}.png";

        public const string GoogleMapsReverseGeocodingApiUrl = "https://maps.googleapis.com/maps/api/geocode/json";
        public const string LatestGameMasterFileUrl = "https://raw.githubusercontent.com/WatWowMap/Masterfile-Generator/master/master-latest.json";

        public const string ConfigFileName = "config.json";
        public const string DefaultsFileName = "defaults.json";
        public const string OsmNestFileName = "nest.json";
        public const string DebugLogFileName = "debug.log";
        public const string ErrorLogFileName = "error.log";

        public const int DiscordMaximumMessageLength = 2048;

        public const string All = "All";

        public const string EmojiSchema = "<:{0}:{1}>";
        public const string TypeEmojiSchema = "<:types_{0}:{1}>";
    }
}