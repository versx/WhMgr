namespace WhMgr.Commands
{
    using System;
    using System.Threading.Tasks;

    using DSharpPlus.CommandsNext;
    using DSharpPlus.CommandsNext.Attributes;
    using DSharpPlus.Entities;

    using WhMgr.Diagnostics;
    using WhMgr.Utilities;

    [
        RequireOwner,
        Hidden
    ]
    public class Owner
    {
        const string PokemonTrainerClub = "https://sso.pokemon.com/sso/login";
        const string NianticLabs = "https://pgorelease.nianticlabs.com/plfe/version";

        private static readonly IEventLogger _logger = EventLogger.GetLogger();
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