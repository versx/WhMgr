namespace WhMgr.Extensions
{
    using DSharpPlus.Entities;

    using WhMgr.Services.Discord.Models;

    public static class DiscordWebhookMessageExtensions
    {
        // Because DSharpPlus v4.x latest DiscordEmbed isn't compatible with raw webhooks
        public static DiscordEmbed GenerateDiscordMessage(this DiscordEmbedMessage embed)
        {
            var eb = new DiscordEmbedBuilder
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor
                {
                    Name = embed.Author?.Name,
                    IconUrl = embed.Author?.IconUrl,
                },
                Description = embed.Description,
                //Fields = embed.Fields,
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = embed.Footer?.Text,
                    IconUrl = embed.Footer?.IconUrl,
                },
                ImageUrl = embed.Image?.Url,
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                {
                    Url = embed.Thumbnail?.Url,
                },
                Title = embed.Title,
                Url = embed.Url,
            };
            return eb.Build();
        }
    }
}