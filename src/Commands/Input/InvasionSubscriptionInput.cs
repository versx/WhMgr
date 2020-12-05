namespace WhMgr.Commands.Input
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using DSharpPlus.CommandsNext;
    using DSharpPlus.Entities;

    using WhMgr.Extensions;
    using WhMgr.Localization;

    internal class InvasionSubscriptionInput
    {
        private readonly CommandContext _context;

        public InvasionSubscriptionInput(CommandContext ctx)
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