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
    using WhMgr.Extensions;
    using WhMgr.Utilities;

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

        [JsonProperty("lure_id")]
        public PokestopLureType LureType { get; set; }

        [JsonIgnore]
        public DateTime LureExpireTime { get; set; }

        [JsonIgnore]
        public DateTime InvasionExpireTime { get; set; }

        [JsonProperty("pokestop_display")]
        public PokestopDisplay PokestopDisplay { get; set; }

        [JsonProperty("incident_expire_timestamp")]
        public long IncidentExpire { get; set; }

        [JsonProperty("grunt_type")]
        public InvasionGruntType GruntType { get; set; }

        [JsonProperty("last_modified")]
        public ulong LastModified { get; set; }

        [JsonProperty("updated")]
        public ulong Updated { get; set; }

        public bool HasLure => LureExpire > 0 && LureType != PokestopLureType.None && LureExpireTime > DateTime.Now;

        public bool HasInvasion => IncidentExpire > 0 && InvasionExpireTime > DateTime.Now;

        #endregion

        public PokestopData()
        {
            SetTimes();
        }

        public void SetTimes()
        {
            LureExpireTime = LureExpire.FromUnix();
            //if (TimeZoneInfo.Local.IsDaylightSavingTime(StartTime))
            //{
            //    StartTime = StartTime.AddHours(1); //DST
            //}

            InvasionExpireTime = IncidentExpire.FromUnix();
            //if (TimeZoneInfo.Local.IsDaylightSavingTime(EndTime))
            //{
            //    EndTime = EndTime.AddHours(1); //DST
            //}
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

        public DiscordEmbed GeneratePokestopMessage(DiscordClient client, WhConfig whConfig, AlarmObject alarm, string city)
        {
            var alertType = HasInvasion ? AlertMessageType.Invasions : HasLure ? AlertMessageType.Lures : AlertMessageType.Pokestops;
            var alert = alarm?.Alerts[alertType] ?? AlertMessage.Defaults[alertType];
            var properties = GetProperties(client, whConfig, city);
            var eb = new DiscordEmbedBuilder
            {
                Title = DynamicReplacementEngine.ReplaceText(alert.Title, properties),
                Url = DynamicReplacementEngine.ReplaceText(alert.Url, properties),
                ImageUrl = properties["tilemaps_url"],
                ThumbnailUrl = Url,
                Description = DynamicReplacementEngine.ReplaceText(alert.Content, properties),
                Color = HasInvasion ? DiscordColor.Red : HasLure ?
                    (LureType == PokestopLureType.Normal ? DiscordColor.HotPink
                    : LureType == PokestopLureType.Glacial ? DiscordColor.CornflowerBlue
                    : LureType == PokestopLureType.Mossy ? DiscordColor.SapGreen
                    : LureType == PokestopLureType.Magnetic ? DiscordColor.Gray
                    : DiscordColor.CornflowerBlue) : DiscordColor.CornflowerBlue
            };
            return eb.Build();
        }

        private IReadOnlyDictionary<string, string> GetProperties(DiscordClient client, WhConfig whConfig, string city)
        {
            var gmapsLink = string.Format(Strings.GoogleMaps, Latitude, Longitude);
            var appleMapsLink = string.Format(Strings.AppleMaps, Latitude, Longitude);
            var staticMapLink = string.Format(whConfig.Urls.StaticMap, Latitude, Longitude);
            var gmapsLocationLink = string.IsNullOrEmpty(whConfig.ShortUrlApiUrl) ? gmapsLink : NetUtil.CreateShortUrl(whConfig.ShortUrlApiUrl, gmapsLink);
            var appleMapsLocationLink = string.IsNullOrEmpty(whConfig.ShortUrlApiUrl) ? appleMapsLink : NetUtil.CreateShortUrl(whConfig.ShortUrlApiUrl, appleMapsLink);
            var gmapsStaticMapLink = string.IsNullOrEmpty(whConfig.ShortUrlApiUrl) ? staticMapLink : NetUtil.CreateShortUrl(whConfig.ShortUrlApiUrl, staticMapLink);
            var invasion = new TeamRocketInvasion(GruntType);
            var invasionEncounters = invasion.GetPossibleInvasionEncounters();

            const string defaultMissingValue = "?";
            var dict = new Dictionary<string, string>
            {
                //Main properties
                { "has_lure", HasLure ? "Yes" : "No" },
                { "lure_type", LureType.ToString() },
                { "lure_expire_time", LureExpireTime.ToLongTimeString() },
                { "lure_expire_time_left", LureExpireTime.GetTimeRemaining().ToReadableStringNoSeconds() },
                { "has_invasion", HasInvasion ? "Yes" : "No" },
                { "grunt_type", invasion.Type == PokemonType.None ? "Tier II" : invasion?.Type.ToString() },
                { "grunt_type_emoji", invasion.Type == PokemonType.None ? "Tier II" : invasion.Type.GetTypeEmojiIcons(client, whConfig.GuildId) },
                { "grunt_gender", invasion.Gender.ToString() },
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
                { "tilemaps_url", gmapsStaticMapLink },
                { "gmaps_url", gmapsLocationLink },
                { "applemaps_url", appleMapsLocationLink },

                //Pokestop properties
                { "pokestop_id", PokestopId ?? defaultMissingValue },
                { "pokestop_name", Name ?? defaultMissingValue },
                { "pokestop_url", Url ?? defaultMissingValue },

                //Misc properties
                { "br", "\r\n" }
            };
            return dict;
        }
    }

    public class TeamRocketInvasion
    {
        public PokemonType Type { get; set; }

        public PokemonGender Gender { get; set; }

        public bool SecondReward { get; set; }

        public bool HasEncounter
        {
            get
            {
                return Encounters?.First?.Count > 0 || Encounters?.Second?.Count > 0 || Encounters?.Third?.Count > 0;
            }
        }

        public TeamRocketEncounters Encounters { get; set; }

        public TeamRocketInvasion()
        {
            Type = PokemonType.None;
            Gender = PokemonGender.Unset;
            Encounters = new TeamRocketEncounters();
        }

        public TeamRocketInvasion(InvasionGruntType gruntType)
        {
            var gender = PokemonGender.Unset;
            var type = PokemonType.None;
            var secondReward = false;
            var encounters = new TeamRocketEncounters();
            switch (gruntType)
            {
                case InvasionGruntType.Unset:
                case InvasionGruntType.Blanche:
                case InvasionGruntType.Candela:
                case InvasionGruntType.Spark:
                    break;
                case InvasionGruntType.MaleGrunt:
                    gender = PokemonGender.Male;
                    secondReward = true;
                    encounters.First = new List<int> { 1, 4, 7 };
                    encounters.Second = new List<int> { 2, 5, 8 };
                    encounters.Third = new List<int> { 3, 6, 9 };
                    break;
                case InvasionGruntType.FemaleGrunt:
                    gender = PokemonGender.Female;
                    secondReward = false;
                    encounters.First = new List<int> { 143, 131 };
                    encounters.Second = new List<int> { 143, 62, 282 };
                    encounters.Third = new List<int> { 143, 149, 130 };
                    break;
                case InvasionGruntType.BugFemaleGrunt:
                    gender = PokemonGender.Female;
                    type = PokemonType.Bug;
                    break;
                case InvasionGruntType.BugMaleGrunt:
                    gender = PokemonGender.Male;
                    type = PokemonType.Bug;
                    secondReward = false;
                    encounters.First = new List<int> { 13, 48, 123 };
                    encounters.Second = new List<int> { 14, 49, 212 };
                    encounters.Third = new List<int> { 15, 123, 212 };
                    break;
                case InvasionGruntType.DarknessFemaleGrunt:
                    gender = PokemonGender.Female;
                    type = PokemonType.Ghost;
                    break;
                case InvasionGruntType.DarknessMaleGrunt:
                    gender = PokemonGender.Male;
                    type = PokemonType.Ghost;
                    secondReward = false;
                    encounters.First = new List<int> { 302, 353, 355 };
                    encounters.Second = new List<int> { 302, 354, 356 };
                    encounters.Third = new List<int> { 302, 354, 477 };
                    break;
                case InvasionGruntType.DarkFemaleGrunt:
                    gender = PokemonGender.Female;
                    type = PokemonType.Dark;
                    break;
                case InvasionGruntType.DarkMaleGrunt:
                    gender = PokemonGender.Male;
                    type = PokemonType.Dark;
                    break;
                case InvasionGruntType.DragonFemaleGrunt:
                    gender = PokemonGender.Female;
                    type = PokemonType.Dragon;
                    secondReward = false;
                    encounters.First = new List<int> { 147 };
                    encounters.Second = new List<int> { 147, 148, 330 };
                    encounters.Third = new List<int> { 130, 148, 149 };
                    break;
                case InvasionGruntType.DragonMaleGrunt:
                    gender = PokemonGender.Male;
                    type = PokemonType.Dragon;
                    break;
                case InvasionGruntType.FairyFemaleGrunt:
                    gender = PokemonGender.Female;
                    type = PokemonType.Fairy;
                    break;
                case InvasionGruntType.FairyMaleGrunt:
                    gender = PokemonGender.Male;
                    type = PokemonType.Fairy;
                    break;
                case InvasionGruntType.FightingFemaleGrunt:
                    gender = PokemonGender.Female;
                    type = PokemonType.Fighting;
                    secondReward = false;
                    encounters.First = new List<int> { 107 };
                    encounters.Second = new List<int> { 107 };
                    encounters.Third = new List<int> { 107 };
                    break;
                case InvasionGruntType.FightingMaleGrunt:
                    gender = PokemonGender.Male;
                    type = PokemonType.Fighting;
                    break;
                case InvasionGruntType.FireFemaleGrunt:
                    gender = PokemonGender.Female;
                    type = PokemonType.Fire;
                    secondReward = true;
                    encounters.First = new List<int> { 58, 126, 228 };
                    encounters.Second = new List<int> { 5, 229 };
                    encounters.Third = new List<int> { 5, 59, 229 };
                    break;
                case InvasionGruntType.FireMaleGrunt:
                    gender = PokemonGender.Male;
                    type = PokemonType.Fire;
                    break;
                case InvasionGruntType.FlyingFemaleGrunt:
                    gender = PokemonGender.Female;
                    type = PokemonType.Flying;
                    secondReward = false;
                    encounters.First = new List<int> { 41, 42 };
                    encounters.Second = new List<int> { 42, 123, 169 };
                    encounters.Third = new List<int> { 130, 149, 169 };
                    break;
                case InvasionGruntType.FlyingMaleGrunt:
                    gender = PokemonGender.Male;
                    type = PokemonType.Flying;
                    break;
                case InvasionGruntType.GrassFemaleGrunt:
                    gender = PokemonGender.Female;
                    type = PokemonType.Grass;
                    break;
                case InvasionGruntType.GrassMaleGrunt:
                    gender = PokemonGender.Male;
                    type = PokemonType.Grass;
                    secondReward = true;
                    encounters.First = new List<int> { 273, 331, 387 };
                    encounters.Second = new List<int> { 1, 2, 44 };
                    encounters.Third = new List<int> { 45, 275, 332 };
                    break;
                case InvasionGruntType.GroundFemaleGrunt:
                    gender = PokemonGender.Female;
                    type = PokemonType.Ground;
                    break;
                case InvasionGruntType.GroundMaleGrunt:
                    gender = PokemonGender.Male;
                    type = PokemonType.Ground;
                    secondReward = false;
                    encounters.First = new List<int> { 104, 246, 328 };
                    encounters.Second = new List<int> { 104, 105, 329 };
                    encounters.Third = new List<int> { 105, 130 };
                    break;
                case InvasionGruntType.IceFemaleGrunt:
                    gender = PokemonGender.Female;
                    type = PokemonType.Ice;
                    break;
                case InvasionGruntType.IceMaleGrunt:
                    gender = PokemonGender.Male;
                    type = PokemonType.Ice;
                    break;
                case InvasionGruntType.MetalFemaleGrunt:
                    gender = PokemonGender.Female;
                    type = PokemonType.Steel;
                    break;
                case InvasionGruntType.MetalMaleGrunt:
                    gender = PokemonGender.Male;
                    type = PokemonType.Steel;
                    break;
                case InvasionGruntType.NormalFemaleGrunt:
                    gender = PokemonGender.Female;
                    type = PokemonType.Normal;
                    break;
                case InvasionGruntType.NormalMaleGrunt:
                    gender = PokemonGender.Male;
                    type = PokemonType.Normal;
                    secondReward = true;
                    encounters.First = new List<int> { 19, 41 };
                    encounters.Second = new List<int> { 19, 20 };
                    encounters.Third = new List<int> { 20, 143 };
                    break;
                case InvasionGruntType.PoisonFemaleGrunt:
                    gender = PokemonGender.Female;
                    type = PokemonType.Poison;
                    secondReward = true;
                    encounters.First = new List<int> { 41, 48, 88 };
                    encounters.Second = new List<int> { 42, 88, 89 };
                    encounters.Third = new List<int> { 42, 49, 89 };
                    break;
                case InvasionGruntType.PoisonMaleGrunt:
                    gender = PokemonGender.Male;
                    type = PokemonType.Poison;
                    break;
                case InvasionGruntType.PsychicFemaleGrunt:
                    gender = PokemonGender.Female;
                    type = PokemonType.Psychic;
                    break;
                case InvasionGruntType.PsychicMaleGrunt:
                    gender = PokemonGender.Male;
                    type = PokemonType.Psychic;
                    secondReward = true;
                    encounters.First = new List<int> { 63, 96, 280 };
                    encounters.Second = new List<int> { 96, 97, 280 };
                    encounters.Third = new List<int> { 64, 97, 281 };
                    break;
                case InvasionGruntType.RockFemaleGrunt:
                    gender = PokemonGender.Female;
                    type = PokemonType.Rock;
                    break;
                case InvasionGruntType.RockMaleGrunt:
                    gender = PokemonGender.Male;
                    type = PokemonType.Rock;
                    secondReward = false;
                    encounters.First = new List<int> { 246 };
                    encounters.Second = new List<int> { 246, 247 };
                    encounters.Third = new List<int> { 247, 248 };
                    break;
                case InvasionGruntType.WaterFemaleGrunt:
                    gender = PokemonGender.Female;
                    type = PokemonType.Water;
                    secondReward = false;
                    encounters.First = new List<int> { 54, 60 };
                    encounters.Second = new List<int> { 55, 61 };
                    encounters.Third = new List<int> { 62, 186 };
                    break;
                case InvasionGruntType.WaterMaleGrunt:
                    gender = PokemonGender.Male;
                    type = PokemonType.Water;
                    secondReward = false;
                    encounters.First = new List<int> { 129 };
                    encounters.Second = new List<int> { 129 };
                    encounters.Third = new List<int> { 129, 130 };
                    break;
                case InvasionGruntType.PlayerTeamLeader:
                    break;
            }

            Type = type;
            Gender = gender;
            SecondReward = secondReward;
            Encounters = encounters;
        }

        public static InvasionGruntType GruntTypeToTrInvasion(PokemonType type, PokemonGender gender)
        {
            switch (type)
            {
                case PokemonType.None:
                    switch (gender)
                    {
                        case PokemonGender.Male:
                            return InvasionGruntType.MaleGrunt;
                        case PokemonGender.Female:
                            return InvasionGruntType.FemaleGrunt;
                    }
                    break;
                case PokemonType.Bug:
                    switch (gender)
                    {
                        case PokemonGender.Male:
                            return InvasionGruntType.BugMaleGrunt;
                        case PokemonGender.Female:
                            return InvasionGruntType.BugFemaleGrunt;
                    }
                    break;
                case PokemonType.Dark:
                    switch (gender)
                    {
                        case PokemonGender.Male:
                            return InvasionGruntType.DarkMaleGrunt;
                        case PokemonGender.Female:
                            return InvasionGruntType.DarkFemaleGrunt;
                    }
                    break;
                case PokemonType.Dragon:
                    switch (gender)
                    {
                        case PokemonGender.Male:
                            return InvasionGruntType.DragonMaleGrunt;
                        case PokemonGender.Female:
                            return InvasionGruntType.DragonFemaleGrunt;
                    }
                    break;
                case PokemonType.Electric:
                    switch (gender)
                    {
                        case PokemonGender.Male:
                            break;
                        case PokemonGender.Female:
                            break;
                    }
                    break;
                case PokemonType.Fairy:
                    switch (gender)
                    {
                        case PokemonGender.Male:
                            return InvasionGruntType.FairyMaleGrunt;
                        case PokemonGender.Female:
                            return InvasionGruntType.FairyFemaleGrunt;
                    }
                    break;
                case PokemonType.Fighting:
                    switch (gender)
                    {
                        case PokemonGender.Male:
                            return InvasionGruntType.FightingMaleGrunt;
                        case PokemonGender.Female:
                            return InvasionGruntType.FightingFemaleGrunt;
                    }
                    break;
                case PokemonType.Fire:
                    switch (gender)
                    {
                        case PokemonGender.Male:
                            return InvasionGruntType.FireMaleGrunt;
                        case PokemonGender.Female:
                            return InvasionGruntType.FireFemaleGrunt;
                    }
                    break;
                case PokemonType.Flying:
                    switch (gender)
                    {
                        case PokemonGender.Male:
                            return InvasionGruntType.FlyingMaleGrunt;
                        case PokemonGender.Female:
                            return InvasionGruntType.FlyingFemaleGrunt;
                    }
                    break;
                case PokemonType.Ghost:
                    switch (gender)
                    {
                        case PokemonGender.Male:
                            return InvasionGruntType.DarknessMaleGrunt;
                        case PokemonGender.Female:
                            return InvasionGruntType.DarknessFemaleGrunt;
                    }
                    break;
                case PokemonType.Grass:
                    switch (gender)
                    {
                        case PokemonGender.Male:
                            return InvasionGruntType.GrassMaleGrunt;
                        case PokemonGender.Female:
                            return InvasionGruntType.GrassFemaleGrunt;
                    }
                    break;
                case PokemonType.Ground:
                    switch (gender)
                    {
                        case PokemonGender.Male:
                            return InvasionGruntType.GroundMaleGrunt;
                        case PokemonGender.Female:
                            return InvasionGruntType.GroundFemaleGrunt;
                    }
                    break;
                case PokemonType.Ice:
                    switch (gender)
                    {
                        case PokemonGender.Male:
                            return InvasionGruntType.IceMaleGrunt;
                        case PokemonGender.Female:
                            return InvasionGruntType.IceFemaleGrunt;
                    }
                    break;
                case PokemonType.Normal:
                    switch (gender)
                    {
                        case PokemonGender.Male:
                            return InvasionGruntType.NormalMaleGrunt;
                        case PokemonGender.Female:
                            return InvasionGruntType.NormalFemaleGrunt;
                    }
                    break;
                case PokemonType.Poison:
                    switch (gender)
                    {
                        case PokemonGender.Male:
                            return InvasionGruntType.PoisonMaleGrunt;
                        case PokemonGender.Female:
                            return InvasionGruntType.PoisonFemaleGrunt;
                    }
                    break;
                case PokemonType.Psychic:
                    switch (gender)
                    {
                        case PokemonGender.Male:
                            return InvasionGruntType.PsychicMaleGrunt;
                        case PokemonGender.Female:
                            return InvasionGruntType.PsychicFemaleGrunt;
                    }
                    break;
                case PokemonType.Rock:
                    switch (gender)
                    {
                        case PokemonGender.Male:
                            return InvasionGruntType.RockMaleGrunt;
                        case PokemonGender.Female:
                            return InvasionGruntType.RockFemaleGrunt;
                    }
                    break;
                case PokemonType.Steel:
                    switch (gender)
                    {
                        case PokemonGender.Male:
                            return InvasionGruntType.MetalMaleGrunt;
                        case PokemonGender.Female:
                            return InvasionGruntType.MetalFemaleGrunt;
                    }
                    break;
                case PokemonType.Water:
                    switch (gender)
                    {
                        case PokemonGender.Male:
                            return InvasionGruntType.WaterMaleGrunt;
                        case PokemonGender.Female:
                            return InvasionGruntType.WaterFemaleGrunt;
                    }
                    break;
            }
            return InvasionGruntType.Unset;
        }
    }

    public class TeamRocketEncounters
    {
        public List<int> First { get; set; }

        public List<int> Second { get; set; }

        public List<int> Third { get; set; }

        public TeamRocketEncounters()
        {
            First = new List<int>();
            Second = new List<int>();
            Third = new List<int>();
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
        PlayerTeamLeader
    }
}