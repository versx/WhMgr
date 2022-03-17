namespace WhMgr.Services.Discord
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using DSharpPlus;
    using DSharpPlus.CommandsNext;
    using DSharpPlus.Entities;
    using DSharpPlus.Interactivity;
    using DSharpPlus.Interactivity.Extensions;

    using WhMgr.Commands.Discord;
    using WhMgr.Configuration;
    using WhMgr.Extensions;

    public class DiscordClientFactory
    {
        public static DiscordClient CreateDiscordClient(DiscordServerConfig config, IServiceProvider services)
        {
            if (string.IsNullOrEmpty(config?.Bot?.Token))
            {
                throw new NullReferenceException("DiscordClient bot token must be set!");
            }
            config.Subscriptions?.LoadDmEmbeds();
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
            client.UseInteractivity(new InteractivityConfiguration
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
            // Register available Discord command handler classes
            commands.RegisterCommands<Nests>();
            commands.RegisterCommands<DailyStats>();
            commands.RegisterCommands<Quests>();
            if (config.Subscriptions?.Enabled ?? false)
            {
                commands.RegisterCommands<Notifications>();
            }
            if (config.GeofenceRoles?.Enabled ?? false)
            {
                // Add assignable Discord roles and listing command
                commands.RegisterCommands<Feeds>();
            }
            else
            {
                // Add basic area listing command
                commands.RegisterCommands<Areas>();
            }
            /*
            commands.RegisterCommands<Owner>();
            commands.RegisterCommands<Event>();
            commands.RegisterCommands<Gyms>();
            commands.RegisterCommands<Settings>();
            */
            commands.CommandExecuted += Commands_CommandExecuted;
            commands.CommandErrored += Commands_CommandErrored;
            return client;
        }

        private static async Task Commands_CommandExecuted(CommandsNextExtension commands, CommandExecutionEventArgs e)
        {
            // let's log the name of the command and user
            Console.WriteLine($"{e.Context.User.Username} successfully executed '{e.Command.QualifiedName}'", DateTime.Now);

            // since this method is not async, let's return
            // a completed task, so that no additional work
            // is done
            await Task.CompletedTask;
        }

        private static async Task Commands_CommandErrored(CommandsNextExtension commands, CommandErrorEventArgs e)
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
                var config = (ConfigHolder)commands.Services.GetService(typeof(ConfigHolder));
                var arguments = e.Command.Overloads[0];
                // The user lacks required permissions, 
                var emoji = DiscordEmoji.FromName(e.Context.Client, ":x:");

                var guildId = e.Context.Guild?.Id ?? e.Context.Client.Guilds.FirstOrDefault(x => config.Instance.Servers.ContainsKey(x.Key)).Key;
                var prefix = config.Instance.Servers.ContainsKey(guildId) ? config.Instance.Servers[guildId].Bot.CommandPrefix : "!";
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
    }
}