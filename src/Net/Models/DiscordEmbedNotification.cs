namespace WhMgr.Net.Models
{
    using System;
    using System.Collections.Generic;

    using DSharpPlus.Entities;

    public class DiscordEmbedNotification
    {
        public string Username { get; set; }

        public string IconUrl { get; set; }

        public string Description { get; set; }

        public List<DiscordEmbed> Embeds { get; set; }

        public DiscordEmbedNotification(string username, string iconUrl, string description, List<DiscordEmbed> embeds)
        {
            Username = username;
            IconUrl = iconUrl;
            Description = description;
            Embeds = embeds;
        }
    }
}