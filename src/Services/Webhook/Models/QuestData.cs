namespace WhMgr.Services.Webhook.Models
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    using DSharpPlus.Entities;
    using POGOProtos.Rpc;
    using ActivityType = POGOProtos.Rpc.HoloActivityType;
    using QuestConditionType = POGOProtos.Rpc.QuestConditionProto.Types.ConditionType;
    using QuestRewardType = POGOProtos.Rpc.QuestRewardProto.Types.Type;

    using WhMgr.Extensions;
    using WhMgr.Services.Alarms;
    using WhMgr.Services.Alarms.Embeds;
    using WhMgr.Services.Discord.Models;
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

        /// <summary>
        /// Generates a Discord embed message for a Pokestop Quest
        /// </summary>
        /// <param name="guildId">Discord Guild ID related to the data</param>
        /// <param name="client">Discord client to use</param>
        /// <param name="whConfig">Config to use</param>
        /// <param name="alarm">Alarm to use</param>
        /// <param name="city">City to specify</param>
        /// <returns></returns>
        public DiscordWebhookMessage GenerateEmbedMessage(AlarmMessageSettings settings)
        {
            var server = settings.Config.Instance.Servers[settings.GuildId];
            var embedType = EmbedMessageType.Quests;
            var embed = settings.Alarm?.Embeds[embedType] ?? server.DmEmbeds?[embedType] ?? EmbedMessage.Defaults[embedType];
            settings.ImageUrl = IconFetcher.Instance.GetQuestIcon(settings.Config.Instance.Servers[settings.GuildId].IconStyle, this);
            var properties = GetProperties(settings);
            var eb = new DiscordEmbedBuilder
            {
                Title = TemplateRenderer.Parse(embed.Title, properties),
                Url = TemplateRenderer.Parse(embed.Url, properties),
                ImageUrl = TemplateRenderer.Parse(embed.ImageUrl, properties),
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                {
                    Url = TemplateRenderer.Parse(embed.IconUrl, properties),
                },
                Description = TemplateRenderer.Parse(embed.Content, properties),
                // TODO: Color = new DiscordColor(MasterFile.Instance.DiscordEmbedColors.Pokestops.Quests),
                Footer = new DiscordEmbedBuilder.EmbedFooter
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
                Embeds = new List<DiscordEmbed> { eb.Build() }
            };
        }

        private dynamic GetProperties(AlarmMessageSettings properties)
        {
            var questMessage = this.GetQuestMessage();
            var questConditions = this.GetConditions();
            var questReward = this.GetReward();
            var gmapsLink = string.Format(Strings.GoogleMaps, Latitude, Longitude);
            var appleMapsLink = string.Format(Strings.AppleMaps, Latitude, Longitude);
            var wazeMapsLink = string.Format(Strings.WazeMaps, Latitude, Longitude);
            var scannerMapsLink = string.Format(properties.Config.Instance.Urls.ScannerMap, Latitude, Longitude);
            var staticMapLink = StaticMap.GetUrl(properties.Config.Instance.Urls.StaticMap, properties.Config.Instance.StaticMaps["quests"], Latitude, Longitude, properties.ImageUrl);
            var gmapsLocationLink = UrlShortener.CreateShortUrl(properties.Config.Instance.ShortUrlApiUrl, gmapsLink);
            var appleMapsLocationLink = UrlShortener.CreateShortUrl(properties.Config.Instance.ShortUrlApiUrl, appleMapsLink);
            var wazeMapsLocationLink = UrlShortener.CreateShortUrl(properties.Config.Instance.ShortUrlApiUrl, wazeMapsLink);
            var scannerMapsLocationLink = UrlShortener.CreateShortUrl(properties.Config.Instance.ShortUrlApiUrl, scannerMapsLink);
            // TODO: var address = new Coordinate(city, Latitude, Longitude).GetAddress(whConfig);
            //var staticMapLocationLink = string.IsNullOrEmpty(whConfig.ShortUrlApiUrl) ? staticMapLink : NetUtil.CreateShortUrl(whConfig.ShortUrlApiUrl, staticMapLink);

            const string defaultMissingValue = "?";
            var dict = new
            {
                //Main properties
                quest_task = questMessage,
                quest_conditions = questConditions,
                quest_reward = questReward,
                quest_reward_img_url = properties.ImageUrl,
                has_quest_conditions = Convert.ToString(!string.IsNullOrEmpty(questConditions)),
                is_ditto = Convert.ToString(IsDitto),
                is_shiny = Convert.ToString(IsShiny),

                //Location properties
                geofence = properties.City ?? defaultMissingValue,
                lat = Latitude.ToString(),
                lng = Longitude.ToString(),
                lat_5 = Latitude.ToString("0.00000"),
                lng_5 = Longitude.ToString("0.00000"),

                //Location links
                tilemaps_url = staticMapLink,
                gmaps_url = gmapsLocationLink,
                applemaps_url = appleMapsLocationLink,
                wazemaps_url = wazeMapsLocationLink,
                scanmaps_url = scannerMapsLocationLink,

                //{ "address", address?.Address },

                //Pokestop properties
                pokestop_id = PokestopId ?? defaultMissingValue,
                pokestop_name = PokestopName ?? defaultMissingValue,
                pokestop_url = PokestopUrl ?? defaultMissingValue,

                // Discord Guild properties
                guild_name = "", // TODO: guild?.Name },
                guild_img_url = "", // TODO: guild?.IconUrl },

                //Misc properties
                date_time = DateTime.Now.ToString(),
                br = "\r\n",
            };
            return dict;
        }
    }

    public sealed class QuestConditionMessage
    {
        [JsonPropertyName("type")]
        public QuestConditionType Type { get; set; }

        [JsonPropertyName("info")]
        public QuestCondition Info { get; set; }

        public QuestConditionMessage()
        {
            Type = QuestConditionType.Unset;
        }
    }

    public sealed class QuestCondition
    {
        [JsonPropertyName("pokemon_ids")]
        public List<uint> PokemonIds { get; set; }

        [JsonPropertyName("category_name")]
        public string CategoryName { get; set; }

        [JsonPropertyName("pokemon_type_ids")]
        public List<int> PokemonTypeIds { get; set; }

        [JsonPropertyName("throw_type_id")]
        public ActivityType ThrowTypeId { get; set; }

        [JsonPropertyName("hit")]
        public bool Hit { get; set; }

        [JsonPropertyName("raid_levels")]
        public List<int> RaidLevels { get; set; }

        [JsonPropertyName("alignment_ids")]
        public List<int> AlignmentIds { get; set; }

        [JsonPropertyName("character_category_ids")]
        public List<int> CharacterCategoryIds { get; set; }

        [JsonPropertyName("raid_pokemon_evolutions")]
        public List<int> RaidPokemonEvolutions { get; set; }

        public QuestCondition()
        {
            ThrowTypeId = ActivityType.ActivityUnknown;
        }
    }

    public sealed class QuestRewardMessage
    {
        [JsonPropertyName("type")]
        public QuestRewardType Type { get; set; }

        [JsonPropertyName("info")]
        public QuestReward Info { get; set; }

        public QuestRewardMessage()
        {
            Type = QuestRewardType.Unset;
        }
    }

    public sealed class QuestReward
    {
        [JsonPropertyName("pokemon_id")]
        public uint PokemonId { get; set; }

        [JsonPropertyName("costume_id")]
        public int CostumeId { get; set; }

        [JsonPropertyName("form_id")]
        public int FormId { get; set; }

        [JsonPropertyName("gender_id")]
        public int GenderId { get; set; }

        [JsonPropertyName("ditto")]
        public bool Ditto { get; set; }

        [JsonPropertyName("shiny")]
        public bool Shiny { get; set; }

        [JsonPropertyName("amount")]
        public int Amount { get; set; }

        [JsonPropertyName("item_id")]
        public Item Item { get; set; }

        [JsonPropertyName("raid_levels")]
        public List<int> RaidLevels { get; set; }

        [JsonPropertyName("mega_resource")]
        public QuestMegaResource MegaResource { get; set; }

        [JsonPropertyName("sticker_id")]
        public string StickerId { get; set; }

        // TODO: Pokemon alignment
    }

    public sealed class QuestMegaResource
    {
        public ushort PokemonId { get; set; }

        public int Amount { get; set; }
    }
}