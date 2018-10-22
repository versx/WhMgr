namespace WhMgr.Extensions
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using DSharpPlus;
    using DSharpPlus.Entities;

    using WhMgr.Diagnostics;

    public static class DiscordExtensions
    {
        private static readonly IEventLogger _logger = EventLogger.GetLogger();

        public static async Task<DiscordMessage> SendDirectMessage(this DiscordClient client, DiscordUser user, DiscordEmbed embed)
        {
            if (embed == null)
                return null;

            try
            {
                var dm = await client.CreateDmAsync(user);
                if (dm != null)
                {
                    var msg = await dm.SendMessageAsync(string.Empty, false, embed);
                    return msg;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }

            return null;
        }

        public static async Task<DiscordMember> GetMemberById(this DiscordClient client, ulong guildId, ulong id)
        {
            var guild = await client.GetGuildAsync(guildId);
            if (guild == null)
            {
                _logger.Error($"Failed to get guild from id {guildId}.");
                return null;
            }

            var member = guild?.Members?.FirstOrDefault(x => x.Id == id);
            if (member == null)
            {
                _logger.Error($"Failed to get member from id {id}.");
                return null;
            }

            return member;
        }
    }
}