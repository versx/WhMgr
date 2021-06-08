namespace WhMgr.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    public class BotConfig
    {
        /// <summary>
        /// Gets or sets the command prefix for all Discord commands
        /// </summary>
        [JsonPropertyName("commandPrefix")]
        public string CommandPrefix { get; set; }

        /// <summary>
        /// Gets or sets the emoji guild id
        /// </summary>
        [JsonPropertyName("emojiGuildId")]
        public ulong EmojiGuildId { get; set; }

        /// <summary>
        /// Gets or sets the Discord bot token
        /// </summary>
        [JsonPropertyName("token")]
        public string Token { get; set; }

        /// <summary>
        /// Gets or sets the bot channel ID(s)
        /// </summary>
        [
            Obsolete("Not used"),
            JsonPropertyName("channelIds"),
        ]
        public List<ulong> ChannelIds { get; set; } = new();

        /// <summary>
        /// Gets or sets the Discord bot's custom status
        /// </summary>
        [JsonPropertyName("status")]
        public string Status { get; set; }
    }
}