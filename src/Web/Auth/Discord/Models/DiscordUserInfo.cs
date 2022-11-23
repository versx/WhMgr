namespace WhMgr.Web.Auth.Discord.Models
{
    using System.Text.Json.Serialization;

    public class DiscordUserInfo
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } // Need to specify string instead of ulong >.>

        [JsonPropertyName("username")]
        public string Username { get; set; }

        [JsonPropertyName("avatar")]
        public string Avatar { get; set; }

        [JsonPropertyName("discriminator")]
        public string Discriminator { get; set; }

        [JsonPropertyName("public_flags")]
        public int PublicFlags { get; set; }

        [JsonPropertyName("flags")]
        public int Flags { get; set; }

        [JsonPropertyName("locale")]
        public string Locale { get; set; }

        [JsonPropertyName("mfa_enabled")]
        public bool MfaEnabled { get; set; }

        [JsonPropertyName("premium_type")]
        public int PremiumType { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("verified")]
        public bool Verified { get; set; }
    }
}