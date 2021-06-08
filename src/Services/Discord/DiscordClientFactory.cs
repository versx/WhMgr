namespace WhMgr.Services.Discord
{
    using System;

    using DSharpPlus;
    using DSharpPlus.CommandsNext;
    using DSharpPlus.Entities;
    using DSharpPlus.Interactivity;
    using DSharpPlus.Interactivity.Extensions;

    using WhMgr.Configuration;

    public class DiscordClientFactory
    {
        public static DiscordClient CreateDiscordClient(DiscordServerConfig config, IServiceProvider services)
        {
            // TODO: config.LoadDmAlerts();
            var client = new DiscordClient(new DiscordConfiguration
            {
                AutoReconnect = true,
                AlwaysCacheMembers = true,
                GatewayCompressionLevel = GatewayCompressionLevel.Payload,
                Token = config.Bot?.Token,
                TokenType = TokenType.Bot,
                MinimumLogLevel = Microsoft.Extensions.Logging.LogLevel.Error,
                Intents = DiscordIntents.DirectMessages
                    | DiscordIntents.DirectMessageTyping
                    | DiscordIntents.GuildEmojis
                    | DiscordIntents.GuildMembers
                    | DiscordIntents.GuildMessages
                    | DiscordIntents.GuildMessageTyping
                    | DiscordIntents.GuildPresences
                    | DiscordIntents.Guilds
                    | DiscordIntents.GuildWebhooks,
                ReconnectIndefinitely = true,
            });

            // Configure Discord interactivity module
            var interactivity = client.UseInteractivity(new InteractivityConfiguration
            {
                PollBehaviour = DSharpPlus.Interactivity.Enums.PollBehaviour.KeepEmojis,
                Timeout = TimeSpan.FromSeconds(30),
                PaginationBehaviour = DSharpPlus.Interactivity.Enums.PaginationBehaviour.WrapAround,
            });

            // Discord commands configuration
            var commands = client.UseCommandsNext
            (
                new CommandsNextConfiguration
                {
                    StringPrefixes = new[] { config.Bot?.CommandPrefix?.ToString() },
                    EnableDms = true,
                    // If command prefix is null, allow for mention prefix
                    EnableMentionPrefix = string.IsNullOrEmpty(config.Bot?.CommandPrefix),
                    // Use DSharpPlus's built-in help formatter
                    EnableDefaultHelp = true,
                    CaseSensitive = false,
                    IgnoreExtraArguments = true,
                    Services = services,
                }
            );
            /*
            commands.CommandExecuted += Commands_CommandExecuted;
            commands.CommandErrored += Commands_CommandErrored;
            // Register Discord command handler classes
            commands.RegisterCommands<Owner>();
            commands.RegisterCommands<Event>();
            commands.RegisterCommands<Nests>();
            commands.RegisterCommands<ShinyStats>();
            commands.RegisterCommands<Gyms>();
            commands.RegisterCommands<Quests>();
            commands.RegisterCommands<Settings>();
            if (config.Subscriptions.Enabled)
            {
                commands.RegisterCommands<Notifications>();
            }
            if (config.EnableGeofenceRoles)
            {
                commands.RegisterCommands<Feeds>();
            }
            else
            {
                commands.RegisterCommands<Areas>();
            }
            */
            return client;
        }

        /*
        private async Task Commands_CommandExecuted(CommandsNextExtension commands, CommandExecutionEventArgs e)
        {
            // let's log the name of the command and user
            Console.WriteLine($"{e.Context.User.Username} successfully executed '{e.Command.QualifiedName}'", DateTime.Now);

            // since this method is not async, let's return
            // a completed task, so that no additional work
            // is done
            await Task.CompletedTask;
        }

        private async Task Commands_CommandErrored(CommandsNextExtension commands, CommandErrorEventArgs e)
        {
            Console.WriteLine($"{e.Context.User.Username} tried executing '{e.Command?.QualifiedName ?? e.Context.Message.Content}' but it errored: {e.Exception.GetType()}: {e.Exception.Message ?? "<no message>"}", DateTime.Now);

            // let's check if the error is a result of lack of required permissions
            if (e.Exception is DSharpPlus.CommandsNext.Exceptions.ChecksFailedException)
            {
                // The user lacks required permissions, 
                var emoji = DiscordEmoji.FromName(e.Context.Client, ":no_entry:");

                // let's wrap the response into an embed
                var embed = new DiscordEmbedBuilder
                {
                    Title = "Access denied",
                    Description = $"{emoji} You do not have the permissions required to execute this command.",
                    Color = new DiscordColor(0xFF0000) // red
                };
                await e.Context.RespondAsync(embed: embed);
            }
            else if (e.Exception is ArgumentException)
            {
                var arguments = e.Command.Overloads.FirstOrDefault();
                // The user lacks required permissions, 
                var emoji = DiscordEmoji.FromName(e.Context.Client, ":x:");

                var guildId = e.Context.Guild?.Id ?? e.Context.Client.Guilds.FirstOrDefault(x => _config.Instance.Servers.ContainsKey(x.Key)).Key;
                var prefix = _config.Instance.Servers.ContainsKey(guildId) ? _config.Instance.Servers[guildId].CommandPrefix : "!";
                //var example = $"Command Example: ```{prefix}{e.Command.Name} {string.Join(" ", e.Command.Arguments.Select(x => x.IsOptional ? $"[{x.Name}]" : x.Name))}```\r\n*Parameters in brackets are optional.*";
                var example = $"Command Example: ```{prefix}{e.Command.Name} {string.Join(" ", arguments.Arguments.Select(x => x.IsOptional ? $"[{x.Name}]" : x.Name))}```\r\n*Parameters in brackets are optional.*";

                // let's wrap the response into an embed
                var embed = new DiscordEmbedBuilder
                {
                    Title = $"{emoji} Invalid Argument(s)",
                    //Description = $"{string.Join(Environment.NewLine, e.Command.Arguments.Select(x => $"Parameter **{x.Name}** expects type **{x.Type.ToHumanReadableString()}.**"))}.\r\n\r\n{example}",
                    Description = $"{string.Join(Environment.NewLine, arguments.Arguments.Select(x => $"Parameter **{x.Name}** expects type **{x.Type.ToHumanReadableString()}.**"))}.\r\n\r\n{example}",
                    Color = new DiscordColor(0xFF0000) // red
                };
                await e.Context.RespondAsync(embed: embed);
            }
            else if (e.Exception is DSharpPlus.CommandsNext.Exceptions.CommandNotFoundException)
            {
                Console.WriteLine($"User {e.Context.User.Username} tried executing command {e.Context.Message.Content} but command does not exist.");
            }
            else
            {
                Console.WriteLine($"User {e.Context.User.Username} tried executing command {e.Command?.Name} and unknown error occurred.\r\n: {e.Exception}");
            }
        }
        */
    }
}