namespace WhMgr.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using DSharpPlus;
    using DSharpPlus.CommandsNext;
    using DSharpPlus.CommandsNext.Attributes;
    using DSharpPlus.Entities;

    using WhMgr.Data;
    using WhMgr.Diagnostics;
    using WhMgr.Extensions;

    //TODO: Expand with add/remove commands

    [
        Group("event"),
        Aliases("ev"),
        Description("Event Pokemon management commands."),
        Hidden,
        RequirePermissions(Permissions.KickMembers)
    ]
    public class CommunityDay
    {
        private static readonly IEventLogger _logger = EventLogger.GetLogger("COMMUNITYDAY");

        private readonly Dependencies _dep;

        public CommunityDay(Dependencies dep)
        {
            _dep = dep;
        }

        [
            Command("list"),
            Aliases("l"),
            Description("List all Pokemon considered as `event`.")
        ]
        public async Task ListCommandDayAsync(CommandContext ctx)
        {
            var eb = new DiscordEmbedBuilder
            {
                Color = DiscordColor.Blurple,
                Description = "List of Pokemon set as event Pokemon which will only show in channels that are 90% or higher.",
                Title = "Event Pokemon List",
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = $"{ctx.Guild?.Name} | {DateTime.Now}",
                    IconUrl = ctx.Guild?.IconUrl
                }
            };
            var pkmnNames = new List<string>();
            for (var i = 0; i < _dep.WhConfig.EventPokemonIds.Count; i++)
            {
                var pkmnId = _dep.WhConfig.EventPokemonIds[i];
                if (MasterFile.Instance.Pokedex.ContainsKey(pkmnId))
                {
                    pkmnNames.Add(pkmnId + ":" + MasterFile.Instance.Pokedex[pkmnId].Name);
                }
            }

            eb.AddField("Event Pokemon", string.Join("\r\n", pkmnNames));
            await ctx.RespondAsync(embed: eb);
        }

        [
            Command("set"),
            Aliases("s"),
            Description("Sets a list of Pokemon as `event`.")
        ]
        public async Task SetCommunityDayAsync(CommandContext ctx,
            [Description("Comma separated list of event Pokemon")] string eventPokemonIds = "0")
        {
            var eventPokemonSplit = eventPokemonIds.Split(',');
            var pkmnToAdd = new List<int>();
            var pkmnFailed = new List<string>();
            for (var i = 0; i < eventPokemonSplit.Length; i++)
            {
                var eventPokemonId = eventPokemonSplit[i];
                if (int.TryParse(eventPokemonId, out var pokemonId) && (pokemonId == 0 || MasterFile.Instance.Pokedex.ContainsKey(pokemonId)))
                {
                    pkmnToAdd.Add(pokemonId);
                    continue;
                }

                pkmnFailed.Add(eventPokemonId);
            }

            _dep.WhConfig.EventPokemonIds = pkmnToAdd;
            _dep.WhConfig.Save(_dep.WhConfig.FileName);

            var pkmnNames = new List<string>();
            for (var i = 0; i < pkmnToAdd.Count; i++)
            {
                var pkmnId = pkmnToAdd[i];
                if (MasterFile.Instance.Pokedex.ContainsKey(pkmnId))
                {
                    pkmnNames.Add(pkmnId + ":" + MasterFile.Instance.Pokedex[pkmnId].Name);
                }
            }

            var message = _dep.Language.Translate("EVENT_POKEMON_SET").FormatText(ctx.User.Username, string.Join(", ", pkmnNames));
            if (pkmnFailed.Count > 0)
            {
                message += "\r\n" + _dep.Language.Translate("NOTIFY_INVALID_POKEMON_IDS_OR_NAMES").FormatText(ctx.User.Username, string.Join(", ", pkmnFailed));
            }
            await ctx.RespondEmbed(message);
        }
    }
}