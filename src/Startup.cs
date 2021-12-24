namespace WhMgr
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.OpenApi.Models;

    using WhMgr.Configuration;
    using WhMgr.Data.Contexts;
    using WhMgr.Extensions;
    using WhMgr.HostedServices;
    using WhMgr.HostedServices.TaskQueue;
    using WhMgr.Localization;
    using WhMgr.Services;
    using WhMgr.Services.Alarms;
    using WhMgr.Services.Alarms.Models;
    using WhMgr.Services.Cache;
    using WhMgr.Services.Discord;
    using WhMgr.Services.Geofence;
    using WhMgr.Services.Subscriptions;
    using WhMgr.Services.Webhook;

    using QuestRewardType = POGOProtos.Rpc.QuestRewardProto.Types.Type;

    // TODO: Reload alarms/filters/geofences on change
    // TODO: Simplify alarm and subscription filter checks
    // TODO: Allow pokemon names and ids for pokemon/raid alarm filters

    public class Startup
    {
        private readonly Dictionary<ulong, ChannelAlarmsManifest> _alarms;
        private readonly ConfigHolder _config;

        public IConfiguration Configuration { get; }

        public static Config Config { get; set; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            _config = new ConfigHolder(Config);
            _config.Reloaded += () =>
            {
                Console.WriteLine($"Config file reloaded!");
                _config.Instance.LoadDiscordServers();
                // TODO: _alarms = ChannelAlarmsManifest.LoadAlarms(config.Servers);
                // TODO: filters and embeds
            };
            _alarms = ChannelAlarmsManifest.LoadAlarms(Config.Servers);

            // Create locale translation files
            Translator.Instance.CreateLocaleFiles();
            Translator.Instance.SetLocale(_config.Instance.Locale);
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IGeofenceService>(new GeofenceService());
            services.AddSingleton<IAlarmControllerService, AlarmControllerService>();
            //services.AddSingleton<ISubscriptionProcessorQueueService, SubscriptionProcessorQueueService>();
            services.AddSingleton<ISubscriptionProcessorService, SubscriptionProcessorService>();
            services.AddSingleton<ISubscriptionManagerService, SubscriptionManagerService>();
            services.AddSingleton<IWebhookProcessorService, WebhookProcessorService>();
            services.AddSingleton<ChannelAlarmsManifest, ChannelAlarmsManifest>();
            services.AddSingleton(_config);
            services.AddSingleton<IReadOnlyDictionary<ulong, ChannelAlarmsManifest>>(_alarms);
            services.AddSingleton<IMapDataCache, MapDataCache>();
            services.AddSingleton<IStaticsticsService, StatisticsService>();
            services.AddSingleton<IDiscordClientService, DiscordClientService>();
            //services.AddSingleton<IconStyleCollection>();
            //services.AddSingleton<Dictionary<QuestRewardType, string>>();
            //services.AddSingleton<IUIconService, UIconService>();

            services.AddHostedService<SubscriptionProcessorService>();
            // Subscription processor queue
            services.AddSingleton<IBackgroundTaskQueue>(_ =>
            {
                // TODO: Get max subscription queue capacity config value
                var maxQueueCapacity = 500;
                return new DefaultBackgroundTaskQueue(maxQueueCapacity);
            });

            services.AddHostedService<QuestPurgeHostedService>();

            var mainConnectionString = _config.Instance.Database.Main.ToString();
            var scannerConnectionString = _config.Instance.Database.Scanner.ToString();
            var nestsConnectionString = _config.Instance.Database.Nests.ToString();

            services.AddDatabase<AppDbContext>(mainConnectionString);
            services.AddDatabase<MapDbContext>(scannerConnectionString);
            services.AddDatabase<ManualDbContext>(nestsConnectionString);

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