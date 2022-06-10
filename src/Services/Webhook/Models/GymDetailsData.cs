namespace WhMgr.Services.Webhook.Models
{
    using DSharpPlus.Entities;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Text.Json.Serialization;
    using System.Threading.Tasks;

    using WhMgr.Common;
    using WhMgr.Data;
    using WhMgr.Localization;
    using WhMgr.Services.Alarms;
    using WhMgr.Services.Alarms.Embeds;
    using WhMgr.Services.Discord.Models;
    using WhMgr.Services.Geofence;
    using WhMgr.Services.Geofence.Geocoding;
    using WhMgr.Services.Icons;
    using WhMgr.Utilities;

    [Table("gym")]
    public sealed class GymDetailsData : IWebhookData, IWebhookPoint
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
            JsonPropertyName("ex_raid_eligible"),
            Column("ex_raid_eligible"),
        ]
        public bool IsExEligible { get; set; }

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

        [
            JsonPropertyName("ar_scan_eligible"),
            Column("ar_scan_eligible"),
        ]
        public bool? IsArScanEligible { get; set; }

        #endregion

        public void SetTimes()
        {
            // No times to change
        }

        public async Task<DiscordWebhookMessage> GenerateEmbedMessageAsync(AlarmMessageSettings settings)
        {
            var server = settings.Config.Instance.Servers[settings.GuildId];
            var embedType = EmbedMessageType.Gyms;
            var embed = settings.Alarm?.Embeds[embedType] ?? server.Subscriptions?.Embeds?[embedType] ?? EmbedMessage.Defaults[embedType];
            var properties = await GetPropertiesAsync(settings).ConfigureAwait(false);
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

        private async Task<dynamic> GetPropertiesAsync(AlarmMessageSettings properties)
        {
            // Get old gym from cache
            var oldGym = await properties.MapDataCache.GetGym(GymId).ConfigureAwait(false);
            var exEmojiId = GameMaster.Instance.Emojis.ContainsKey("ex")
                ? GameMaster.Instance.Emojis["ex"]
                : 0;
            var exEmoji = string.IsNullOrEmpty(GameMaster.Instance.CustomEmojis["ex"]) ? exEmojiId > 0
                ? string.Format(Strings.EmojiSchema, "ex", exEmojiId) : "EX"
                : GameMaster.Instance.CustomEmojis["ex"];
            var teamEmojiId = GameMaster.Instance.Emojis.ContainsKey(Team.ToString().ToLower())
                ? GameMaster.Instance.Emojis[Team.ToString().ToLower()]
                : 0;
            var teamEmoji = string.IsNullOrEmpty(GameMaster.Instance.CustomEmojis[Team.ToString().ToLower()])
                ? teamEmojiId > 0
                    ? string.Format(Strings.EmojiSchema, Team.ToString().ToLower(), teamEmojiId)
                    : Team.ToString()
                : GameMaster.Instance.CustomEmojis[Team.ToString().ToLower()];
            var oldTeamEmojiId = GameMaster.Instance.Emojis.ContainsKey(oldGym.Team.ToString().ToLower())
                ? GameMaster.Instance.Emojis[oldGym?.Team.ToString().ToLower()]
                : 0;
            var oldTeamEmoji = string.IsNullOrEmpty(GameMaster.Instance.CustomEmojis[oldGym?.Team.ToString().ToLower()])
                ? oldTeamEmojiId > 0
                    ? string.Format(Strings.EmojiSchema, oldGym?.Team.ToString().ToLower(), oldTeamEmojiId)
                    : oldGym?.Team.ToString()
                : GameMaster.Instance.CustomEmojis[oldGym.Team.ToString().ToLower()];

            var gmapsLink = string.Format(Strings.Defaults.GoogleMaps, Latitude, Longitude);
            var appleMapsLink = string.Format(Strings.Defaults.AppleMaps, Latitude, Longitude);
            var wazeMapsLink = string.Format(Strings.Defaults.WazeMaps, Latitude, Longitude);
            var scannerMapsLink = string.Format(properties.Config.Instance.Urls.ScannerMap, Latitude, Longitude);
            var gymImageUrl = UIconService.Instance.GetGymIcon(properties.Config.Instance.Servers[properties.GuildId].IconStyle, Team);// $"https://raw.githubusercontent.com/nileplumb/PkmnHomeIcons/ICONS/ICONS/gym/{Convert.ToInt32(Team)}.png"; // TODO: Build gym image url

            var staticMapConfig = properties.Config.Instance.StaticMaps[StaticMapType.Gyms];
            var staticMap = new StaticMapGenerator(new StaticMapOptions
            {
                BaseUrl = staticMapConfig.Url,
                TemplateName = staticMapConfig.TemplateName,
                Latitude = Latitude,
                Longitude = Longitude,
                Team = Team,
                SecondaryImageUrl = gymImageUrl,
                Gyms = staticMapConfig.IncludeNearbyGyms
                    // Fetch nearby gyms from MapDataCache
                    ? await properties.MapDataCache?.GetGymsNearby(Latitude, Longitude)
                    : new(),
                Pokestops = staticMapConfig.IncludeNearbyPokestops
                    // Fetch nearby pokestops from MapDataCache
                    ? await properties.MapDataCache?.GetPokestopsNearby(Latitude, Longitude)
                    : new(),
            });
            var staticMapLink = staticMap.GenerateLink();
            var urlShortener = new UrlShortener(properties.Config.Instance.ShortUrlApi);
            var gmapsLocationLink = await urlShortener.CreateAsync(gmapsLink);
            var appleMapsLocationLink = await urlShortener.CreateAsync( appleMapsLink);
            var wazeMapsLocationLink = await urlShortener.CreateAsync( wazeMapsLink);
            var scannerMapsLocationLink = await urlShortener.CreateAsync( scannerMapsLink);
            var address = await ReverseGeocodingLookup.Instance.GetAddressAsync(new Coordinate(Latitude, Longitude));
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
                is_ex = IsExEligible,
                sponsor_id = Convert.ToString(SponsorId),
                ex_emoji = exEmoji,
                slots_available = SlotsAvailable == 0
                    ? Translator.Instance.Translate("FULL")
                    : SlotsAvailable == 6
                        ? Translator.Instance.Translate("Empty")
                        : SlotsAvailable.ToString("N0"),
                is_ar = IsArScanEligible ?? false,

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