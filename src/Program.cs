namespace WhMgr
{
    using System;
    using System.IO;

    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    using WhMgr.Configuration;

    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    var configPath = Path.Combine(
                        Environment.CurrentDirectory,
                        Strings.BasePath + Strings.ConfigFileName
                    );
                    var config = Config.Load(configPath);
                    if (config == null)
                    {
                        Console.WriteLine($"Failed to load config {configPath}.");
                        return;
                    }
                    config.FileName = configPath;
                    config.LoadDiscordServers();
                    Startup.Config = config;
                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseUrls($"http://*:{config.WebhookPort}");
                });
    }
}