namespace WhMgr.Extensions
{
    using System;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    using DSharpPlus;
    using DSharpPlus.CommandsNext;
    using DSharpPlus.Entities;
    using DSharpPlus.Interactivity;

    using WhMgr.Configuration;
    using WhMgr.Diagnostics;

    public static class DiscordExtensions
    {
        private const string ConfirmRegex = "\\b[Yy][Ee]?[Ss]?\\b|\\b[Nn][Oo]?\\b";
        private const string YesRegex = "[Yy][Ee]?[Ss]?";
        private const string NoRegex = "[Nn][Oo]?";

        private static readonly IEventLogger _logger = EventLogger.GetLogger();

        public static async Task<DiscordMessage> RespondEmbed(this CommandContext ctx, string message)
        {
            return await RespondEmbed(ctx, message, DiscordColor.Green);
        }

        public static async Task<DiscordMessage> RespondEmbed(this CommandContext ctx, string message, DiscordColor color)
        {
            var eb = new DiscordEmbedBuilder
            {
                Color = color,
                Description = message
            };

            await ctx.TriggerTypingAsync();
            return await ctx.RespondAsync(string.Empty, false, eb);
        }

        public static async Task<DiscordMessage> SendDirectMessage(this DiscordClient client, DiscordUser user, DiscordEmbed embed)
        {
            if (embed == null)
                return null;

            return await client.SendDirectMessage(user, string.Empty, embed);
        }

        public static async Task<DiscordMessage> SendDirectMessage(this DiscordClient client, DiscordUser user, string message, DiscordEmbed embed)
        {
            try
            {
                var dm = await client.CreateDmAsync(user);
                if (dm != null)
                {
                    var msg = await dm.SendMessageAsync(message, false, embed);
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
            if (!client.Guilds.ContainsKey(guildId))
                return null;

            var guild = client.Guilds[guildId];
            if (guild == null)
                return null;

            var member = guild?.Members?.FirstOrDefault(x => x.Id == id);
            if (member == null)
            {
                try
                {
                    member = await guild.GetMemberAsync(id);
                }
                catch
                {
                    return null;
                }
            }

            return member;
        }

        public static async Task<DiscordMessage> DonateUnlockFeaturesMessage(this CommandContext ctx, bool triggerTyping = true)
        {
            if (triggerTyping)
            {
                await ctx.TriggerTypingAsync();
            }
            return await ctx.RespondEmbed($"{ctx.User.Username} This feature is only available to supporters, please donate to unlock this feature and more. Donation information can be found by typing the `.donate` command.");
        }

        internal static async Task<bool> IsDirectMessageSupported(this DiscordMessage message)
        {
            if (message.Channel.Guild == null)
            {
                await message.RespondAsync($"{message.Author.Mention} DM is not supported for this command yet.");
                return false;
            }

            return true;
        }

        public static bool IsSupporterOrHigher(this DiscordClient client, ulong userId, WhConfig config)
        {
            try
            {
                var isAdmin = userId == config.OwnerId;
                if (isAdmin)
                    return true;

                var isModerator = config.Moderators.Contains(userId);
                if (isModerator)
                    return true;

                var isSupporter = client.HasSupporterRole(config.GuildId, userId, config.SupporterRoleId);
                if (isSupporter)
                    return true;

                //TODO: Check TeamElite role.
                //var isElite = await client.HasSupporterRole(userId, config.TeamEliteRoleId);
                //if (isElite)
                    //return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }

            return false;
        }

        public static bool IsModeratorOrHigher(this ulong userId, WhConfig config)
        {
            var isAdmin = IsAdmin(userId, config.OwnerId);
            if (isAdmin)
                return true;

            var isModerator = config.Moderators.Contains(userId);
            if (isModerator)
                return true;

            return false;
        }

        public static bool IsModerator(this ulong userId, WhConfig config)
        {
            return config.Moderators.Contains(userId);
        }

        public static bool IsAdmin(this ulong userId, ulong ownerId)
        {
            return userId == ownerId;
        }

        public static bool HasSupporterRole(this DiscordClient client, ulong guildId, ulong userId, ulong supporterRoleId)
        {
            if (!client.Guilds.ContainsKey(guildId))
                return false;

            var guild = client.Guilds[guildId];
            var member = guild.Members.FirstOrDefault(x => x.Id == userId);
            if (member == null)
            {
                _logger.Error($"Failed to get user with id {userId}.");
                return false;
            }

            return member.HasSupporterRole(supporterRoleId);
        }

        public static bool HasSupporterRole(this DiscordMember member, ulong supporterRoleId)
        {
            return HasRole(member, supporterRoleId);
        }

        public static async Task<bool> HasModeratorRole(this DiscordClient client, ulong guildId, ulong userId, ulong moderatorRoleId)
        {
            var member = await client.GetMemberById(guildId, userId);
            if (member == null)
            {
                _logger.Error($"Failed to get moderator user with id {userId}.");
                return false;
            }

            return member.HasModeratorRole(moderatorRoleId);
        }

        public static bool HasModeratorRole(this DiscordMember member, ulong moderatorRoleId)
        {
            return HasRole(member, moderatorRoleId);
        }

        public static bool HasRole(this DiscordMember member, ulong roleId)
        {
            try
            {
                var role = member?.Roles.FirstOrDefault(x => x.Id == roleId);
                return role != null;
            }
            catch
            {
                return false;
            }
        }

        public static bool HasRole(this DiscordClient client, DiscordMember member, string roleName)
        {
            var role = client.GetRoleFromName(roleName);
            if (role == null) return false;

            return HasRole(member, role.Id);
        }

        public static DiscordRole GetRoleFromName(this DiscordClient client, string roleName)
        {
            foreach (var guild in client.Guilds)
            {
                var role = guild.Value.Roles.FirstOrDefault(x => string.Compare(x.Name, roleName, true) == 0);
                if (role != null)
                    return role;
            }

            return null;
        }

        public static ulong? GetEmojiId(this DiscordGuild guild, string emojiName)
        {
            return guild.Emojis.FirstOrDefault(x => string.Compare(x.Name, emojiName, true) == 0)?.Id;
        }

        public static async Task<bool> Confirm(this CommandContext ctx, string message)
        {
            await ctx.RespondEmbed(message);
            var interactivity = ctx.Client.GetModule<InteractivityModule>();
            if (interactivity == null)
            {
                _logger.Error("Interactivity model failed to load!");
                return false;
            }

            var m = await interactivity.WaitForMessageAsync(
                x => x.Channel.Id == ctx.Channel.Id
                && x.Author.Id == ctx.User.Id
                && Regex.IsMatch(x.Content, ConfirmRegex), 
                TimeSpan.FromMinutes(2));

            return Regex.IsMatch(m.Message.Content, YesRegex);
        }

        public static DiscordColor BuildColor(this string iv)
        {
            if (double.TryParse(iv.Substring(0, iv.Length - 1), out var result))
            {
                if (Math.Abs(result - 100) < double.Epsilon)
                    return DiscordColor.Green;
                else if (result >= 90 && result < 100)
                    return DiscordColor.Orange;
                else if (result < 90)
                    return DiscordColor.Yellow;
            }

            return DiscordColor.White;
        }

        public static DiscordColor BuildRaidColor(this int level)
        {
            switch (level)
            {
                case 1:
                    return DiscordColor.HotPink;
                case 2:
                    return DiscordColor.HotPink;
                case 3:
                    return DiscordColor.Yellow;
                case 4:
                    return DiscordColor.Yellow;
                case 5:
                    return DiscordColor.Purple;
            }

            return DiscordColor.White;
        }
    }
}