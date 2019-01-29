namespace WhMgr.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using DSharpPlus.CommandsNext;
    using DSharpPlus.CommandsNext.Attributes;
    using DSharpPlus.Entities;

    using WhMgr.Diagnostics;
    using WhMgr.Extensions;

    public class Feeds
    {
        private static readonly IEventLogger _logger = EventLogger.GetLogger();

        private readonly Dependencies _dep;

        public Feeds(Dependencies dep)
        {
            _dep = dep;
        }

        [
            Command("feeds"),
            Aliases("cities", "roles"),
            Description("Shows a list of assignable city roles and other roles.")
        ]
        public async Task FeedsAsync(CommandContext ctx)
        {
            var eb = new DiscordEmbedBuilder
            {
                Color = DiscordColor.Red,
                Description = 
                    "**Available City Roles:**\r\n" +
                    $"- {string.Join($"{Environment.NewLine}- ", _dep.WhConfig.CityRoles)}" +
                    Environment.NewLine +
                    $"- {Strings.All}" +
                    Environment.NewLine +
                    Environment.NewLine +
                    $"*Type `{_dep.WhConfig.CommandPrefix}feedme cityname` to assign yourself to that city role.*",
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = $"versx | {DateTime.Now}",
                    IconUrl = ctx.Guild?.IconUrl
                }
            };

            await ctx.TriggerTypingAsync();
            await ctx.RespondAsync(embed: eb.Build());
        }

        [
            Command("feedme"),
            Description("Joins a city feed.\r\n\r\n**Example:** `.feedme Upland,Ontario`")
        ]
        public async Task FeedMeAsync(CommandContext ctx,
            [Description("City name to join or all."), RemainingText] string cityName = null)
        {
            if (!await ctx.Message.IsDirectMessageSupported())
                return;

            if (string.IsNullOrEmpty(cityName))
            {
                //TODO: Show message with reactions to assign.
            }

            if (string.Compare(cityName, Strings.All, true) == 0)
            {
                await AssignAllDefaultFeedRoles(ctx);
                return;
            }

            var assigned = new List<string>();
            var alreadyAssigned = new List<string>();

            try
            {
                var cityNames = cityName.Replace(" ", "").Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var city in cityNames)
                {
                    var roles = new List<string>();
                    roles.AddRange(_dep.WhConfig.CityRoles);
                    if (roles.FirstOrDefault(x => string.Compare(city, x, true) == 0) == null)
                    {
                        await ctx.RespondAsync($"{ctx.User.Mention} {city} is an incorrect city name, please type `.cities` to see a list of available cities.");
                        continue;
                    }

                    var cityRole = ctx.Client.GetRoleFromName(city);
                    if (cityRole == null)
                    {
                        await ctx.RespondAsync($"{ctx.User.Mention} {city} is not a valid city role name.");
                        continue;
                    }

                    var result = await AddFeedRole(ctx.Member, cityRole);
                    if (result)
                    {
                        assigned.Add(cityRole.Name);
                    }
                    else
                    {
                        alreadyAssigned.Add(cityRole.Name);
                    }

                    if (_dep.WhConfig.CityRoles.FirstOrDefault(x => string.Compare(x, city, true) == 0) != null)
                    {
                        var cityRaidRole = ctx.Client.GetRoleFromName($"{city}Raids");
                        if (cityRaidRole != null)
                        {
                            result = await AddFeedRole(ctx.Member, cityRaidRole);
                            if (result)
                            {
                                assigned.Add(cityRaidRole.Name);
                            }
                            else
                            {
                                alreadyAssigned.Add(cityRaidRole.Name);
                            }
                        }
                    }
                }

                if (assigned.Count == 0 && alreadyAssigned.Count == 0)
                {
                    ctx.Client.DebugLogger.LogMessage(DSharpPlus.LogLevel.Debug, "Feeds", $"No roles assigned or already assigned for user {ctx.User.Username} ({ctx.User.Id}). Value: {string.Join(", ", cityNames)}", DateTime.Now);
                    //await message.RespondAsync($"{message.Author.Mention} you did not provide valid values that I could recognize.");
                    return;
                }

                await ctx.RespondAsync
                (
                    (assigned.Count > 0
                        ? $"{ctx.User.Mention} has joined role{(assigned.Count > 1 ? "s" : null)} **{string.Join("**, **", assigned)}**."
                        : string.Empty) +
                    (alreadyAssigned.Count > 0
                        ? $"\r\n{ctx.User.Mention} is already assigned to **{string.Join("**, **", alreadyAssigned)}** role{(alreadyAssigned.Count > 1 ? "s" : null)}."
                        : string.Empty)
                );
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }

        [
            Command("feedmenot"),
            Description("Leaves a city's feed.")
        ]
        public async Task FeedMeNotAsync(CommandContext ctx,
            [Description("City name to leave or all."), RemainingText] string cityName)
        {
            if (!await ctx.Message.IsDirectMessageSupported())
                return;

            if (string.Compare(cityName, Strings.All, true) == 0)
            {
                await RemoveAllDefaultFeedRoles(ctx);
                return;
            }

            var unassigned = new List<string>();
            var alreadyUnassigned = new List<string>();

            try
            {
                var cityNames = cityName.Replace(" ", "").Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var city in cityNames)
                {
                    var roles = new List<string>();
                    roles.AddRange(_dep.WhConfig.CityRoles);
                    if (!roles.Exists(x => string.Compare(city, x, true) == 0))
                    {
                        await ctx.RespondAsync($"{ctx.User.Mention} has entered an incorrect city feed name, please enter one of the following: {(string.Join(",", _dep.WhConfig.CityRoles))}, or {Strings.All}.");
                        continue;
                    }

                    var cityRole = ctx.Client.GetRoleFromName(city);
                    if (cityRole == null)
                    {
                        await ctx.RespondAsync($"{ctx.User.Mention} {city} is not a valid city role name.");
                        continue;
                    }

                    var result = await RemoveFeedRole(ctx.Member, cityRole);
                    if (result)
                    {
                        unassigned.Add(cityRole.Name);
                    }
                    else
                    {
                        alreadyUnassigned.Add(cityRole.Name);
                    }

                    var cityRaidRole = ctx.Client.GetRoleFromName($"{city}Raids");
                    if (cityRaidRole != null)
                    {
                        result = await RemoveFeedRole(ctx.Member, cityRaidRole);
                        if (result)
                        {
                            unassigned.Add(cityRaidRole.Name);
                        }
                        else
                        {
                            alreadyUnassigned.Add(cityRaidRole.Name);
                        }
                    }
                }

                await ctx.RespondAsync
                (
                    (unassigned.Count > 0
                        ? $"{ctx.User.Mention} has been removed from role{(unassigned.Count > 1 ? "s" : null)} **{string.Join("**, **", unassigned)}**."
                        : string.Empty) +
                    (alreadyUnassigned.Count > 0
                        ? $"\r\n{ctx.User.Mention} is not assigned to **{string.Join("**, **", alreadyUnassigned)}** roles{(alreadyUnassigned.Count > 1 ? "s" : null)}."
                        : string.Empty)
                );
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }

        //[
        //    Command("chatme"),
        //    Aliases("tagme"),
        //    Description("Assign yourself to a city raid chat role.")
        //]
        //public async Task ChatMeAsync(CommandContext ctx,
        //    [Description("City raid chat role to assign or leave blank for all.")] string raidChatRoleName = "")
        //{
        //    if (ctx.Guild == null)
        //    {
        //        var channel = await ctx.Client.GetChannel(_dep.Config.CommandsChannelId);
        //        if (channel == null) return;

        //        await ctx.RespondAsync($"{ctx.User.Mention} Currently I only support city feed assignment via the channel #{channel.Name}, direct message support is coming soon.");
        //        return;
        //    }

        //    var raidChatRole = ctx.Client.GetRoleFromName(raidChatRoleName);
        //    if (raidChatRole != null)
        //    {
        //        if (!raidChatRole.Name.ToLower().Contains("raids"))
        //        {
        //            await ctx.RespondAsync($"{ctx.User.Mention} Invalid role, please use one of the city raid chat roles such as @UplandRaids, @ChinoRaids, etc.");
        //            return;
        //        }

        //        var feedRole = ctx.Client.GetRoleFromName(raidChatRoleName.Replace("raids", ""));
        //        if (feedRole == null)
        //        {
        //            await ctx.RespondAsync($"{ctx.User.Mention} Failed to check if you have the appropriate city feed role.");
        //            return;
        //        }

        //        if (!ctx.Member.HasRole(feedRole.Id))
        //        {
        //            await ctx.RespondAsync($"{ctx.User.Mention} You must first assign yourself the city feed role in order to assign yourself to your specified raids chat role. Please use the `.feedme` command to join a city feed.");
        //            return;
        //        }

        //        if (!ctx.Member.Roles.Contains(raidChatRole))
        //        {
        //            await ctx.Member.GrantRoleAsync(raidChatRole);
        //            await ctx.RespondAsync($"{ctx.User.Mention} was successfully assigned the {raidChatRole.Name} raid chat role.");
        //        }
        //        else
        //        {
        //            await ctx.RespondAsync($"{ctx.User.Mention} is already assigned to the {raidChatRole.Name} raid chat role.");
        //        }

        //        return;
        //    }

        //    try
        //    {
        //        var assigned = new List<string>();
        //        var alreadyAssigned = new List<string>();
        //        var roles = ctx.Member.Roles.ToList();
        //        for (int i = 0; i < roles.Count; i++)
        //        {
        //            var role = roles[i];
        //            if (!_dep.Config.CityRoles.Contains(role.Name))
        //                continue;

        //            var cityRaidRole = ctx.Client.GetRoleFromName(role.Name + "Raids");
        //            if (cityRaidRole == null)
        //            {
        //                _dep.Logger.Error($"Failed to retrieve city raids role {role.Name}Raids.");
        //                continue;
        //            }

        //            if (ctx.Member.HasRole(cityRaidRole.Id))
        //            {
        //                alreadyAssigned.Add(cityRaidRole.Name);
        //                continue;
        //            }

        //            var reason = $"User initiated city raid assignment via {AssemblyUtils.AssemblyName}.";
        //            await ctx.Guild.GrantRoleAsync(ctx.Member, cityRaidRole, reason);
        //            assigned.Add(cityRaidRole.Name);
        //        }

        //        if (assigned.Count == 0 && alreadyAssigned.Count == 0)
        //        {
        //            //await message.RespondAsync($"{message.Author.Mention} you did not provide valid values that I could recognize.");
        //            return;
        //        }

        //        await ctx.RespondAsync
        //        (
        //            (assigned.Count > 0
        //                ? $"{ctx.User.Mention} has turned on city raid{(assigned.Count > 1 ? "s" : null)} chat role(s) for **{string.Join("**, **", assigned)}**."
        //                : string.Empty) +
        //            (alreadyAssigned.Count > 0
        //                ? $"\r\n{ctx.User.Mention} is already assigned to **{string.Join("**, **", alreadyAssigned)}** city raid{(alreadyAssigned.Count > 1 ? "s" : null)} chat role(s)."
        //                : string.Empty)
        //        );
        //    }
        //    catch (Exception ex)
        //    {
        //        _dep.Logger.Error(ex);
        //    }
        //}

        //[
        //    Command("chatmenot"),
        //    Aliases("tagmenot"),
        //    Description("Unassign yourself from a city raid chat role.")
        //]
        //public async Task ChatMeNotAsync(CommandContext ctx,
        //    [Description("City raid chat role to unassign or leave blank for all.")] string raidChatRoleName = "")
        //{
        //    if (ctx.Guild == null)
        //    {
        //        var channel = await ctx.Client.GetChannel(_dep.Config.CommandsChannelId);
        //        if (channel == null) return;

        //        await ctx.RespondAsync($"{ctx.User.Mention} Currently I only support city feed assignment via the channel #{channel.Name}, direct message support is coming soon.");
        //        return;
        //    }

        //    var raidChatRole = ctx.Client.GetRoleFromName(raidChatRoleName);
        //    if (raidChatRole != null)
        //    {
        //        if (!raidChatRole.Name.ToLower().Contains("raids"))
        //        {
        //            await ctx.RespondAsync($"{ctx.User.Mention} Invalid role, please use one of the city raid chat tag roles such as UplandRaids, ChinoRaids, etc.");
        //            return;
        //        }

        //        if (ctx.Member.Roles.Contains(raidChatRole))
        //        {
        //            await ctx.Member.RevokeRoleAsync(raidChatRole);
        //            await ctx.RespondAsync($"{ctx.User.Mention} was successfully unassigned the {raidChatRole.Name} raid chat role.");
        //        }
        //        else
        //        {
        //            await ctx.RespondAsync($"{ctx.User.Mention} is not assigned to the {raidChatRole.Name} raid chat role.");
        //        }

        //        return;
        //    }

        //    var unassigned = new List<string>();
        //    var alreadyUnassigned = new List<string>();

        //    try
        //    {
        //        var roles = ctx.Member.Roles.ToList();
        //        for (int i = 0; i < roles.Count; i++)
        //        {
        //            var role = roles[i];
        //            if (!_dep.Config.CityRoles.Contains(role.Name))
        //                continue;

        //            var cityRaidRole = ctx.Client.GetRoleFromName(role.Name + "Raids");
        //            if (cityRaidRole == null)
        //            {
        //                _dep.Logger.Error($"Failed to retrieve city raids chat role {role.Name}Raids.");
        //                continue;
        //            }

        //            if (ctx.Member.HasRole(cityRaidRole.Id))
        //            {
        //                var reason = $"{ctx.User.Mention} initiated city raid assignment removal via {AssemblyUtils.AssemblyName}.";
        //                await ctx.Member.RevokeRoleAsync(cityRaidRole, reason);
        //                unassigned.Add(cityRaidRole.Name);
        //                continue;
        //            }

        //            alreadyUnassigned.Add(cityRaidRole.Name);
        //        }

        //        await ctx.RespondAsync
        //        (
        //            (unassigned.Count > 0
        //                ? $"{ctx.User.Mention} has been removed from city raid{(unassigned.Count > 1 ? "s" : null)} chat role(s) **{string.Join("**, **", unassigned)}**."
        //                : string.Empty) +
        //            (alreadyUnassigned.Count > 0
        //                ? $"\r\n{ctx.User.Mention} is not assigned to **{string.Join("**, **", alreadyUnassigned)}** city raid{(alreadyUnassigned.Count > 1 ? "s" : null)} chat role(s)."
        //                : string.Empty)
        //        );
        //    }
        //    catch (Exception ex)
        //    {
        //        _dep.Logger.Error(ex);
        //    }
        //}

        private async Task AssignAllDefaultFeedRoles(CommandContext ctx)
        {
            if (_dep.WhConfig.CityRoles == null)
            {
                _logger.Warn($"City roles empty.");
                return;
            }

            foreach (var city in _dep.WhConfig.CityRoles)
            {
                var cityRole = ctx.Client.GetRoleFromName(city);
                if (cityRole == null)
                {
                    _logger.Error($"Failed to get city raid role from city {city}.");
                    continue;
                }

                var result = await AddFeedRole(ctx.Member, cityRole);
                if (!result)
                {
                    _logger.Error($"Failed to assign role {cityRole.Name} to user {ctx.User.Username} ({ctx.User.Id}).");
                }
            }

            await ctx.RespondAsync($"{ctx.User.Mention} was assigned all city feed roles.");
        }

        private async Task RemoveAllDefaultFeedRoles(CommandContext ctx)
        {
            foreach (var city in _dep.WhConfig.CityRoles)
            {
                var cityRole = ctx.Client.GetRoleFromName(city);
                if (cityRole == null)
                {
                    _logger.Error($"Failed to get city role from city {city}.");
                    continue;
                }

                var result = await RemoveFeedRole(ctx.Member, cityRole);
                if (!result)
                {
                    _logger.Error($"Failed to remove role {cityRole.Name} from user {ctx.User.Username} ({ctx.User.Id}).");
                }
            }

            await ctx.RespondAsync($"{ctx.User.Mention} was unassigned all city feed roles.");
        }

        private async Task<bool> AddFeedRole(DiscordMember member, DiscordRole city)
        {
            var reason = "Default city role/raid role assignment.";
            if (city == null)
            {
                _logger.Error($"Failed to find city role {city?.Name}, please make sure it exists.");
                return false;
            }

            await member.GrantRoleAsync(city, reason);
            return true;
        }

        private async Task<bool> RemoveFeedRole(DiscordMember member, DiscordRole city)
        {
            var reason = "Default city role/raid role removal.";
            if (city == null)
            {
                _logger.Error($"Failed to find city role {city?.Name}, please make sure it exists.");
                return false;
            }

            await member.RevokeRoleAsync(city, reason);
            return true;
        }
    }
}