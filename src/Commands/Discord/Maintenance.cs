namespace WhMgr.Commands.Discord
{
    using System.Linq;
    using System.Threading.Tasks;

    using DSharpPlus;
    using DSharpPlus.CommandsNext;
    using DSharpPlus.CommandsNext.Attributes;

    using WhMgr.Configuration;
    using WhMgr.Extensions;
    using WhMgr.Localization;
    using WhMgr.Services.Subscriptions;
    using WhMgr.Services.Subscriptions.Models;

    public class Maintenance : BaseCommandModule
    {
        private readonly ConfigHolder _config;
        private readonly Microsoft.Extensions.Logging.ILogger _logger;
        private readonly ISubscriptionManagerService _subManager;

        public Maintenance(
            ConfigHolder config,
            Microsoft.Extensions.Logging.ILoggerFactory loggerFactory,
            ISubscriptionManagerService subManager)
        {
            _config = config;
            _logger = loggerFactory.CreateLogger(typeof(Maintenance).FullName);
            _subManager = subManager;
        }

        [
            Command("clean-departed"),
            Description("Remove user subscriptions that are no longer donors from the database. Specify whether it's a dry run and if non-donor subscriptions should be set to disabled rather than deleted completely."),
            Hidden,
            RequireUserPermissions(Permissions.KickMembers, false),
        ]
        public async Task CleanDepartedAsync(CommandContext ctx,
            [Description("Only Disable: Use `true` to only disable non-donor subscriptions, otherwise they will be deleted.")]
            bool onlyDisable = true)
        {
            _logger.Debug($"Checking if there are any subscriptions for members that are no longer apart of the server...");

            var guildId = ctx.Guild?.Id ?? ctx.Client.Guilds.Keys.FirstOrDefault(x => _config.Instance.Servers.ContainsKey(x));
            var removed = 0;
            var users = _subManager?.Subscriptions;

            if (users.Count == 0)
            {
                await ctx.RespondEmbedAsync($"No user subscriptions for '{guildId}', unable to clean subscriptions for users.");
                return;
            }

            if (!_config.Instance.Servers.ContainsKey(guildId))
            {
                await ctx.RespondEmbedAsync($"Guild '{guildId}' not configured, unable to remove user area roles.");
                return;
            }

            var guildConfig = _config.Instance.Servers[guildId];
            var revokeReason = "No longer server donor";
            _logger.Information($"Starting expired donor subscriptions cleanup.");

            for (var i = 0; i < users.Count; i++)
            {
                var user = users[i];
                var discordMember = await ctx.Client.GetMemberByIdAsync(guildId, user.UserId);
                var donorRoleIds = guildConfig.DonorRoleIds.Keys.ToList();
                var isDonor = ctx.Client.HasSupporterRole(guildId, user.UserId, donorRoleIds);
                if (discordMember == null)
                {
                    // No longer in the guild, completely remove subscriptions
                    _logger.Debug($"User is no longer in guild '{guildId}', removing all user subscriptions...");
                    await _subManager.RemoveAllUserSubscriptionsAsync(user.Id);
                    _logger.Information($"Removed all {user.UserId} subscriptions for guild '{guildId}'.");

                    removed++;
                }

                if (discordMember != null && !isDonor)
                {
                    // No longer a donor/supporter, remove/disable subscriptions and geofence/area roles assigned
                    if (onlyDisable)
                    {
                        await _subManager.SetSubscriptionStatusAsync(user.Id, NotificationStatusType.None);
                    }
                    else
                    {
                        await _subManager.RemoveAllUserSubscriptionsAsync(user.Id);
                    }
                    _logger.Information($"{(onlyDisable ? "Disabled" : "Removed")} all {user.UserId} subscriptions for guild '{guildId}'.");

                    removed++;

                    // Remove any assigned area/geofence roles from the Discord member if
                    // the geofence roles config option is enabled as well as the auto remove
                    // and requires donor role options.
                    if ((guildConfig.GeofenceRoles?.Enabled ?? false) &&
                        (guildConfig.GeofenceRoles?.AutoRemove ?? false) &&
                        (guildConfig.GeofenceRoles?.RequiresDonorRole ?? false))
                    {
                        // Skip users without any roles assigned
                        if (!discordMember.Roles.Any())
                            continue;

                        // Compose list of user's role names and guild's area/geofence role names to compare
                        var userRoleNames = discordMember.Roles.Select(role => role.Name.ToLower())
                                                               .ToList();
                        var areaRoleNames = guildConfig.Geofences.Select(geofence => geofence.Name.ToLower())
                                                                 .ToList();
                        // Check if user has any area roles assigned to remove
                        if (!userRoleNames.Exists(userRoleName => areaRoleNames.Contains(userRoleName)))
                            continue;

                        // Remove any assigned area roles from user
                        foreach (var areaRoleName in areaRoleNames)
                        {
                            var memberRole = discordMember.Roles.FirstOrDefault(role => string.Compare(role.Name, areaRoleName, true) == 0);
                            var memberTag = $"{discordMember.Username}#{discordMember.Discriminator} ({discordMember.Id})";
                            if (memberRole == null)
                            {
                                _logger.Warning($"Failed to get discord member '{memberTag}' role by name '{areaRoleName}', skipping area role.");
                                continue;
                            }

                            // Removing role from user
                            await discordMember.RevokeRoleAsync(memberRole, revokeReason);
                            _logger.Debug($"Removed role '{memberRole.Name} ({memberRole.Id})' from user '{memberTag}' in guild '{ctx.Guild?.Name} ({guildId})'");
                        }
                    }
                }
            }

            _logger.Information($"Finished cleaning expired donor subscriptions. Removed donor access from {removed:N0} of {users:N0} total members in guild '{ctx.Guild?.Name}' ({guildId})");

            await ctx.RespondEmbedAsync(Translator.Instance.Translate("REMOVED_TOTAL_DEPARTED_MEMBERS").FormatText(new
            {
                removed = removed.ToString("N0"),
                users = users.Count.ToString("N0"),
            }));
        }
    }
}