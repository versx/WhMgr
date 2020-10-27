namespace WhMgr.Net.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.IO;
    using System.Linq;

    using DSharpPlus;
    using DSharpPlus.Entities;
    using Newtonsoft.Json;

    using WhMgr.Alarms.Alerts;
    using WhMgr.Alarms.Models;
    using WhMgr.Configuration;
    using WhMgr.Data;
    using WhMgr.Data.Factories;
    using WhMgr.Diagnostics;
    using WhMgr.Utilities;

    /// <summary>
    /// RealDeviceMap Gym Details webhook model class.
    /// </summary>
    [Table("gym")]
    public sealed class GymDetailsData
    {
        private static readonly IEventLogger _logger = EventLogger.GetLogger("GYMDETAILSDATA", Program.LogLevel);

        public const string WebhookHeader = "gym_details";

        #region Properties

        [
            JsonProperty("id"),
            Column("id"),
            Key
        ]
        public string GymId { get; set; }

        [
            JsonProperty("name"),
            Column("name")
        ]
        public string GymName { get; set; } = "Unknown";

        [
            JsonProperty("url"),
            Column("name")
        ]
        public string Url { get; set; }

        [
            JsonProperty("latitude"),
            Column("lat")
        ]
        public double Latitude { get; set; }

        [
            JsonProperty("longitude"),
            Column("lon")
        ]
        public double Longitude { get; set; }

        [
            JsonProperty("team"),
            Column("team_id")
        ]
        public PokemonTeam Team { get; set; } = PokemonTeam.Neutral;

        [
            JsonProperty("slots_available"),
            Column("availble_slots") // TODO: Typflo
        ]
        public ushort SlotsAvailable { get; set; }

        [
            JsonProperty("sponsor_id"),
            Column("sponsor_id")
        ]
        public bool SponsorId { get; set; }

        [
            JsonProperty("in_battle"),
            Column("in_battle")
        ]
        public bool InBattle { get; set; }

        #endregion

        public DiscordEmbedNotification GenerateGymMessage(ulong guildId, DiscordClient client, WhConfig whConfig, AlarmObject alarm, GymDetailsData oldGym, string city)
        {
            var alertType = AlertMessageType.Gyms;
            var alert = alarm?.Alerts[alertType] ?? AlertMessage.Defaults[alertType];
            var properties = GetProperties(client.Guilds[guildId], whConfig, city, oldGym);
            var eb = new DiscordEmbedBuilder
            {
                Title = DynamicReplacementEngine.ReplaceText(alert.Title, properties),
                Url = DynamicReplacementEngine.ReplaceText(alert.Url, properties),
                ImageUrl = DynamicReplacementEngine.ReplaceText(alert.ImageUrl, properties),
                ThumbnailUrl = DynamicReplacementEngine.ReplaceText(alert.IconUrl, properties),
                Description = DynamicReplacementEngine.ReplaceText(alert.Content, properties),
                Color = Team == PokemonTeam.Mystic ? DiscordColor.Blue :
                        Team == PokemonTeam.Valor ? DiscordColor.Red :
                        Team == PokemonTeam.Instinct ? DiscordColor.Yellow :
                        DiscordColor.LightGray,
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = DynamicReplacementEngine.ReplaceText(alert.Footer?.Text ?? client.Guilds[guildId]?.Name ?? DateTime.Now.ToString(), properties),
                    IconUrl = DynamicReplacementEngine.ReplaceText(alert.Footer?.IconUrl ?? client.Guilds[guildId]?.IconUrl ?? string.Empty, properties)
                }
            };
            var username = DynamicReplacementEngine.ReplaceText(alert.Username, properties);
            var iconUrl = DynamicReplacementEngine.ReplaceText(alert.AvatarUrl, properties);
            var description = DynamicReplacementEngine.ReplaceText(alarm?.Description, properties);
            return new DiscordEmbedNotification(username, iconUrl, description, new List<DiscordEmbed> { eb.Build() });
        }

        private IReadOnlyDictionary<string, string> GetProperties(DiscordGuild guild, WhConfig whConfig, string city, GymDetailsData oldGym)
        {
            var exEmojiId = MasterFile.Instance.Emojis["ex"];
            var exEmoji = string.IsNullOrEmpty(MasterFile.Instance.CustomEmojis["ex"]) ? exEmojiId > 0
                ? string.Format(Strings.EmojiSchema, "ex", exEmojiId): "EX"
                : MasterFile.Instance.CustomEmojis["ex"];
            var teamEmojiId = MasterFile.Instance.Emojis[Team.ToString().ToLower()];
            var teamEmoji = string.IsNullOrEmpty(MasterFile.Instance.CustomEmojis[Team.ToString().ToLower()])
                ? teamEmojiId > 0
                    ? string.Format(Strings.EmojiSchema, Team.ToString().ToLower(), teamEmojiId)
                    : Team.ToString()
                : MasterFile.Instance.CustomEmojis[Team.ToString().ToLower()];
            var oldTeamEmojiId = MasterFile.Instance.Emojis[oldGym?.Team.ToString().ToLower()];
            var oldTeamEmoji = string.IsNullOrEmpty(MasterFile.Instance.CustomEmojis[oldGym?.Team.ToString().ToLower()])
                ? oldTeamEmojiId > 0
                    ? string.Format(Strings.EmojiSchema, oldGym?.Team.ToString().ToLower(), oldTeamEmojiId)
                    : oldGym?.Team.ToString()
                : MasterFile.Instance.CustomEmojis[oldGym.Team.ToString().ToLower()];

            var gmapsLink = string.Format(Strings.GoogleMaps, Latitude, Longitude);
            var appleMapsLink = string.Format(Strings.AppleMaps, Latitude, Longitude);
            var wazeMapsLink = string.Format(Strings.WazeMaps, Latitude, Longitude);
            var scannerMapsLink = string.Format(whConfig.Urls.ScannerMap, Latitude, Longitude);
            var templatePath = Path.Combine(whConfig.StaticMaps.TemplatesFolder, whConfig.StaticMaps.Gyms.TemplateFile);
            var staticMapLink = Utils.GetStaticMapsUrl(templatePath, whConfig.Urls.StaticMap, whConfig.StaticMaps.Gyms.ZoomLevel, Latitude, Longitude, /*TODO: Add team image*/string.Empty, Team);
            //var staticMapLink = string.Format(whConfig.Urls.StaticMap, Latitude, Longitude);//whConfig.Urls.StaticMap.Gyms.Enabled ? string.Format(whConfig.Urls.StaticMap.Gyms.Url, Latitude, Longitude) : string.Empty
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

            const string defaultMissingValue = "?";
            var dict = new Dictionary<string, string>
            {
                //Main properties
                { "gym_id", GymId },
                { "gym_name", GymName },
                { "gym_url", Url },
                { "gym_team", Team.ToString() },
                { "gym_team_id", Convert.ToInt32(Team).ToString() },
                { "gym_team_emoji", teamEmoji },
                { "old_gym_team", oldGym.Team.ToString() },
                { "old_gym_team_id", Convert.ToInt32(oldGym.Team).ToString() },
                { "old_gym_team_emoji", oldTeamEmoji },
                { "team_changed", Convert.ToString(oldGym?.Team != Team) },
                { "in_battle", Convert.ToString(InBattle) },
                { "under_attack", Convert.ToString(InBattle) },
                { "is_ex", Convert.ToString(SponsorId) },
                { "ex_emoji", exEmoji },
                { "slots_available", SlotsAvailable == 0
                                        ? "Full"
                                        : SlotsAvailable == 6
                                            ? "Empty"
                                            : SlotsAvailable.ToString("N0") },

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

        internal static Dictionary<string, GymDetailsData> GetGyms(string connectionString = "")
        {
            if (string.IsNullOrEmpty(connectionString))
                return null;

            try
            {
                using (var db = DbContextFactory.CreateScannerDbContext(connectionString))
                {
                    return db.Gyms?.ToDictionary(x => x.GymId, x => x);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }

            return null;
        }
    }
}