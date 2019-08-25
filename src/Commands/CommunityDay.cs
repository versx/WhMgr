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

    [
        Group("event"),
        Aliases("ev"),
        Description("Administrative commands."),
        Hidden,
        RequirePermissions(Permissions.Administrator)
    ]
    public class CommunityDay
    {
        private static readonly IEventLogger _logger = EventLogger.GetLogger();

        private readonly Dependencies _dep;

        public CommunityDay(Dependencies dep)
        {
            _dep = dep;
        }

        [
            Command("list"),
            Aliases("l"),
            Description("")
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
                    Text = $"versx | {DateTime.Now}",
                    IconUrl = ctx.Guild?.IconUrl
                }
            };
            var pkmnNames = new List<string>();
            for (var i = 0; i < _dep.WhConfig.EventPokemonIds.Count; i++)
            {
                var pkmnId = _dep.WhConfig.EventPokemonIds[i];
                if (Database.Instance.Pokemon.ContainsKey(pkmnId))
                {
                    pkmnNames.Add(pkmnId + ":" + Database.Instance.Pokemon[pkmnId].Name);
                }
            }

            eb.AddField("Event Pokemon", string.Join("\r\n", pkmnNames));
            await ctx.RespondAsync(embed: eb);
        }

        [
            Command("set"),
            Aliases("s"),
            Description("")
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
                if (int.TryParse(eventPokemonId, out var pokemonId) && (pokemonId == 0 || Database.Instance.Pokemon.ContainsKey(pokemonId)))
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
                if (Database.Instance.Pokemon.ContainsKey(pkmnId))
                {
                    pkmnNames.Add(pkmnId + ":" + Database.Instance.Pokemon[pkmnId].Name);
                }
            }

            await ctx.RespondEmbed($"Event Pokemon set to `{string.Join(", ", pkmnNames)}`, feeds will adjust according and only show in channels that are 90% or higher.{(pkmnFailed.Count == 0 ? "" : $"\r\nFailed to parse the following Pokemon IDs: `{string.Join(", ", pkmnFailed)}`")}");
        }
    }
}