namespace WhMgr.Net.Models
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using DSharpPlus;
    using DSharpPlus.Entities;
    using Newtonsoft.Json;

    using WhMgr.Alarms.Alerts;
    using WhMgr.Alarms.Models;
    using WhMgr.Configuration;
    using WhMgr.Data;
    using WhMgr.Extensions;
    using WhMgr.Localization;
    using WhMgr.Utilities;

    /// <summary>
    /// RealDeviceMap Pokestop (lure/invasion) webhook model class.
    /// </summary>
    public sealed class PokestopData
    {
        public const string WebhookHeader = "pokestop";
        public const string WebhookHeaderInvasion = "invasion";

        #region Properties

        [JsonProperty("pokestop_id")]
        public string PokestopId { get; set; }

        [JsonProperty("latitude")]
        public double Latitude { get; set; }

        [JsonProperty("longitude")]
        public double Longitude { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; } = "Unknown";

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("enabled")]
        public bool Enabled { get; set; }

        [JsonProperty("lure_expiration")]
        public long LureExpire { get; set; }

        [JsonIgnore]
        public DateTime LureExpireTime { get; set; }

        [JsonProperty("lure_id")]
        public PokestopLureType LureType { get; set; }

        [JsonProperty("incident_expire_timestamp")]
        public long IncidentExpire { get; set; }

        [JsonIgnore]
        public DateTime InvasionExpireTime { get; set; }

        [JsonProperty("pokestop_display")]
        public PokestopDisplay PokestopDisplay { get; set; }

        [JsonProperty("grunt_type")]
        public InvasionGruntType GruntType { get; set; }

        [JsonProperty("last_modified")]
        public ulong LastModified { get; set; }

        [JsonProperty("updated")]
        public ulong Updated { get; set; }

        [JsonIgnore]
        public bool HasLure => LureExpire > 0 && LureType != PokestopLureType.None && LureExpireTime > DateTime.UtcNow.ConvertTimeFromCoordinates(Latitude, Longitude);

        [JsonIgnore]
        public bool HasInvasion => IncidentExpire > 0 && InvasionExpireTime > DateTime.UtcNow.ConvertTimeFromCoordinates(Latitude, Longitude);

        #endregion

        #region Constructor

        /// <summary>
        /// Instantiate a new <see cref="PokestopData"/> class.
        /// </summary>
        public PokestopData()
        {
            SetTimes();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Set expire times because .NET doesn't support Unix timestamp deserialization to <seealso cref="DateTime"/> class by default.
        /// </summary>
        public void SetTimes()
        {
            LureExpireTime = LureExpire
                .FromUnix()
                .ConvertTimeFromCoordinates(Latitude, Longitude);

            InvasionExpireTime = IncidentExpire
                .FromUnix()
                .ConvertTimeFromCoordinates(Latitude, Longitude);
        }

        public DiscordEmbedNotification GeneratePokestopMessage(ulong guildId, DiscordClient client, WhConfig whConfig, AlarmObject alarm, string city)
        {
            var alertType = HasInvasion ? AlertMessageType.Invasions : HasLure ? AlertMessageType.Lures : AlertMessageType.Pokestops;
            var alert = alarm?.Alerts[alertType] ?? AlertMessage.Defaults[alertType];
            var properties = GetProperties(client.Guilds[guildId], whConfig, city);
            var eb = new DiscordEmbedBuilder
            {
                Title = Renderer.Parse(alert.Title, properties),
                Url = Renderer.Parse(alert.Url, properties),
                ImageUrl = Renderer.Parse(alert.ImageUrl, properties),
                ThumbnailUrl = Renderer.Parse(alert.IconUrl, properties),
                Description = Renderer.Parse(alert.Content, properties),
                Color = HasInvasion ? DiscordColor.Red : HasLure ?
                    (LureType == PokestopLureType.Normal ? DiscordColor.HotPink
                    : LureType == PokestopLureType.Glacial ? DiscordColor.CornflowerBlue
                    : LureType == PokestopLureType.Mossy ? DiscordColor.SapGreen
                    : LureType == PokestopLureType.Magnetic ? DiscordColor.Gray
                    : DiscordColor.CornflowerBlue) : DiscordColor.CornflowerBlue,
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = Renderer.Parse(alert.Footer?.Text ?? client.Guilds[guildId]?.Name ?? DateTime.Now.ToString(), properties),
                    IconUrl = Renderer.Parse(alert.Footer?.IconUrl ?? client.Guilds[guildId]?.IconUrl ?? string.Empty, properties)
                }
            };
            var username = Renderer.Parse(alert.Username, properties);
            var iconUrl = Renderer.Parse(alert.AvatarUrl, properties);
            var description = Renderer.Parse(alarm?.Description, properties);
            return new DiscordEmbedNotification(username, iconUrl, description, new List<DiscordEmbed> { eb.Build() });
        }

        #endregion

        #region Private Methods

        private IReadOnlyDictionary<string, string> GetProperties(DiscordGuild guild, WhConfig whConfig, string city)
        {
            var lureImageUrl = this.GetLureIcon(whConfig, whConfig.Servers[guild.Id].IconStyle);
            var invasionImageUrl = this.GetInvasionIcon(whConfig, whConfig.Servers[guild.Id].IconStyle);
            var imageUrl = HasInvasion ? invasionImageUrl : HasLure ? lureImageUrl : Url;
            var gmapsLink = string.Format(Strings.GoogleMaps, Latitude, Longitude);
            var appleMapsLink = string.Format(Strings.AppleMaps, Latitude, Longitude);
            var wazeMapsLink = string.Format(Strings.WazeMaps, Latitude, Longitude);
            var scannerMapsLink = string.Format(whConfig.Urls.ScannerMap, Latitude, Longitude);
            var templatePath = Path.Combine(whConfig.StaticMaps.TemplatesFolder, HasInvasion ? whConfig.StaticMaps.Invasions.TemplateFile : HasLure ? whConfig.StaticMaps.Lures.TemplateFile : whConfig.StaticMaps.Lures.TemplateFile);
            var staticMapLink = Utils.GetStaticMapsUrl(templatePath, whConfig.Urls.StaticMap, HasInvasion ? whConfig.StaticMaps.Invasions.ZoomLevel : HasLure ? whConfig.StaticMaps.Lures.ZoomLevel : whConfig.StaticMaps.Lures.ZoomLevel, Latitude, Longitude, imageUrl, null);
            var gmapsLocationLink = string.IsNullOrEmpty(whConfig.ShortUrlApiUrl) ? gmapsLink : NetUtil.CreateShortUrl(whConfig.ShortUrlApiUrl, gmapsLink);
            var appleMapsLocationLink = string.IsNullOrEmpty(whConfig.ShortUrlApiUrl) ? appleMapsLink : NetUtil.CreateShortUrl(whConfig.ShortUrlApiUrl, appleMapsLink);
            var wazeMapsLocationLink = string.IsNullOrEmpty(whConfig.ShortUrlApiUrl) ? wazeMapsLink : NetUtil.CreateShortUrl(whConfig.ShortUrlApiUrl, wazeMapsLink);
            var scannerMapsLocationLink = string.IsNullOrEmpty(whConfig.ShortUrlApiUrl) ? scannerMapsLink : NetUtil.CreateShortUrl(whConfig.ShortUrlApiUrl, scannerMapsLink);
            Geofence.Location address = null;
            if (!string.IsNullOrEmpty(whConfig.GoogleMapsKey))
            {
                address = Utils.GetGoogleAddress(city, Latitude, Longitude, whConfig.GoogleMapsKey);
            }
            else if (!string.IsNullOrEmpty(whConfig.NominatimEndpoint))
            {
                address = Utils.GetNominatimAddress(city, Latitude, Longitude, whConfig.NominatimEndpoint);
            }
            //var staticMapLocationLink = string.IsNullOrEmpty(whConfig.ShortUrlApiUrl) ? staticMapLink : NetUtil.CreateShortUrl(whConfig.ShortUrlApiUrl, staticMapLink);
            var invasion = MasterFile.Instance.GruntTypes.ContainsKey(GruntType) ? MasterFile.Instance.GruntTypes[GruntType] : null;
            var leaderString = Translator.Instance.Translate("grunt_" + Convert.ToInt32(GruntType));
            var pokemonType = MasterFile.Instance.GruntTypes.ContainsKey(GruntType) ? Commands.Notifications.GetPokemonTypeFromString(invasion?.Type) : PokemonType.None;
            var invasionTypeEmoji = pokemonType == PokemonType.None
                ? leaderString
                : pokemonType.GetTypeEmojiIcons();
            var invasionEncounters = GruntType > 0 ? invasion.GetPossibleInvasionEncounters() : string.Empty;

            const string defaultMissingValue = "?";
            var dict = new Dictionary<string, string>
            {
                //Main properties
                { "has_lure", Convert.ToString(HasLure) },
                { "lure_type", LureType.ToString() },
                { "lure_expire_time", LureExpireTime.ToLongTimeString() },
                { "lure_expire_time_24h", LureExpireTime.ToString("HH:mm:ss") },
                { "lure_expire_time_left", LureExpireTime.GetTimeRemaining().ToReadableStringNoSeconds() },
                { "has_invasion", Convert.ToString(HasInvasion) },
                { "grunt_type", invasion?.Type },
                { "grunt_type_emoji", invasionTypeEmoji },
                { "grunt_gender", invasion?.Grunt },
                { "invasion_expire_time", InvasionExpireTime.ToLongTimeString() },
                { "invasion_expire_time_24h", InvasionExpireTime.ToString("HH:mm:ss") },
                { "invasion_expire_time_left", InvasionExpireTime.GetTimeRemaining().ToReadableStringNoSeconds() },
                { "invasion_encounters", $"**Encounter Reward Chance:**\r\n" + invasionEncounters },

                //Location properties
                { "geofence", city ?? defaultMissingValue },
                { "lat", Latitude.ToString() },
                { "lng", Longitude.ToString() },
                { "lat_5", Math.Round(Latitude, 5).ToString() },
                { "lng_5", Math.Round(Longitude, 5).ToString() },

                //Location links
                { "tilemaps_url", staticMapLink },
                { "gmaps_url", gmapsLocationLink },
                { "applemaps_url", appleMapsLocationLink },
                { "wazemaps_url", wazeMapsLocationLink },
                { "scanmaps_url", scannerMapsLocationLink },

                //Pokestop properties
                { "pokestop_id", PokestopId ?? defaultMissingValue },
                { "pokestop_name", Name ?? defaultMissingValue },
                { "pokestop_url", Url ?? defaultMissingValue },
                { "lure_img_url", lureImageUrl },
                { "invasion_img_url", invasionImageUrl },

                { "address", address?.Address },

                // Discord Guild properties
                { "guild_name", guild?.Name },
                { "guild_img_url", guild?.IconUrl },

                { "date_time", DateTime.Now.ToString() },

                //Misc properties
                { "br", "\r\n" }
            };
            return dict;
        }

        #endregion
    }

    /// <summary>
    /// Pokestop lure type
    /// </summary>
    public enum PokestopLureType
    {
        /// <summary>
        /// No Pokestop lure deployed
        /// </summary>
        None = 0,

        /// <summary>
        /// Normal Pokestop lure deployed
        /// </summary>
        Normal = 501,

        /// <summary>
        /// Glacial Pokestop lure deployed
        /// </summary>
        Glacial = 502,

        /// <summary>
        /// Mossy Pokestop lure deployed
        /// </summary>
        Mossy = 503,

        /// <summary>
        /// Magnetic Pokestop lure deployed
        /// </summary>
        Magnetic = 504
    }

    /// <summary>
    /// Pokestop display type
    /// </summary>
    public enum PokestopDisplay
    {
        /// <summary>
        /// Normal Pokestop
        /// </summary>
        Normal = 0,

        /// <summary>
        /// Team Rocket Invasion Pokestop
        /// </summary>
        RocketInvasion,

        /// <summary>
        /// Team Rocket victory Pokestop
        /// </summary>
        RocketVictory
    }

    /// <summary>
    /// Team Rocket Invasion grunt type
    /// </summary>
    public enum InvasionGruntType
    {
        Unset = 0,
        Blanche,
        Candela,
        Spark,
        MaleGrunt,
        FemaleGrunt,
        BugFemaleGrunt,
        BugMaleGrunt,
        DarknessFemaleGrunt,
        DarknessMaleGrunt,
        DarkFemaleGrunt,
        DarkMaleGrunt,
        DragonFemaleGrunt,
        DragonMaleGrunt,
        FairyFemaleGrunt,
        FairyMaleGrunt,
        FightingFemaleGrunt,
        FightingMaleGrunt,
        FireFemaleGrunt,
        FireMaleGrunt,
        FlyingFemaleGrunt,
        FlyingMaleGrunt,
        GrassFemaleGrunt,
        GrassMaleGrunt,
        GroundFemaleGrunt,
        GroundMaleGrunt,
        IceFemaleGrunt,
        IceMaleGrunt,
        MetalFemaleGrunt,
        MetalMaleGrunt,
        NormalFemaleGrunt,
        NormalMaleGrunt,
        PoisonFemaleGrunt,
        PoisonMaleGrunt,
        PsychicFemaleGrunt,
        PsychicMaleGrunt,
        RockFemaleGrunt,
        RockMaleGrunt,
        WaterFemaleGrunt,
        WaterMaleGrunt,
        PlayerTeamLeader,
        ExecutiveCliff,
        ExecutiveArlo,
        ExecutiveSierra,
        Giovanni,
        DecoyMale,
        DecoyFemale,
        GhostFemaleGrunt,
        GhostMaleGrunt,
        ElectricFemaleGrunt,
        ElectricMaleGrunt
    }
}
