namespace WhMgr.Services.Discord.Models
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    using DSharpPlus.Entities;

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
            try
            {
                var json = this.ToJson();
                return json;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex}");
                return null;
            }
        }
    }
}