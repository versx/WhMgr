namespace WhMgr.Net.Models
{
    using System;
    using System.Collections.Generic;

    using DSharpPlus;
    using DSharpPlus.Entities;
    using Newtonsoft.Json;

    using WhMgr.Alarms.Alerts;
    using WhMgr.Alarms.Models;
    using WhMgr.Configuration;
    using WhMgr.Data;
    using WhMgr.Extensions;
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

        public bool HasLure => LureExpire > 0 && LureType != PokestopLureType.None && LureExpireTime > DateTime.Now;

        public bool HasInvasion => IncidentExpire > 0 && InvasionExpireTime > DateTime.Now;

        #endregion

        /// <summary>
        /// Instantiate a new <see cref="PokestopData"/> class.
        /// </summary>
        public PokestopData()
        {
            SetTimes(false, false);
        }

        #region Public Methods

        /// <summary>
        /// Set expire times because .NET doesn't support Unix timestamp deserialization to <seealso cref="DateTime"/> class by default.
        /// </summary>
        /// <param name="enableDST">Enable Day Light Savings time adjustment.</param>
        /// <param name="enableLeapYear">Enable leap year time adjustment.</param>
        public void SetTimes(bool enableDST, bool enableLeapYear)
        {
            LureExpireTime = LureExpire.FromUnix();
            if (enableDST)//TimeZoneInfo.Local.IsDaylightSavingTime(LureExpireTime))
            {
                LureExpireTime = LureExpireTime.AddHours(1); //DST
            }

            InvasionExpireTime = IncidentExpire.FromUnix();
            if (enableDST)//TimeZoneInfo.Local.IsDaylightSavingTime(InvasionExpireTime))
            {
                InvasionExpireTime = InvasionExpireTime.AddHours(1); //DST
            }

            if (enableLeapYear)
            {
                LureExpireTime = LureExpireTime.Subtract(TimeSpan.FromDays(1));
                InvasionExpireTime = InvasionExpireTime.Subtract(TimeSpan.FromDays(1));
            }
        }

        public static string InvasionTypeToString(InvasionGruntType gruntType)
        {
            switch (gruntType)
            {
                case InvasionGruntType.Unset:
                    return "None";
                case InvasionGruntType.Blanche:
                    return "Blanche";
                case InvasionGruntType.Candela:
                    return "Candela";
                case InvasionGruntType.Spark:
                    return "Spark";
                case InvasionGruntType.MaleGrunt:
                    return "Male Grunt";
                case InvasionGruntType.FemaleGrunt:
                    return "Female Grunt";
                case InvasionGruntType.BugFemaleGrunt:
                    return "Bug - Female Grunt";
                case InvasionGruntType.BugMaleGrunt:
                    return "Bug - Male Grunt";
                case InvasionGruntType.DarknessFemaleGrunt:
                    return "Ghost - Female Grunt";
                case InvasionGruntType.DarknessMaleGrunt:
                    return "Ghost - Male Grunt";
                case InvasionGruntType.DarkFemaleGrunt:
                    return "Dark - Female Grunt";
                case InvasionGruntType.DarkMaleGrunt:
                    return "Dark - Male Grunt";
                case InvasionGruntType.DragonFemaleGrunt:
                    return "Dragon - Female Grunt";
                case InvasionGruntType.DragonMaleGrunt:
                    return "Dragon - Male Grunt";
                case InvasionGruntType.FairyFemaleGrunt:
                    return "Fairy - Female Grunt";
                case InvasionGruntType.FairyMaleGrunt:
                    return "Fairy - Male Grunt";
                case InvasionGruntType.FightingFemaleGrunt:
                    return "Fighting - Female Grunt";
                case InvasionGruntType.FightingMaleGrunt:
                    return "Fighting - Male Grunt";
                case InvasionGruntType.FireFemaleGrunt:
                    return "Fire - Female Grunt";
                case InvasionGruntType.FireMaleGrunt:
                    return "Fire - Male Grunt";
                case InvasionGruntType.FlyingFemaleGrunt:
                    return "Flying - Female Grunt";
                case InvasionGruntType.FlyingMaleGrunt:
                    return "Flying - Male Grunt";
                case InvasionGruntType.GrassFemaleGrunt:
                    return "Grass - Female Grunt";
                case InvasionGruntType.GrassMaleGrunt:
                    return "Grass - Male Grunt";
                case InvasionGruntType.GroundFemaleGrunt:
                    return "Ground - Female Grunt";
                case InvasionGruntType.GroundMaleGrunt:
                    return "Ground - Male Grunt";
                case InvasionGruntType.IceFemaleGrunt:
                    return "Ice - Female Grunt";
                case InvasionGruntType.IceMaleGrunt:
                    return "Ice - Male Grunt";
                case InvasionGruntType.MetalFemaleGrunt:
                    return "Steel - Female Grunt";
                case InvasionGruntType.MetalMaleGrunt:
                    return "Steel - Male Grunt";
                case InvasionGruntType.NormalFemaleGrunt:
                    return "Normal - Female Grunt";
                case InvasionGruntType.NormalMaleGrunt:
                    return "Normal - Male Grunt";
                case InvasionGruntType.PoisonFemaleGrunt:
                    return "Poison - Female Grunt";
                case InvasionGruntType.PoisonMaleGrunt:
                    return "Poison - Male Grunt";
                case InvasionGruntType.PsychicFemaleGrunt:
                    return "Psychic - Female Grunt";
                case InvasionGruntType.PsychicMaleGrunt:
                    return "Psychic - Male Grunt";
                case InvasionGruntType.RockFemaleGrunt:
                    return "Rock - Female Grunt";
                case InvasionGruntType.RockMaleGrunt:
                    return "Rock - Male Grunt";
                case InvasionGruntType.WaterFemaleGrunt:
                    return "Water - Female Grunt";
                case InvasionGruntType.WaterMaleGrunt:
                    return "Water - Male Grunt";
                case InvasionGruntType.PlayerTeamLeader:
                    return "Player Team Leader";
                default:
                    return gruntType.ToString();
            }
        }

        public DiscordEmbed GeneratePokestopMessage(ulong guildId, DiscordClient client, WhConfig whConfig, AlarmObject alarm, string city)
        {
            var alertType = HasInvasion ? AlertMessageType.Invasions : HasLure ? AlertMessageType.Lures : AlertMessageType.Pokestops;
            var alert = alarm?.Alerts[alertType] ?? AlertMessage.Defaults[alertType];
            var properties = GetProperties(whConfig, city);
            var mention = DynamicReplacementEngine.ReplaceText(alarm?.Mentions ?? string.Empty, properties);
            var description = DynamicReplacementEngine.ReplaceText(alert.Content, properties);
            var eb = new DiscordEmbedBuilder
            {
                Title = DynamicReplacementEngine.ReplaceText(alert.Title, properties),
                Url = DynamicReplacementEngine.ReplaceText(alert.Url, properties),
                ImageUrl = DynamicReplacementEngine.ReplaceText(alert.ImageUrl, properties),
                ThumbnailUrl = DynamicReplacementEngine.ReplaceText(alert.IconUrl, properties),
                Description = mention + description,
                Color = HasInvasion ? DiscordColor.Red : HasLure ?
                    (LureType == PokestopLureType.Normal ? DiscordColor.HotPink
                    : LureType == PokestopLureType.Glacial ? DiscordColor.CornflowerBlue
                    : LureType == PokestopLureType.Mossy ? DiscordColor.SapGreen
                    : LureType == PokestopLureType.Magnetic ? DiscordColor.Gray
                    : DiscordColor.CornflowerBlue) : DiscordColor.CornflowerBlue,
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = $"{(client.Guilds?[guildId]?.Name ?? Strings.Creator)} | {DateTime.Now}",
                    IconUrl = client.Guilds?[guildId]?.IconUrl ?? string.Empty
                }
            };
            return eb.Build();
        }

        #endregion

        public static string GetGruntLeaderString(InvasionGruntType gruntType)
        {
            switch (gruntType)
            {
                case InvasionGruntType.Blanche:
                case InvasionGruntType.Candela:
                case InvasionGruntType.Spark:
                    return Convert.ToString(gruntType);
                case InvasionGruntType.ExecutiveArlo:
                    return "Executive Arlo";
                case InvasionGruntType.ExecutiveCliff:
                    return "Executive Cliff";
                case InvasionGruntType.ExecutiveSierra:
                    return "Executive Sierra";
                case InvasionGruntType.Giovanni:
                    return "Giovanni or Decoy";
                case InvasionGruntType.DecoyFemale:
                case InvasionGruntType.DecoyMale:
                    return "Decoy";
                case InvasionGruntType.PlayerTeamLeader:
                    return "Player Team Leader";
                default:
                    return "Tier II";
            }
        }

        #region Private Methods

        private IReadOnlyDictionary<string, string> GetProperties(WhConfig whConfig, string city)
        {
            //var server = whConfig.Servers[guildId];
            string icon;
            if (HasInvasion)
            {
                //TODO: Load from local file
                icon = "http://images2.fanpop.com/image/photos/11300000/Team-Rocket-Logo-team-rocket-11302897-198-187.jpg";
            }
            else if (HasLure)
            {
                icon = string.Format(whConfig.Urls.QuestImage, Convert.ToInt32(LureType));
            }
            else
            {
                icon = Url;
            }
            var gmapsLink = string.Format(Strings.GoogleMaps, Latitude, Longitude);
            var appleMapsLink = string.Format(Strings.AppleMaps, Latitude, Longitude);
            var wazeMapsLink = string.Format(Strings.WazeMaps, Latitude, Longitude);
            var staticMapLink = Utils.PrepareStaticMapUrl(whConfig.Urls.StaticMap, icon, Latitude, Longitude);
            var gmapsLocationLink = string.IsNullOrEmpty(whConfig.ShortUrlApiUrl) ? gmapsLink : NetUtil.CreateShortUrl(whConfig.ShortUrlApiUrl, gmapsLink);
            var appleMapsLocationLink = string.IsNullOrEmpty(whConfig.ShortUrlApiUrl) ? appleMapsLink : NetUtil.CreateShortUrl(whConfig.ShortUrlApiUrl, appleMapsLink);
            var wazeMapsLocationLink = string.IsNullOrEmpty(whConfig.ShortUrlApiUrl) ? wazeMapsLink : NetUtil.CreateShortUrl(whConfig.ShortUrlApiUrl, wazeMapsLink);
            //var staticMapLocationLink = string.IsNullOrEmpty(whConfig.ShortUrlApiUrl) ? staticMapLink : NetUtil.CreateShortUrl(whConfig.ShortUrlApiUrl, staticMapLink);
            var invasion = MasterFile.Instance.GruntTypes.ContainsKey(GruntType) ? MasterFile.Instance.GruntTypes[GruntType] : null;
            var leaderString = GetGruntLeaderString(GruntType);
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
                { "lure_expire_time_left", LureExpireTime.GetTimeRemaining().ToReadableStringNoSeconds() },
                { "has_invasion", Convert.ToString(HasInvasion) },
                { "grunt_type", invasion?.Type },
                { "grunt_type_emoji", invasionTypeEmoji },
                { "grunt_gender", invasion?.Grunt },
                { "invasion_expire_time", InvasionExpireTime.ToLongTimeString() },
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

                //Pokestop properties
                { "pokestop_id", PokestopId ?? defaultMissingValue },
                { "pokestop_name", Name ?? defaultMissingValue },
                { "pokestop_url", Url ?? defaultMissingValue },

                //Misc properties
                { "br", "\r\n" }
            };
            return dict;
        }

        #endregion
    }

    public class TeamRocketInvasion
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("grunt")]
        public string Grunt { get; set; }

        [JsonProperty("second_reward")]
        public bool SecondReward { get; set; }

        [JsonIgnore]
        public bool HasEncounter
        {
            get
            {
                return Encounters?.First?.Count > 0 || Encounters?.Second?.Count > 0 || Encounters?.Third?.Count > 0;
            }
        }

        [JsonProperty("encounters")]
        public TeamRocketEncounters Encounters { get; set; }

        public TeamRocketInvasion()
        {
            Encounters = new TeamRocketEncounters();
        }
    }

    public class TeamRocketEncounters
    {
        [JsonProperty("first")]
        public List<string> First { get; set; }

        [JsonProperty("second")]
        public List<string> Second { get; set; }

        [JsonProperty("third")]
        public List<string> Third { get; set; }

        public TeamRocketEncounters()
        {
            First = new List<string>();
            Second = new List<string>();
            Third = new List<string>();
        }
    }

    public enum PokestopLureType
    {
        None = 0,
        Normal = 501,
        Glacial = 502,
        Mossy = 503,
        Magnetic = 504
    }

    public enum PokestopDisplay
    {
        Normal = 0,
        RocketInvasion,
        RocketVictory
    }

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