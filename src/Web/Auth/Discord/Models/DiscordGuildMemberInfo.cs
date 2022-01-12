namespace WhMgr.Web.Auth.Discord.Models
{
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    public class DiscordGuildMemberInfo
    {
        [JsonPropertyName("user")]
        public DiscordGuildUserInfo User { get; set; }

        [JsonPropertyName("nick")]
        public string Nickname { get; set; }

        [JsonPropertyName("avatar")]
        public string AvatarHash { get; set; }

        [JsonPropertyName("roles")]
        public List<ulong> Roles { get; set; }

        [JsonPropertyName("joined_at")]
        public long JoinedAtTimestamp { get; set; }

        [JsonPropertyName("premium_since")]
        public long? PremiumSinceTimestamp { get; set; }

        [JsonPropertyName("deaf")]
        public bool Deaf { get; set; }

        [JsonPropertyName("mute")]
        public bool Mute { get; set; }

        [JsonPropertyName("pending")]
        public bool? IsPending { get; set; }

        [JsonPropertyName("permissions")]
        public string Permissions { get; set; }

        [JsonPropertyName("communication_disabled_until")]
        public long? CommunicationDisabledUntil { get; set; }
    }
}