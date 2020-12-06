namespace WhMgr.Commands.Input
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using DSharpPlus.CommandsNext;
    using DSharpPlus.Entities;

    using WhMgr.Extensions;

    internal class QuestSubscriptionInput
    {
        private readonly CommandContext _context;

        public QuestSubscriptionInput(CommandContext ctx)
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

        public async Task<List<string>> GetAreasResult(List<string> validAreas)
        {
            var message = (await _context.RespondEmbed($"Enter the areas to get notifications from separated by a comma (i.e. `city1,city2`):", DiscordColor.Blurple)).FirstOrDefault();
            var cities = await _context.WaitForUserChoice();

            // Check if gender is a valid gender provided
            var areas = SubscriptionAreas.GetAreas(cities, validAreas);
            if (areas.Count == 0)
            {
                // No valid areas provided
                await _context.RespondEmbed($"Invalid areas provided.");
                return new List<string>();
            }
            await message.DeleteAsync();

            return areas;
        }
    }
}