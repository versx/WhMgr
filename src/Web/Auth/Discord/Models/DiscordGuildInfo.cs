namespace WhMgr.Web.Auth.Discord.Models
{
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    internal class DiscordGuildInfo
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } // Need to specify string instead of ulong >.>

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("icon")]
        public string Icon { get; set; }

        [JsonPropertyName("owner")]
        public bool IsOwner { get; set; }

        [JsonPropertyName("permissions")]
        public int Permissions { get; set; }

        [JsonPropertyName("features")]
        public List<string> Features { get; set; }

        [JsonPropertyName("permissions_new")]
        public string PermissionsNew { get; set; }
    }
}