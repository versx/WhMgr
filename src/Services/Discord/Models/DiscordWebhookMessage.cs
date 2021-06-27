namespace WhMgr.Services.Discord.Models
{
    using DSharpPlus.Entities;
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    using WhMgr.Extensions;

    public class DiscordWebhookMessage
    {
        [JsonPropertyName("content")]
        public string Content { get; set; }

        [JsonPropertyName("username")]
        public string Username { get; set; }

        [JsonPropertyName("avatar_url")]
        public string AvatarUrl { get; set; }

        [JsonPropertyName("embeds")]
        public List<DiscordEmbed> Embeds { get; set; }

        [JsonIgnore]
        public bool HasEmbeds => Embeds?.Count > 0;

        public DiscordWebhookMessage()
        {
            Embeds = new List<DiscordEmbed>();
        }

        public string Build()
        {
            return this.ToJson();
        }
    }
}