namespace WhMgr.Commands.Input
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using DSharpPlus.CommandsNext;
    using DSharpPlus.Entities;

    using WhMgr.Data.Subscriptions.Models;
    using WhMgr.Extensions;
    using WhMgr.Localization;

    internal class PvPSubscriptionInput
    {
        private readonly CommandContext _context;

        public PvPSubscriptionInput(CommandContext ctx)
        {
            _context = ctx;
        }

        public async Task<PokemonValidation> GetPokemonResult()
        {
            var pokemonMessage = (await _context.RespondEmbed("Enter either the Pokemon name(s) or Pokedex ID(s) separated by a comma to subscribe to (i.e. larvitar,dratini):", DiscordColor.Blurple)).FirstOrDefault();
            var pokemonSubs = await _context.WaitForUserChoice();
            // Validate the provided pokemon list
            var validation = PokemonValidation.Validate(pokemonSubs);
            if (validation == null || validation.Valid.Count == 0)
            {
                await _context.RespondEmbed(Translator.Instance.Translate("NOTIFY_INVALID_POKEMON_IDS_OR_NAMES").FormatText(_context.User.Username, string.Join(", ", validation.Invalid)), DiscordColor.Red);
                return new PokemonValidation();
            }
            await pokemonMessage.DeleteAsync();
            return validation;
        }

        public async Task<PvPLeague> GetLeagueResult()
        {
            var message = (await _context.RespondEmbed($"Enter the PvP league type to use (i.e `Great` or `Ultra`):", DiscordColor.Blurple)).FirstOrDefault();
            var league = await _context.WaitForUserChoice();

            var pvpLeague = string.Compare(league, "great", true) == 0 ?
                PvPLeague.Great :
                string.Compare(league, "ultra", true) == 0 ?
                    PvPLeague.Ultra :
                    string.Compare(league, "master", true) == 0 ?
                        PvPLeague.Master :
                        PvPLeague.Other;

            if (pvpLeague == PvPLeague.Other)
            {
                await _context.RespondEmbed(Translator.Instance.Translate("NOTIFY_INVALID_PVP_LEAGUE").FormatText(_context.User.Username, league), DiscordColor.Red);
                return PvPLeague.Other;
            }

            await message.DeleteAsync();

            return pvpLeague;
        }

        public async Task<int> GetRankResult()
        {
            var message = (await _context.RespondEmbed($"Enter the minimum PvP rank to receive (i.e `1`, `3`, `25`):", DiscordColor.Blurple)).FirstOrDefault();
            var minRank = await _context.WaitForUserChoice();
            await message.DeleteAsync();

            if (!int.TryParse(minRank, out var minimumRank))
                return 3;

            //You may only subscribe to the top 100 or higher rank.
            if (minimumRank < Strings.MinimumRank || minimumRank > Strings.MaximumRank)
            {
                await _context.RespondEmbed(Translator.Instance.Translate("NOTIFY_INVALID_PVP_RANK_RANGE").FormatText(_context.User.Username, minRank), DiscordColor.Red);
                return 3;
            }
            return minimumRank;
        }

        public async Task<double> GetPercentResult()
        {
            var message = (await _context.RespondEmbed($"Enter the minimum PvP rank percent to receive (i.e `90`, `99`, '100'):", DiscordColor.Blurple)).FirstOrDefault();
            var minPercent = await _context.WaitForUserChoice();
            await message.DeleteAsync();

            if (!double.TryParse(minPercent, out var minimumPercent))
                return 98;

            if (minimumPercent < Strings.MinimumPercent || minimumPercent > Strings.MaximumPercent)
            {
                await _context.RespondEmbed(Translator.Instance.Translate("NOTIFY_INVALID_PVP_PERCENT_RANGE").FormatText(_context.User.Username, minimumPercent), DiscordColor.Red);
                return 98;
            }
            return minimumPercent;
        }

        public async Task<List<string>> GetAreasResult(List<string> validAreas)
        {
            var message = (await _context.RespondEmbed($"Enter the areas to get notifications from separated by a comma:", DiscordColor.Blurple)).FirstOrDefault();
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