namespace WhMgr.Alarms.Models
{
    using Newtonsoft.Json;

    /*
     * {
     *  "name": "POGO",
     *  "channel_id": "497225469121396736",
     *  "token": "0xa6rGEwP5TYrFEDGMTUWBFdKALn2LAaJ0XP8ntlAipfPUjwke_fxeruaigsDy8YFwEU",
     *  "avatar": null,
     *  "guild_id": "342025055510855680",
     *  "id": "497227426607267854"
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