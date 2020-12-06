namespace WhMgr.Commands.Input
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using DSharpPlus.CommandsNext;
    using DSharpPlus.Entities;

    using WhMgr.Extensions;

    internal class QuestSubscriptionInput : SubscriptionInput
    {
        private readonly CommandContext _context;

        public QuestSubscriptionInput(CommandContext ctx) : base(ctx)
        {
            _context = ctx;
        }

        public async Task<string> GetRewardInput()
        {
            var message = (await _context.RespondEmbed($"Enter a reward keyword (i.e `larvitar`, `razz`, `1000 stardust`):", DiscordColor.Blurple)).FirstOrDefault();
            var reward = await _context.WaitForUserChoice();
            await message.DeleteAsync();
            return reward;
        }
    }
}