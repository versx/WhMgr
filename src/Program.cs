namespace WhMgr
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    using CommandLine;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    using WhMgr.Configuration;

    public class Options
    {
        [Option('c', "config", Required = false, HelpText = "Set config file to use.")]
        public string ConfigFileName { get; set; }

        [Option('n', "name", Required = false, HelpText = "Set name of instance.")]
        public string InstanceName { get; set; }
    }

    public class Program
    {
        private static CancellationTokenSource _cts = new();
        private static string[] _args;
        private static bool _restartRequest;

        public static void Main(string[] args)
        {
            _args = args;

            Start();
            while (_restartRequest)
            {
                _restartRequest = false;
                Console.WriteLine("Restarting application...");
                Start();
            }
            //CreateHostBuilder(args).Build().Run();
        }

        public static void Restart()
        {
            _restartRequest = true;
            _cts.Cancel();
        }

        private static void Start()
        {
            try
            {
                _cts = new CancellationTokenSource();
                CreateHostBuilder(_args).Build()
                                        .RunAsync(_cts.Token)
                                        .ConfigureAwait(false)
                                        .GetAwaiter()
                                        .GetResult();
            }
            catch (OperationCanceledException ex)
            {
                Console.WriteLine(ex);
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    var arguments = Parser.Default.ParseArguments<Options>(args)
                        .WithParsed(options =>
                        {
                            var instanceName = options.InstanceName ?? $"{Strings.BotName}_{Guid.NewGuid()}";
                            var configFileName = options.ConfigFileName ?? Strings.ConfigFileName;
                            var configPath = Path.Combine(Strings.ConfigsFolder, configFileName);
                            var config = Config.Load(configPath);
                            if (config == null)
                            {
                                Console.WriteLine($"Failed to load config {configPath}.");
                                return;
                            }
                            // TODO: >.> Need to implement ConfigHolder in Startup instead of just Config
                            //var holder = new ConfigHolder(config);
                            config.FileName = configPath;
                            config.LoadDiscordServers();
                            Startup.Config = config;

                            webBuilder.UseStartup<Startup>();
                            webBuilder.UseUrls($"http://{config.ListeningHost}:{config.WebhookPort}");

                            // Allow safe shutdown of 10 seconds before sigkill
                            webBuilder.UseShutdownTimeout(TimeSpan.FromSeconds(10));

                            // Configure Sentry monitoring and reporting
                            if (config.EnableSentry)
                            {
                                webBuilder.UseSentry(options =>
                                {
                                    options.Dsn = "https://cece44d9799f4009b67ed0702208c0c9@o1113124.ingest.sentry.io/6143193";
                                    //options.ServerName = Strings.BotName;
                                    options.Release = Strings.BotVersion;
                                    options.AutoSessionTracking = true;
                                    options.MaxBreadcrumbs = 200;
                                    options.TracesSampleRate = 0.1;
                                    options.HttpProxy = null;
                                    options.DecompressionMethods = System.Net.DecompressionMethods.None;
                                    options.MaxQueueItems = 100;
                                    options.ShutdownTimeout = TimeSpan.FromSeconds(5);
                                    options.ConfigureScope(scope => scope.SetTag("Started", DateTime.Now.ToString()));
                                });
                            }
                        });
                });
    }
}