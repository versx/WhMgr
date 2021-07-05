namespace WhMgr.Services.Webhook.Models
{
    using DSharpPlus.Entities;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Text.Json.Serialization;

    using WhMgr.Common;
    using WhMgr.Data;
    using WhMgr.Services.Alarms;
    using WhMgr.Services.Alarms.Embeds;
    using WhMgr.Services.Discord.Models;
    using WhMgr.Services.Geofence;
    using WhMgr.Utilities;

    [Table("gym")]
    public sealed class GymDetailsData : IWebhookData
    {
        #region Properties

        [
            JsonPropertyName("id"),
            Column("id"),
            Key,
        ]
        public string GymId { get; set; }

        [
            JsonPropertyName("name"),
            Column("name"),
        ]
        public string GymName { get; set; } = "Unknown";

        [
            JsonPropertyName("url"),
            Column("url"),
        ]
        public string Url { get; set; }

        [
            JsonPropertyName("latitude"),
            Column("lat"),
        ]
        public double Latitude { get; set; }

        [
            JsonPropertyName("longitude"),
            Column("lon"),
        ]
        public double Longitude { get; set; }

        [
            JsonPropertyName("team"),
            Column("team_id"),
        ]
        public PokemonTeam Team { get; set; } = PokemonTeam.Neutral;

        [
            JsonPropertyName("slots_available"),
            Column("availble_slots"), // TODO: Typflo
        ]
        public ushort SlotsAvailable { get; set; }

        [
            JsonPropertyName("sponsor_id"),
            Column("sponsor_id"),
        ]
        public uint? SponsorId { get; set; }

        [
            JsonPropertyName("in_battle"),
            Column("in_battle"),
        ]
        public bool InBattle { get; set; }

        #endregion

        public DiscordWebhookMessage GenerateEmbedMessage(AlarmMessageSettings settings)
        {
            var server = settings.Config.Instance.Servers[settings.GuildId];
            var embedType = EmbedMessageType.Gyms;
            var embed = settings.Alarm?.Embeds[embedType] ?? server.DmEmbeds?[embedType] ?? EmbedMessage.Defaults[embedType];
            var properties = GetProperties(settings);
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
                Color = (
                    Team == PokemonTeam.Mystic
                    ? DiscordColor.Blue
                    : Team == PokemonTeam.Valor
                        ? DiscordColor.Red
                        : Team == PokemonTeam.Instinct
                            ? DiscordColor.Yellow
                            : DiscordColor.LightGray
                    ).Value,
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

        private dynamic GetProperties(AlarmMessageSettings properties)
        {
            // Get old gym from cache
            var oldGym = properties.MapDataCache.GetGym(GymId).ConfigureAwait(false)
                                                .GetAwaiter()
                                                .GetResult();

            var exEmojiId = MasterFile.Instance.Emojis["ex"];
            var exEmoji = string.IsNullOrEmpty(MasterFile.Instance.CustomEmojis["ex"]) ? exEmojiId > 0
                ? string.Format(Strings.EmojiSchema, "ex", exEmojiId) : "EX"
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
            var scannerMapsLink = string.Format(properties.Config.Instance.Urls.ScannerMap, Latitude, Longitude);
            var gymImageUrl = IconFetcher.Instance.GetGymIcon(properties.Config.Instance.Servers[properties.GuildId].IconStyle, Team);// $"https://raw.githubusercontent.com/nileplumb/PkmnHomeIcons/ICONS/ICONS/gym/{Convert.ToInt32(Team)}.png"; // TODO: Build gym image url
            //var staticMapLink = StaticMap.GetUrl(properties.Config.Instance.Urls.StaticMap, properties.Config.Instance.StaticMaps["gyms"], Latitude, Longitude, gymImageUrl);
            var staticMap = new StaticMapGenerator(new StaticMapOptions
            {
                BaseUrl = properties.Config.Instance.StaticMaps[StaticMapType.Gyms].Url,
                TemplateName = properties.Config.Instance.StaticMaps[StaticMapType.Gyms].TemplateName,
                Latitude = Latitude,
                Longitude = Longitude,
                Team = Team,
                SecondaryImageUrl = gymImageUrl,
            });
            var staticMapLink = staticMap.GenerateLink();
            var gmapsLocationLink = UrlShortener.CreateShortUrl(properties.Config.Instance.ShortUrlApiUrl, gmapsLink);
            var appleMapsLocationLink = UrlShortener.CreateShortUrl(properties.Config.Instance.ShortUrlApiUrl, appleMapsLink);
            var wazeMapsLocationLink = UrlShortener.CreateShortUrl(properties.Config.Instance.ShortUrlApiUrl, wazeMapsLink);
            var scannerMapsLocationLink = UrlShortener.CreateShortUrl(properties.Config.Instance.ShortUrlApiUrl, scannerMapsLink);
            var address = new Coordinate(properties.City, Latitude, Longitude).GetAddress(properties.Config.Instance);
            //var staticMapLocationLink = string.IsNullOrEmpty(whConfig.ShortUrlApiUrl) ? staticMapLink : NetUtil.CreateShortUrl(whConfig.ShortUrlApiUrl, staticMapLink);
            var guild = properties.Client.Guilds.ContainsKey(properties.GuildId) ? properties.Client.Guilds[properties.GuildId] : null;

            const string defaultMissingValue = "?";
            var dict = new
            {
                // Main properties
                gym_id = GymId,
                gym_name = GymName,
                gym_url = Url,
                gym_team = Team.ToString(),
                gym_team_id = Convert.ToInt32(Team).ToString(),
                gym_team_emoji = teamEmoji,
                old_gym_team = oldGym.Team.ToString(),
                old_gym_team_id = Convert.ToInt32(oldGym.Team).ToString(),
                old_gym_team_emoji = oldTeamEmoji,
                team_changed = oldGym?.Team != Team,
                in_battle = InBattle,
                under_attack = InBattle,
                is_ex = Convert.ToString(SponsorId),
                ex_emoji = exEmoji,
                slots_available = SlotsAvailable == 0
                                        ? "Full"
                                        : SlotsAvailable == 6
                                            ? "Empty"
                                            : SlotsAvailable.ToString("N0"),

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

                address = address?.Address,

                // Discord Guild properties
                guild_name = guild?.Name,
                guild_img_url = guild?.IconUrl,

                // Misc properties
                date_time = DateTime.Now.ToString(),
                br = "\n",
            };
            return dict;
        }
    }
}