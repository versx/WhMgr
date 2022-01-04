namespace WhMgr.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    public class DiscordAuthConfig
    {
        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; }

        // Skips login process
        [JsonPropertyName("ownerId")]
        public ulong OwnerId { get; set; }

        [JsonPropertyName("clientId")]
        public ulong ClientId { get; set; }

        [JsonPropertyName("clientSecret")]
        public string ClientSecret { get; set; }

        [JsonPropertyName("redirectUri")]
        public string RedirectUri { get; set; }

        public IEnumerable<ulong> UserIds { get; set; }
    }
}