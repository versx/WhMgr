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

    internal sealed class PokemonSubscriptionInput
    {
        private readonly CommandContext _context;

        public PokemonSubscriptionInput(CommandContext ctx)
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

        public async Task<IVResult> GetIVResult()
        {
            var message = (await _context.RespondEmbed("Enter the minimum IV value or specific individual values (i.e. 95 or 0-14-15):", DiscordColor.Blurple)).FirstOrDefault();
            var userValue = await _context.WaitForUserChoice();
            var attack = -1;
            var defense = -1;
            var stamina = -1;
            var realIV = 0;

            // Check if IV value contains `-` and to expect individual values instead of whole IV value
            if (userValue.Contains("-"))
            {
                var split = userValue.Split('-');
                if (split.Length != 3)
                {
                    await _context.RespondEmbed(Translator.Instance.Translate("NOTIFY_INVALID_IV_VALUES").FormatText(_context.User.Username, userValue), DiscordColor.Red);
                    return new IVResult();
                }
                if (!int.TryParse(split[0], out attack))
                {
                    await _context.RespondEmbed(Translator.Instance.Translate("NOTIFY_INVALID_ATTACK_VALUE").FormatText(_context.User.Username, split[0]), DiscordColor.Red);
                    return new IVResult();
                }
                if (!int.TryParse(split[1], out defense))
                {
                    await _context.RespondEmbed(Translator.Instance.Translate("NOTIFY_INVALID_DEFENSE_VALUE").FormatText(_context.User.Username, split[1]), DiscordColor.Red);
                    return new IVResult();
                }
                if (!int.TryParse(split[2], out stamina))
                {
                    await _context.RespondEmbed(Translator.Instance.Translate("NOTIFY_INVALID_STAMINA_VALUE").FormatText(_context.User.Username, split[2]), DiscordColor.Red);
                    return new IVResult();
                }
            }
            else
            {
                // User provided IV value as a whole
                if (!int.TryParse(userValue, out realIV) || realIV < Strings.MinimumIV || realIV > Strings.MaximumIV)
                {
                    await _context.RespondEmbed(Translator.Instance.Translate("NOTIFY_INVALID_IV_RANGE").FormatText(_context.User.Username, userValue), DiscordColor.Red);
                    return new IVResult();
                }
            }
            await message.DeleteAsync();

            return new IVResult
            {
                IV = (ushort)realIV,
                Attack = (short)attack,
                Defense = (short)defense,
                Stamina = (short)stamina,
            };
        }

        public async Task<LevelResult> GetLevelResult()
        {
            var message = (await _context.RespondEmbed($"Enter the minimum level or minimum and maximum level (i.e 25 or 25-35):", DiscordColor.Blurple)).FirstOrDefault();
            var levelSub = await _context.WaitForUserChoice();
            var minLevel = Strings.MinimumLevel;
            var maxLevel = Strings.MaximumLevel;
            // Check if level contains `-` and to expect a minimum and maximum level provided
            if (levelSub.Contains('-'))
            {
                var split = levelSub.Split('-');
                if (!int.TryParse(split[0], out minLevel))
                {
                    await _context.RespondEmbed(Translator.Instance.Translate("NOTIFY_INVALID_MINIMUM_LEVEL", _context.User.Username, split[0]), DiscordColor.Red);
                    return new LevelResult();
                }
                if (!int.TryParse(split[1], out maxLevel))
                {
                    await _context.RespondEmbed(Translator.Instance.Translate("NOTIFY_INVALID_MAXIMUM_LEVEL", _context.User.Username, split[1]), DiscordColor.Red);
                    return new LevelResult();
                }
            }
            else
            {
                // Only minimum level was provided
                if (!int.TryParse(levelSub, out minLevel))
                {
                    await _context.RespondEmbed(Translator.Instance.Translate("NOTIFY_INVALID_MINIMUM_LEVEL", _context.User.Username, levelSub), DiscordColor.Red);
                    return new LevelResult();
                }
            }

            // Validate minimum and maximum levels are within range
            if (minLevel < 0 || minLevel > 35)
            {
                await _context.RespondEmbed(Translator.Instance.Translate("NOTIFY_INVALID_LEVEL").FormatText(_context.User.Username, levelSub), DiscordColor.Red);
                return new LevelResult();
            }

            await message.DeleteAsync();

            return new LevelResult
            {
                MinimumLevel = (ushort)minLevel,
                MaximumLevel = (ushort)maxLevel,
            };
        }

        public async Task<string> GetGenderResult()
        {
            var message = (await _context.RespondEmbed($"Enter the gender to receive notifications for (i.e `m`, `f`, or `*`):", DiscordColor.Blurple)).FirstOrDefault();
            var gender = await _context.WaitForUserChoice();

            // Check if gender is a valid gender provided
            if (!Strings.ValidGenders.Contains(gender.ToLower()))
            {
                await _context.TriggerTypingAsync();
                await _context.RespondEmbed(Translator.Instance.Translate("NOTIFY_INVALID_GENDER").FormatText(_context.User.Username, gender), DiscordColor.Red);
                return "*";
            }

            await message.DeleteAsync();

            return gender;
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

    internal sealed class IVResult
    {
        public ushort IV { get; set; }

        public short Attack { get; set; }

        public short Defense { get; set; }

        public short Stamina { get; set; }

        public bool IsSet => Attack == -1 || Defense == -1 || Stamina == -1;

        public IVResult() : this(0, -1, -1, -1)
        {
        }

        public IVResult(ushort iv, short attack, short defense, short stamina)
        {
            IV = iv;
            Attack = attack;
            Defense = defense;
            Stamina = stamina;
        }
    }

    internal sealed class LevelResult
    {
        public ushort MinimumLevel { get; set; }

        public ushort MaximumLevel { get; set; }

        public LevelResult() : this(0, 35)
        {
        }

        public LevelResult(ushort minLevel, ushort maxLevel)
        {
            MinimumLevel = minLevel;
            MaximumLevel = maxLevel;
        }
    }
}