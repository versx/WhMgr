namespace WhMgr.Alarms.Models
{
    using Newtonsoft.Json;

    /*
     * {
     *  "name": "",
     *  "channel_id": "",
     *  "token": "",
     *  "avatar": null,
     *  "guild_id": "",
     *  "id": ""
     * }
     */
    public class WebHookObject
    {
        [JsonProperty("id")]
        public ulong Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("guild_id")]
        public ulong GuildId { get; set; }

        [JsonProperty("channel_id")]
        public ulong ChannelId { get; set; }

        [JsonProperty("token")]
        public string Token { get; set; }

        [JsonProperty("avatar")]
        public string Avatar { get; set; }
    }
}