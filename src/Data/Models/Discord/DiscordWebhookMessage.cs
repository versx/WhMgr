namespace WhMgr.Data.Models.Discord
{
    using System;
    using System.Collections.Generic;

    using DSharpPlus.Entities;
    using Newtonsoft.Json;

    public class DiscordWebhookMessage
    {
        [JsonProperty("content")]
        public string Content { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("avatar_url")]
        public string AvatarUrl { get; set; }

        [JsonProperty("embeds")]
        public List<DiscordEmbed> Embeds { get; set; }

        [JsonIgnore]
        public bool HasEmbeds => Embeds?.Count > 0;

        public DiscordWebhookMessage()
        {
            Embeds = new List<DiscordEmbed>();
        }

        public string Build()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}