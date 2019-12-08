namespace WhMgr.Commands
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    using DSharpPlus;
    using DSharpPlus.CommandsNext;
    using DSharpPlus.CommandsNext.Attributes;
    using DSharpPlus.Entities;

    using WhMgr.Data.Subscriptions;
    using WhMgr.Diagnostics;
    using WhMgr.Extensions;
    using WhMgr.Utilities;

    [
        RequireOwner,
        Hidden
    ]
    public class Owner
    {
        const string PokemonTrainerClub = "https://sso.pokemon.com/sso/login";
        const string NianticLabs = "https://pgorelease.nianticlabs.com/plfe/version";

        private static readonly IEventLogger _logger = EventLogger.GetLogger("OWNER");
        private readonly Dependencies _dep;

        public Owner(Dependencies dep)
        {
            _dep = dep;
        }

        [
            Command("isbanned"),
            Description("Check if IP banned from NianticLabs or Pokemon Trainer Club.")
        ]
        public async Task IsIPBannedAsync(CommandContext ctx)
        {
            var isPtcBanned = NetUtil.IsUrlBlocked(PokemonTrainerClub);
            var isNiaBanned = NetUtil.IsUrlBlocked(NianticLabs);
            var eb = new DiscordEmbedBuilder
            {
                Title = "Banned Status",
                Color = (isPtcBanned || isNiaBanned) ? DiscordColor.Red : DiscordColor.Green,
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    IconUrl = ctx.Guild?.IconUrl,
                    Text = $"versx | {DateTime.Now}"
                }
            };
            eb.AddField("Pokemon.com", isPtcBanned ? "Banned" : "Good", true);
            eb.AddField("NianticLabs.com", isNiaBanned ? "Banned" : "Good", true);
            await ctx.RespondAsync(string.Empty, false, eb.Build());
        }

        [
            Command("clean-departed"),
            Description("")
        ]
        public async Task CleanDepartedAsync(CommandContext ctx)
        {
            _logger.Debug($"Checking if there are any subscriptions for members that are no longer apart of the server...");

            var removed = 0;
            var users = _dep.SubscriptionProcessor?.Manager?.Subscriptions;// GetUserSubscriptions();
            for (var i = 0; i < users.Count; i++)
            {
                var user = users[i];
                var discordUser = ctx.Client.GetMemberById(ctx.Guild.Id, user.UserId);
                var isSupporter = ctx.Client.HasSupporterRole(ctx.Guild.Id, user.UserId, _dep.WhConfig.Servers[ctx.Guild.Id].DonorRoleIds);
                if (discordUser == null || !isSupporter)
                {
                    _logger.Debug($"Removing user {user.UserId} subscription settings because they are no longer a member of the server.");
                    if (!SubscriptionManager.RemoveAllUserSubscriptions(ctx.Guild.Id, user.UserId))
                    {
                        _logger.Warn($"Could not remove user {user.UserId} subscription settings from the database.");
                        continue;
                    }

                    _logger.Info($"Removed {user.UserId} and subscriptions from database.");
                    removed++;
                }
            }

            await ctx.RespondEmbed($"Removed {removed.ToString("N0")} of {users.Count.ToString("N0")} total members.");
        }

        [
            Command("sudo"), 
            Description("Executes a command as another user.")
        ]
        public async Task Sudo(CommandContext ctx, 
            [Description("Member to execute as.")] DiscordMember member, 
            [Description("Command text to execute."), RemainingText] string command)
        {
            await ctx.TriggerTypingAsync();

            // get the command service, we need this for sudo purposes
            var cmds = ctx.CommandsNext;
            await cmds.SudoAsync(member, ctx.Channel, command);
        }

        [
            Command("test-emoji"),
            Description("")
        ]
        public async Task TestAsync(CommandContext ctx,
            [Description("")] string emojiName)
        {
            var title = "Emoji Test";
            var emoji = emojiName.GetEmoji();
            var eb = new DiscordEmbedBuilder
            {
                Title = title,
                Description = $"{emoji}"
            };
            await ctx.RespondAsync(string.Empty, false, eb);
        }

        [
            Command("save-emojis"),
            Description("")
        ]
        public async Task SaveEmojisAsync(CommandContext ctx,
            [Description("")] ulong guildId)
        {
            await SaveEmojis(ctx.Client, guildId);
            await ctx.RespondEmbed("Emojis saved.", DiscordColor.Green);
        }


        private async Task SaveEmojis(DiscordClient client, ulong emojiGuildId)
        {
            if (!client.Guilds.ContainsKey(emojiGuildId))
            {
                _logger.Error($"Bot not in emoji guild {emojiGuildId}.");
                return;
            }

            var dict = new Dictionary<string, ulong>();
            var guild = client.Guilds[emojiGuildId];
            var emojis = await guild.GetEmojisAsync();
            for (var i = 0; i < Strings.EmojiList.Length; i++)
            {
                try
                {
                    var emojiName = Strings.EmojiList[i];
                    var emoji = emojis.FirstOrDefault(x => string.Compare(x.Name, emojiName, true) == 0);
                    if (emoji == null)
                        continue;

                    if (!dict.ContainsKey(emoji.Name))
                    {
                        dict.Add(emoji.Name, emoji.Id);
                        continue;
                    }

                    _logger.Error($"Emoji {emoji.Name} ({emoji.Id}) from guild {guild.Name} ({guild.Id}) already exists in emoji dictionary.");
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                }
            }

            var data = Newtonsoft.Json.JsonConvert.SerializeObject(dict, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText("emojis.json", data);
        }
    }
}
/*
       headers={'Host': 'sso.pokemon.com',
                 'Connection': 'close',
                 'Accept': '/*',
                 'User-Agent': 'pokemongo/0 CFNetwork/893.14.2 Darwin/17.3.0',
                 'Accept-Language': 'en-us',
                 'Accept-Encoding': 'br, gzip, deflate',
                 'X-Unity-Version': '2017.1.2f1'},
        background_callback=__proxy_check_completed,
 */