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
    using WhMgr.Utilities;

    /// <summary>
    /// RealDeviceMap Gym Details webhook model class.
    /// </summary>
    public sealed class GymDetailsData
    {
        public const string WebhookHeader = "gym_details";

        #region Properties

        [JsonProperty("id")]
        public string GymId { get; set; }

        [JsonProperty("name")]
        public string GymName { get; set; } = "Unknown";

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("latitude")]
        public double Latitude { get; set; }

        [JsonProperty("longitude")]
        public double Longitude { get; set; }

        [JsonProperty("team")]
        public PokemonTeam Team { get; set; } = PokemonTeam.Neutral;

        [JsonProperty("slots_available")]
        public ushort SlotsAvailable { get; set; }

        [JsonProperty("sponsor_id")]
        public bool SponsorId { get; set; }

        [JsonProperty("in_battle")]
        public bool InBattle { get; set; }

        #endregion

        public DiscordEmbed GenerateGymMessage(ulong guildId, DiscordClient client, WhConfig whConfig, AlarmObject alarm, GymDetailsData oldGym, string city)
        {
            var alertType = AlertMessageType.Gyms;
            var alert = alarm?.Alerts[alertType] ?? AlertMessage.Defaults[alertType];
            var properties = GetProperties(whConfig, city, oldGym);
            var mention = DynamicReplacementEngine.ReplaceText(alarm.Mentions, properties);
            var description = DynamicReplacementEngine.ReplaceText(alert.Content, properties);
            var eb = new DiscordEmbedBuilder
            {
                Title = DynamicReplacementEngine.ReplaceText(alert.Title, properties),
                Url = DynamicReplacementEngine.ReplaceText(alert.Url, properties),
                ImageUrl = DynamicReplacementEngine.ReplaceText(alert.ImageUrl, properties),
                ThumbnailUrl = DynamicReplacementEngine.ReplaceText(alert.IconUrl , properties),
                Description = mention + description,
                Color = Team == PokemonTeam.Mystic ? DiscordColor.Blue :
                    Team == PokemonTeam.Valor ? DiscordColor.Red :
                    Team == PokemonTeam.Instinct ? DiscordColor.Yellow :
                    DiscordColor.LightGray,
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = $"{(client.Guilds?[guildId]?.Name ?? Strings.Creator)} | {DateTime.Now}",
                    IconUrl = client.Guilds?[guildId]?.IconUrl ?? string.Empty
                }
            };
            return eb.Build();
        }

        private IReadOnlyDictionary<string, string> GetProperties(WhConfig whConfig, string city, GymDetailsData oldGym)
        {
            var exEmojiId = MasterFile.Instance.Emojis["ex"];
            var exEmoji = exEmojiId > 0 ? $"<:ex:{exEmojiId}>" : "EX";
            var teamEmojiId = MasterFile.Instance.Emojis[Team.ToString().ToLower()];
            var teamEmoji = teamEmojiId > 0 ? $"<:{Team.ToString().ToLower()}:{teamEmojiId}>" : Team.ToString();
            var oldTeamEmojiId = MasterFile.Instance.Emojis[oldGym.Team.ToString().ToLower()];
            var oldTeamEmoji = oldTeamEmojiId > 0 ? $"<:{oldGym.Team.ToString().ToLower()}:{oldTeamEmojiId}>" : oldGym.Team.ToString();

            var gmapsLink = string.Format(Strings.GoogleMaps, Latitude, Longitude);
            var appleMapsLink = string.Format(Strings.AppleMaps, Latitude, Longitude);
            var wazeMapsLink = string.Format(Strings.WazeMaps, Latitude, Longitude);
            var templatesFolder = Path.Combine(Directory.GetCurrentDirectory(), Strings.TemplatesFolder);
            // TODO: Use team icon for gym
            var staticMapLink = Utils.GetStaticMapsUrl(Path.Combine(templatesFolder, whConfig.StaticMaps.WeatherTemplateFile), whConfig.Urls.StaticMap.Replace("/15/", "/11/"), Latitude, Longitude, "https://static.thenounproject.com/png/727778-200.png");
            //var staticMapLink = string.Format(whConfig.Urls.StaticMap, Latitude, Longitude);//whConfig.Urls.StaticMap.Gyms.Enabled ? string.Format(whConfig.Urls.StaticMap.Gyms.Url, Latitude, Longitude) : string.Empty
            var gmapsLocationLink = string.IsNullOrEmpty(whConfig.ShortUrlApiUrl) ? gmapsLink : NetUtil.CreateShortUrl(whConfig.ShortUrlApiUrl, gmapsLink);
            var appleMapsLocationLink = string.IsNullOrEmpty(whConfig.ShortUrlApiUrl) ? appleMapsLink : NetUtil.CreateShortUrl(whConfig.ShortUrlApiUrl, appleMapsLink);
            var wazeMapsLocationLink = string.IsNullOrEmpty(whConfig.ShortUrlApiUrl) ? wazeMapsLink : NetUtil.CreateShortUrl(whConfig.ShortUrlApiUrl, wazeMapsLink);
            //var staticMapLocationLink = string.IsNullOrEmpty(whConfig.ShortUrlApiUrl) ? staticMapLink : NetUtil.CreateShortUrl(whConfig.ShortUrlApiUrl, staticMapLink);

            const string defaultMissingValue = "?";
            var dict = new Dictionary<string, string>
            {
                //Main properties
                { "gym_id", GymId },
                { "gym_name", GymName },
                { "gym_url", Url },
                { "gym_team", Team.ToString() },
                { "gym_team_emoji", teamEmoji },
                { "old_gym_team", oldGym.Team.ToString() },
                { "old_gym_team_emoji", oldTeamEmoji },
                { "team_changed", Convert.ToString(oldGym?.Team != Team) },
                { "in_battle", Convert.ToString(InBattle) },
                { "under_attack", Convert.ToString(InBattle) },
                { "is_ex", Convert.ToString(SponsorId) },
                { "ex_emoji", exEmoji },
                { "slots_available", SlotsAvailable.ToString("N0") },

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

                //Misc properties
                { "br", "\r\n" }
            };
            return dict;
        }
    }
}