namespace WhMgr.Data.Models.Discord
{
    using Newtonsoft.Json;

    public class DiscordEmbedAuthor
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("icon_url")]
        public string IconUrl { get; set; }
    }
}