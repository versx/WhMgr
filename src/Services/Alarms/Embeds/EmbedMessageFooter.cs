namespace WhMgr.Services.Alarms.Embeds
{
    using System.Text.Json.Serialization;
    /// <summary>
    /// Discord alert message footer
    /// </summary>
    public class EmbedMessageFooter
    {
        /// <summary>
        /// Gets or sets the Discord message footer text
        /// </summary>
        [JsonPropertyName("text")]
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the Discord message footer icon url
        /// </summary>
        [JsonPropertyName("iconUrl")]
        public string IconUrl { get; set; }
    }
}