namespace WhMgr.Services.Webhook.Models
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json.Serialization;
    using System.Threading.Tasks;

    using WhMgr.Data;
    using WhMgr.Extensions;
    using WhMgr.Services.Alarms;
    using WhMgr.Services.Alarms.Embeds;
    using WhMgr.Services.Discord.Models;

    public sealed class AccountData : IWebhookData
    {
        #region Properties

        [JsonPropertyName("username")]
        public string Username { get; set; }

        [JsonPropertyName("failed")]
        public string Failed { get; set; }

        [JsonPropertyName("failed_timestamp")]
        public ulong FailedTimestamp { get; set; }

        [JsonIgnore]
        public DateTime FailedTime { get; set; }

        [JsonPropertyName("first_warning_timestamp")]
        public ulong FirstWarningTimestamp { get; set; }

        [JsonIgnore]
        public DateTime FirstWarningTime { get; set; }

        [JsonPropertyName("suspended_message_acknowledged")]
        public bool SuspendedMessageAcknowledged { get; set; }

        [JsonPropertyName("was_suspended")]
        public bool WasSuspended { get; set; }

        [JsonPropertyName("warn_expire_timestamp")]
        public ulong WarningExpireTimestamp { get; set; }

        [JsonIgnore]
        public DateTime WarningExpireTime { get; set; }

        [JsonPropertyName("warn_message_acknowledged")]
        public bool WarningMessageAcknowledged { get; set; }

        [JsonPropertyName("warn")]
        public bool IsWarned { get; set; }

        [JsonPropertyName("banned")]
        public bool IsBanned { get; set; }

        [JsonPropertyName("last_encounter_time")]
        public ulong LastEncounterTimestamp { get; set; }

        [JsonIgnore]
        public DateTime LastEncounterTime { get; set; }

        [JsonPropertyName("creation_timestamp")]
        public ulong CreationTimestamp { get; set; }

        [JsonIgnore]
        public DateTime CreationTime { get; set; }

        [JsonPropertyName("group")]
        public string Group { get; set; }

        [JsonPropertyName("level")]
        public ushort Level { get; set; }

        [JsonPropertyName("spins")]
        public uint Spins { get; set; }

        #endregion

        public AccountData()
        {
            SetTimes();
        }

        public void SetTimes()
        {
            // TODO: Convert to time zone
            FailedTime = FailedTimestamp.FromUnix();
            FirstWarningTime = FirstWarningTimestamp.FromUnix();
            WarningExpireTime = WarningExpireTimestamp.FromUnix();
            LastEncounterTime = LastEncounterTimestamp.FromUnix();
            CreationTime = CreationTimestamp.FromUnix();
        }

        public async Task<DiscordWebhookMessage> GenerateEmbedMessageAsync(AlarmMessageSettings settings)
        {
            var embed = settings.Alarm?.Embeds[EmbedMessageType.Account]
                ?? EmbedMessage.Defaults[EmbedMessageType.Account];
            settings.ImageUrl = ""; // TODO: Account image
            var properties = GetPropertiesAsync(settings);
            var eb = new DiscordEmbedMessage
            {
                Title = TemplateRenderer.Parse(embed.Title, properties),
                Url = TemplateRenderer.Parse(embed.Url, properties),
                Image = new DiscordEmbedImage
                {
                    Url = TemplateRenderer.Parse(embed.ImageUrl, properties),
                },
                Thumbnail = new DiscordEmbedImage
                {
                    Url = TemplateRenderer.Parse(embed.IconUrl, properties),
                },
                Description = TemplateRenderer.Parse(embed.Content, properties),
                Color = Level.BuildRaidColor(GameMaster.Instance.DiscordEmbedColors).Value,
                Footer = new DiscordEmbedFooter
                {
                    Text = TemplateRenderer.Parse(embed.Footer?.Text, properties),
                    IconUrl = TemplateRenderer.Parse(embed.Footer?.IconUrl, properties)
                },
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

        private dynamic GetPropertiesAsync(AlarmMessageSettings properties)
        {
            var guild = properties.Client.Guilds.ContainsKey(properties.GuildId) ? properties.Client.Guilds[properties.GuildId] : null;

            var dict = new
            {
                username = Username,
                is_banned = IsBanned,
                is_warned = IsWarned,
                failed = Failed,
                failed_time = FailedTime.ToLongTimeString(),
                spins = Spins,
                level = Level,
                group = Group,

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