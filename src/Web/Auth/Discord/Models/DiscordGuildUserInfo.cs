﻿namespace WhMgr.Web.Auth.Discord.Models
{
    using System.Text.Json.Serialization;

    public class DiscordGuildUserInfo
    {
        [JsonPropertyName("id")]
        public ulong Id { get; set; }

        [JsonPropertyName("username")]
        public string Username { get; set; }

        [JsonPropertyName("discriminator")]
        public string Discriminator { get; set; }

        [JsonPropertyName("avatar")]
        public string Avatar { get; set; }

        [JsonPropertyName("bot")]
        public bool? IsBot { get; set; }

        [JsonPropertyName("system")]
        public bool? IsSystem { get; set; }

        [JsonPropertyName("mfa_enabled")]
        public bool? IsMfaEnabled { get; set; }

        [JsonPropertyName("banner")]
        public string Banner { get; set; }

        [JsonPropertyName("accent_color")]
        public int? AccentColor { get; set; }

        [JsonPropertyName("locale")]
        public string Locale { get; set; }

        [JsonPropertyName("is_verified")]
        public bool? IsVerified { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("flags")]
        public int? Flags { get; set; }

        [JsonPropertyName("premium_type")]
        public int? PremiumType { get; set; }

        [JsonPropertyName("public_flags")]
        public int? PublicFlags { get; set; }
    }
}