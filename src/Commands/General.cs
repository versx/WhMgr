namespace WhMgr.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using DSharpPlus.CommandsNext;
    using DSharpPlus.CommandsNext.Attributes;

    using WhMgr.Diagnostics;

    public class General
    {
        private static readonly IEventLogger _logger = EventLogger.GetLogger();
        private readonly Dependencies _dep;
        private readonly string[] _cityRoles =
        {
            "RanchoRaids",
            "UplandRaids",
            "OntarioRaids",
            "ChinoRaids",
            "ClaremontRaids",
            "PomonaRaids",
            "MontclairRaids",
            "EastLARaids",
            "WhittierRaids",
            "Rancho",
            "Upland",
            "Ontario",
            "Chino",
            "Claremont",
            "Pomona",
            "Montclair",
            "EastLA",
            "Whittier",
            "100iv",
            "90iv",
            "lunatone",
            "Nests",
            "Raids",
            "Families",
            "Quests",
            "RaidTrain"
        };

        public General(Dependencies dep)
        {
            _dep = dep;
        }

        [
            Command("reset-roles"),
            RequireOwner,
            Hidden
        ]
        public async Task ResetRoles(CommandContext ctx)
        {
            await ctx.RespondAsync($"{ctx.User.Mention} Starting role refresh for {ctx.Guild.MemberCount.ToString("N0")} users, this is going to take a while...");

            var success = 0;
            var failed = 0;
            //var members = new List<DiscordMember> { ctx.Client.GetMemberById(_dep.WhConfig.GuildId, _dep.WhConfig.OwnerId) };
            var members = ctx.Channel.Guild.Members;
            for (var usrId = 0; usrId < members.Count; usrId++)
            {
                var member = members[usrId];
                var roleIds = member.Roles.Select(x => x.Id).ToList();
                var roleNames = member.Roles.Select(x => x.Name.ToLower()).ToList();

                await ctx.RespondAsync($"Starting role refresh for user {member.Username} ({member.Id}).");

                // Skip supporters and members that already have a team role set.
                if (roleIds.Contains(_dep.WhConfig.SupporterRoleId)
                    || roleNames.Contains("valor")
                    || roleNames.Contains("mystic")
                    || roleNames.Contains("instinct")
                    || roleNames.Contains("tmxeliteeastla")
                    || roleNames.Contains("bots"))
                    continue;

                _logger.Debug($"Checking user {member.Username}#{member.Discriminator} roles...");
                var list = new List<string>();
                foreach (var cityRole in _cityRoles)
                {
                    if (!roleNames.Contains(cityRole.ToLower()))
                        continue;

                    try
                    {
                        var role = member.Roles.FirstOrDefault(x => string.Compare(x.Name, cityRole, true) == 0);
                        await member.RevokeRoleAsync(role, "Roles refreshed.");
                        _logger.Debug($"Removed role {role.Name} ({role.Id}) from user {member.Username}#{member.Discriminator}.");
                        success++;
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex);
                        failed++;
                    }
                }

                await ctx.RespondAsync($"Roles refresh for user {member.Username}#{member.Discriminator} finished.");
            }

            await ctx.RespondAsync($"Roles refreshed for all users.");
        }
    }
}