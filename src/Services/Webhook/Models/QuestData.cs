﻿namespace WhMgr.Services.Webhook.Models
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json.Serialization;
    using System.Threading.Tasks;

    using DSharpPlus.Entities;
    using POGOProtos.Rpc;

    using WhMgr.Data;
    using WhMgr.Extensions;
    using WhMgr.Services.Alarms;
    using WhMgr.Services.Alarms.Embeds;
    using WhMgr.Services.Discord.Models;
    using WhMgr.Services.Geofence;
    using WhMgr.Services.Geofence.Geocoding;
    using WhMgr.Services.Webhook.Models.Quests;
    using WhMgr.Utilities;

    public sealed class QuestData : IWebhookData
    {
        #region Properties

        [JsonPropertyName("pokestop_id")]
        public string PokestopId { get; set; }

        [JsonPropertyName("latitude")]
        public double Latitude { get; set; }

        [JsonPropertyName("longitude")]
        public double Longitude { get; set; }

        [JsonPropertyName("pokestop_name")]
        public string PokestopName { get; set; }

        [JsonPropertyName("pokestop_url")]
        public string PokestopUrl { get; set; }

        [JsonPropertyName("type")]
        public QuestType Type { get; set; }

        [JsonPropertyName("target")]
        public int Target { get; set; }

        [JsonPropertyName("template")]
        public string Template { get; set; }

        [JsonPropertyName("updated")]
        public long Updated { get; set; }

        [JsonPropertyName("rewards")]
        public List<QuestRewardMessage> Rewards { get; set; }

        [JsonPropertyName("conditions")]
        public List<QuestConditionMessage> Conditions { get; set; }

        [JsonIgnore]
        public bool IsDitto => Rewards?[0]?.Info?.Ditto ?? false;

        [JsonIgnore]
        public bool IsShiny => Rewards?[0]?.Info?.Shiny ?? false;

        #endregion

        /// <summary>
        /// Instantiate a new <see cref="QuestData"/> class.
        /// </summary>
        public QuestData()
        {
            Rewards = new List<QuestRewardMessage>();
            Conditions = new List<QuestConditionMessage>();
        }

        public void SetTimes()
        {
            // No times to change
        }

        /// <summary>
        /// Generates a Discord embed message for a Pokestop Quest
        /// </summary>
        /// <param name="guildId">Discord Guild ID related to the data</param>
        /// <param name="client">Discord client to use</param>
        /// <param name="whConfig">Config to use</param>
        /// <param name="alarm">Alarm to use</param>
        /// <param name="city">City to specify</param>
        /// <returns></returns>
        public async Task<DiscordWebhookMessage> GenerateEmbedMessageAsync(AlarmMessageSettings settings)
        {
            var server = settings.Config.Instance.Servers[settings.GuildId];
            var embedType = EmbedMessageType.Quests;
            var embed = settings.Alarm?.Embeds[embedType]
                ?? server.DmEmbeds?[embedType]
                ?? EmbedMessage.Defaults[embedType];
            settings.ImageUrl = IconFetcher.Instance.GetQuestIcon(settings.Config.Instance.Servers[settings.GuildId].IconStyle, this);
            var properties = await GetPropertiesAsync(settings);
            var eb = new DiscordEmbedMessage
            {
                Title = TemplateRenderer.Parse(embed.Title, properties),
                Url = TemplateRenderer.Parse(embed.Url, properties),
                Image = new Discord.Models.DiscordEmbedImage
                {
                    Url = TemplateRenderer.Parse(embed.ImageUrl, properties),
                },
                Thumbnail = new Discord.Models.DiscordEmbedImage
                {
                    Url = TemplateRenderer.Parse(embed.IconUrl, properties),
                },
                Description = TemplateRenderer.Parse(embed.Content, properties),
                Color = new DiscordColor(MasterFile.Instance.DiscordEmbedColors.Pokestops.Quests).Value,
                Footer = new Discord.Models.DiscordEmbedFooter
                {
                    Text = TemplateRenderer.Parse(embed.Footer?.Text, properties),
                    IconUrl = TemplateRenderer.Parse(embed.Footer?.IconUrl, properties)
                }
            };
            var username = TemplateRenderer.Parse(embed.Username, properties);
            var iconUrl = TemplateRenderer.Parse(embed.AvatarUrl, properties);
            var description = TemplateRenderer.Parse(settings.Alarm?.Description, properties);
            return new DiscordWebhookMessage
            {
                Username = username,
                AvatarUrl = iconUrl,
                Content = description,
                Embeds = new List<DiscordEmbedMessage> { eb },
            };
        }

        private async Task<dynamic> GetPropertiesAsync(AlarmMessageSettings properties)
        {
            var questMessage = this.GetQuestMessage();
            var questConditions = this.GetConditions();
            var questReward = this.GetReward();
            var gmapsLink = string.Format(Strings.GoogleMaps, Latitude, Longitude);
            var appleMapsLink = string.Format(Strings.AppleMaps, Latitude, Longitude);
            var wazeMapsLink = string.Format(Strings.WazeMaps, Latitude, Longitude);
            var scannerMapsLink = string.Format(properties.Config.Instance.Urls.ScannerMap, Latitude, Longitude);

            var staticMapConfig = properties.Config.Instance.StaticMaps[StaticMapType.Quests];
            var staticMap = new StaticMapGenerator(new StaticMapOptions
            {
                BaseUrl = staticMapConfig.Url,
                TemplateName = staticMapConfig.TemplateName,
                Latitude = Latitude,
                Longitude = Longitude,
                SecondaryImageUrl = properties.ImageUrl,
                Gyms = staticMapConfig.IncludeNearbyGyms
                    // Fetch nearby gyms from MapDataCache
                    ? await properties.MapDataCache.GetGymsNearby(Latitude, Longitude).ConfigureAwait(false)
                    : new List<dynamic>(),
                Pokestops = staticMapConfig.IncludeNearbyPokestops
                    // Fetch nearby gyms from MapDataCache
                    ? await properties.MapDataCache.GetPokestopsNearby(Latitude, Longitude).ConfigureAwait(false)
                    : new List<dynamic>(),
            });
            var staticMapLink = staticMap.GenerateLink();
            var gmapsLocationLink = UrlShortener.CreateShortUrl(properties.Config.Instance.ShortUrlApiUrl, gmapsLink);
            var appleMapsLocationLink = UrlShortener.CreateShortUrl(properties.Config.Instance.ShortUrlApiUrl, appleMapsLink);
            var wazeMapsLocationLink = UrlShortener.CreateShortUrl(properties.Config.Instance.ShortUrlApiUrl, wazeMapsLink);
            var scannerMapsLocationLink = UrlShortener.CreateShortUrl(properties.Config.Instance.ShortUrlApiUrl, scannerMapsLink);
            var address = ReverseGeocodingLookup.Instance.GetAddress(new Coordinate(Latitude, Longitude));
            var guild = properties.Client.Guilds.ContainsKey(properties.GuildId) ? properties.Client.Guilds[properties.GuildId] : null;

            const string defaultMissingValue = "?";
            var dict = new
            {
                // Main properties
                quest_task = questMessage,
                quest_conditions = questConditions,
                quest_reward = questReward,
                quest_reward_img_url = properties.ImageUrl,
                has_quest_conditions = !string.IsNullOrEmpty(questConditions),
                is_ditto = IsDitto,
                is_shiny = IsShiny,

                // Location properties
                geofence = properties.City ?? defaultMissingValue,
                lat = Latitude.ToString(),
                lng = Longitude.ToString(),
                lat_5 = Latitude.ToString("0.00000"),
                lng_5 = Longitude.ToString("0.00000"),

                // Location links
                tilemaps_url = staticMapLink,
                gmaps_url = gmapsLocationLink,
                applemaps_url = appleMapsLocationLink,
                wazemaps_url = wazeMapsLocationLink,
                scanmaps_url = scannerMapsLocationLink,

                address = address ?? string.Empty,

                // Pokestop properties
                pokestop_id = PokestopId ?? defaultMissingValue,
                pokestop_name = PokestopName ?? defaultMissingValue,
                pokestop_url = PokestopUrl ?? defaultMissingValue,

                // Discord Guild properties
                guild_name = guild?.Name,
                guild_img_url = guild?.IconUrl,

                //M isc properties
                date_time = DateTime.Now.ToString(),
                br = "\n",
            };
            return dict;
        }
    }
}