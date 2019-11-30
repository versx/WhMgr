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
        private static readonly IEventLogger _logger = EventLogger.GetLogger("FEEDS");

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
            if (!await ctx.Message.IsDirectMessageSupported())
                return;

            if (!_dep.WhConfig.Servers.ContainsKey(ctx.Guild.Id))
                return;

            var eb = new DiscordEmbedBuilder
            {
                Color = DiscordColor.Red,
                Description = 
                    "**Available City Roles:**\r\n" +
                    $"- {string.Join($"{Environment.NewLine}- ", _dep.WhConfig.Servers[ctx.Guild.Id].CityRoles)}" +
                    Environment.NewLine +
                    $"- {Strings.All}" +
                    Environment.NewLine +
                    Environment.NewLine +
                    $"*Type `{_dep.WhConfig.Servers[ctx.Guild.Id].CommandPrefix}feedme cityname` to assign yourself to that city role.*",
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = $"{ctx.Guild?.Name} | {DateTime.Now}",
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

            var isSupporter = ctx.Client.IsSupporterOrHigher(ctx.User.Id, ctx.Guild.Id, _dep.WhConfig);
            if (_dep.WhConfig.Servers[ctx.Guild.Id].CitiesRequireSupporterRole && !isSupporter)
            {
                await ctx.DonateUnlockFeaturesMessage();
                return;
            }

            if (string.IsNullOrEmpty(cityName))
            {
                await ctx.RespondEmbed($"Please specific a city role name to assign.");
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
                    var roles = _dep.WhConfig.Servers[ctx.Guild.Id].CityRoles;
                    if (roles.FirstOrDefault(x => string.Compare(city, x, true) == 0) == null)
                    {
                        await ctx.RespondEmbed($"{ctx.User.Username}#{ctx.User.Discriminator} {city} is not a valid city name, type `.cities` to see a list of available cities.");
                        continue;
                    }

                    var cityRole = ctx.Client.GetRoleFromName(city);
                    if (cityRole == null)
                    {
                        await ctx.RespondEmbed($"{ctx.User.Mention}#{ctx.User.Discriminator} {city} is not a valid city name.");
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

                    if (_dep.WhConfig.Servers[ctx.Guild.Id].CityRoles.FirstOrDefault(x => string.Compare(x, city, true) == 0) != null)
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

                await ctx.RespondEmbed
                (
                    (assigned.Count > 0
                        ? $"{ctx.User.Username}#{ctx.User.Discriminator} has joined role{(assigned.Count > 1 ? "s" : null)} **{string.Join("**, **", assigned)}**."
                        : string.Empty) +
                    (alreadyAssigned.Count > 0
                        ? $"\r\n{ctx.User.Username}#{ctx.User.Discriminator} is already assigned to **{string.Join("**, **", alreadyAssigned)}** role{(alreadyAssigned.Count > 1 ? "s" : null)}."
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

            var isSupporter = ctx.Client.IsSupporterOrHigher(ctx.User.Id, ctx.Guild.Id, _dep.WhConfig);
            if (_dep.WhConfig.Servers[ctx.Guild.Id].CitiesRequireSupporterRole && !isSupporter)
            {
                await ctx.DonateUnlockFeaturesMessage();
                return;
            }

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
                    roles.AddRange(_dep.WhConfig.Servers[ctx.Guild.Id].CityRoles);
                    if (!roles.Exists(x => string.Compare(city, x, true) == 0))
                    {
                        await ctx.RespondAsync($"{ctx.User.Username}#{ctx.User.Discriminator} {city} is not a valid city name, type `.cities` to see a list of available cities.");
                        continue;
                    }

                    var cityRole = ctx.Client.GetRoleFromName(city);
                    if (cityRole == null)
                    {
                        await ctx.RespondEmbed($"{ctx.User.Username}{ctx.User.Discriminator} {city} is not a valid city role name.");
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

                await ctx.RespondEmbed
                (
                    (unassigned.Count > 0
                        ? $"{ctx.User.Username}#{ctx.User.Discriminator} has been removed from role{(unassigned.Count > 1 ? "s" : null)} **{string.Join("**, **", unassigned)}**."
                        : string.Empty) +
                    (alreadyUnassigned.Count > 0
                        ? $"\r\n{ctx.User.Username}#{ctx.User.Discriminator} is not assigned to **{string.Join("**, **", alreadyUnassigned)}** roles{(alreadyUnassigned.Count > 1 ? "s" : null)}."
                        : string.Empty)
                );
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }

        private async Task AssignAllDefaultFeedRoles(CommandContext ctx)
        {
            if (_dep.WhConfig.Servers[ctx.Guild.Id].CityRoles == null)
            {
                _logger.Warn($"City roles empty.");
                return;
            }

            foreach (var city in _dep.WhConfig.Servers[ctx.Guild.Id].CityRoles)
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
            foreach (var city in _dep.WhConfig.Servers[ctx.Guild.Id].CityRoles)
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