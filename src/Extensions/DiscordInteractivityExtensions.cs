namespace WhMgr.Extensions
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using DSharpPlus.CommandsNext;
    using DSharpPlus.Entities;
    using DSharpPlus.Interactivity;

    static class DiscordInteractivityExtensions
    {
        public static async Task<int> GetSubscriptionTypeSelection(this CommandContext ctx)
        {
            var msg = $@"
Select the type of subscription to create:
:one: Pokemon Subscription
:two: PvP Subscription
:three: Raid Subscription
:four: Quest Subscription
:five: Invasion Subscription
:six: Gym Subscription
";
            var message = ctx.RespondEmbed(msg, DiscordColor.Blurple).GetAwaiter().GetResult().FirstOrDefault();
            await message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":one:"));
            await message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":two:"));
            await message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":three:"));
            await message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":four:"));
            await message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":five:"));
            await message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":six:"));

            var interactivity = ctx.Client.GetInteractivityModule();
            // TODO: Configurable subscription timeout
            var resultReact = await interactivity.WaitForMessageReactionAsync(x => !string.IsNullOrEmpty(x.Name), message, ctx.User, TimeSpan.FromMinutes(3));
            if (resultReact == null)
            {
                await ctx.RespondEmbed($"Invalid result", DiscordColor.Red);
                return 0;
            }

            await message.DeleteAsync();
            switch (resultReact.Emoji.Name.ToLower())
            {
                case "1⃣": return 1;
                case "2⃣": return 2;
                case "3⃣": return 3;
                case "4⃣": return 4;
                case "5⃣": return 5;
                case "6⃣": return 6;
                default: return 0;
            }
        }

        public static async Task<string> WaitForUserChoice(this CommandContext ctx)
        {
            var interactivity = ctx.Client.GetInteractivityModule();
            // TODO: Configurable subscription timeout
            var result = await interactivity.WaitForMessageAsync(x => x.Author.Id == ctx.User.Id && !string.IsNullOrEmpty(x.Content), TimeSpan.FromMinutes(3));
            await result.Message.DeleteAsync();
            return result?.Message.Content;
        }
    }
}