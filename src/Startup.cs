namespace WhMgr
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.OpenApi.Models;

    using WhMgr.Configuration;
    using WhMgr.Data.Contexts;
    using WhMgr.HostedServices;
    using WhMgr.Queues;
    using WhMgr.Services;
    using WhMgr.Services.Alarms;
    using WhMgr.Services.Alarms.Models;
    using WhMgr.Services.Cache;
    using WhMgr.Services.Discord;
    using WhMgr.Services.Geofence;
    using WhMgr.Services.Subscriptions;
    using WhMgr.Services.Webhook;
    using WhMgr.Utilities;

    // TODO: Reload alarms/filters/geofences on change
    // TODO: Fix Pvp pokemon name not showing on Pokemon embed
    // TODO: Twilio notifications
    // TODO: HostedService webhook queue
    // TODO: HostedService subscription queue
    // TODO: Simplify alarm and subscription filter checks

    public class Startup
    {
        private readonly Dictionary<ulong, ChannelAlarmsManifest> _alarms;
        private readonly ConfigHolder _config;

        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
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
            _config = new ConfigHolder(config);
            _config.Reloaded += () =>
            {
                _config.Instance.LoadDiscordServers();
                // TODO: _alarms = ChannelAlarmsManifest.LoadAlarms(config.Servers);
                // TODO: filters and embeds
                // TODO: Use FileWatcher
            };
            _alarms = ChannelAlarmsManifest.LoadAlarms(config.Servers);

            // Create locale translation files
            Localization.Translator.Instance.CreateLocaleFiles();
            Localization.Translator.Instance.SetLocale(_config.Instance.Locale);

            IconFetcher.Instance.SetIconStyles(_config.Instance.IconStyles);
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IGeofenceService>(new GeofenceService());
            services.AddSingleton<IAlarmControllerService, AlarmControllerService>();
            services.AddScoped<NotificationQueue, NotificationQueue>();
            services.AddSingleton<ISubscriptionProcessorQueueService, SubscriptionProcessorQueueService>();
            services.AddSingleton<ISubscriptionProcessorService, SubscriptionProcessorService>();
            services.AddSingleton<ISubscriptionManagerService, SubscriptionManagerService>();
            services.AddSingleton<IWebhookProcessorService, WebhookProcessorService>();
            services.AddSingleton<ChannelAlarmsManifest, ChannelAlarmsManifest>();
            services.AddSingleton(_config);
            services.AddSingleton<IReadOnlyDictionary<ulong, ChannelAlarmsManifest>>(_alarms);
            services.AddSingleton<IMapDataCache, MapDataCache>();
            services.AddSingleton<IStaticsticsService, StatisticsService>();
            services.AddSingleton<IDiscordClientService, DiscordClientService>();

            services.AddHostedService<QuestPurgeHostedService>();

            var mainConnectionString = _config.Instance.Database.Main.ToString();
            var scannerConnectionString = _config.Instance.Database.Scanner.ToString();
            var nestsConnectionString = _config.Instance.Database.Nests.ToString();

            services.AddDbContextFactory<AppDbContext>(options =>
                options.UseMySql(
                    mainConnectionString,
                    ServerVersion.AutoDetect(mainConnectionString)
                ), ServiceLifetime.Singleton
            );

            services.AddDbContext<AppDbContext>(options =>
                options.UseMySql(
                    mainConnectionString,
                    ServerVersion.AutoDetect(mainConnectionString)
                ), ServiceLifetime.Scoped
            );

            services.AddDbContextFactory<MapDbContext>(options =>
                options.UseMySql(
                    scannerConnectionString,
                    ServerVersion.AutoDetect(scannerConnectionString)
                ), ServiceLifetime.Singleton
            );

            services.AddDbContext<MapDbContext>(options =>
                options.UseMySql(
                    scannerConnectionString,
                    ServerVersion.AutoDetect(scannerConnectionString)
                ), ServiceLifetime.Scoped
            );

            services.AddDbContextFactory<ManualDbContext>(options =>
                options.UseMySql(
                    nestsConnectionString,
                    ServerVersion.AutoDetect(nestsConnectionString)
                ), ServiceLifetime.Singleton
            );

            services.AddDbContext<ManualDbContext>(options =>
                options.UseMySql(
                    nestsConnectionString,
                    ServerVersion.AutoDetect(nestsConnectionString)
                ), ServiceLifetime.Scoped
            );

            services.AddHealthChecks();

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "WhMgr", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(
            IApplicationBuilder app,
            IWebHostEnvironment env,
            IDiscordClientService discordClientService,
            IWebhookProcessorService webhookProcessorService)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "WhMgr v1"));
            }

            // Initialize and start Discord clients
            Task.Run(async () => await discordClientService.Start());

            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/health");
            });

            // Initialize webhook processor service
            webhookProcessorService.Start();
        }
    }
}