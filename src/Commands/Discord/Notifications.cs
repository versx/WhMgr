namespace WhMgr.Commands.Discord
{
    using System;
    using System.Threading.Tasks;

    using DSharpPlus.CommandsNext;
    using DSharpPlus.CommandsNext.Attributes;
    using DSharpPlus.Entities;

    using WhMgr.Configuration;

    public class Notifications : BaseCommandModule
    {
        private readonly ConfigHolder _config;

        public Notifications(ConfigHolder config)
        {
            _config = config;
        }

        // TODO: Add support for info and enable/disable commands
        [Command("info")]
        public async Task InfoAsync(CommandContext ctx) => await RespondUserInterface(ctx);

        [Command("enable")]
        public async Task EnableAsync(CommandContext ctx) => await RespondUserInterface(ctx);
        [Command("disable")]
        public async Task DisableAsync(CommandContext ctx) => await RespondUserInterface(ctx);


        [Command("pokeme")]
        public async Task PokeMeAsync(CommandContext ctx) => await RespondUserInterface(ctx);
        [Command("pokemenot")]
        public async Task PokeMeNotAsync(CommandContext ctx) => await RespondUserInterface(ctx);

        [Command("pvpme")]
        public async Task PvpMeAsync(CommandContext ctx) => await RespondUserInterface(ctx);
        [Command("pvpmenot")]
        public async Task PvpMeNotAsync(CommandContext ctx) => await RespondUserInterface(ctx);

        [Command("raidme")]
        public async Task RaidMeAsync(CommandContext ctx) => await RespondUserInterface(ctx);
        [Command("raidmenot")]
        public async Task RaidMeNotAsync(CommandContext ctx) => await RespondUserInterface(ctx);

        [Command("questme")]
        public async Task QuestMeAsync(CommandContext ctx) => await RespondUserInterface(ctx);
        [Command("questmenot")]
        public async Task QuestMeNotAsync(CommandContext ctx) => await RespondUserInterface(ctx);

        [Command("invme")]
        public async Task InvasionMeAsync(CommandContext ctx) => await RespondUserInterface(ctx);
        [Command("invmenot")]
        public async Task InvasionMeNotAsync(CommandContext ctx) => await RespondUserInterface(ctx);

        [Command("lureme")]
        public async Task LureMeAsync(CommandContext ctx) => await RespondUserInterface(ctx);
        [Command("luremenot")]
        public async Task LureMeNotAsync(CommandContext ctx) => await RespondUserInterface(ctx);

        [Command("gymme")]
        public async Task GymMeAsync(CommandContext ctx) => await RespondUserInterface(ctx);
        [Command("gymmenot")]
        public async Task GymMeNotAsync(CommandContext ctx) => await RespondUserInterface(ctx);

        private async Task RespondUserInterface(CommandContext ctx)
        {
            // Make sure guild executing command is configured
            if (!_config.Instance.Servers.ContainsKey(ctx.Guild?.Id ?? 0))
                return;

            // Make sure subscriptions are enabled for guild
            var guildConfig = _config.Instance.Servers[ctx.Guild.Id];
            if (!guildConfig.Subscriptions.Enabled)
                return;

            var eb = new DiscordEmbedBuilder
            {
                // TODO: Localize
                Description = $"Please visit {guildConfig.Subscriptions.Url} to configure your subscriptions.",
                Color = DiscordColor.Blurple,
            };
            await ctx.RespondAsync(eb);
        }
    }
}