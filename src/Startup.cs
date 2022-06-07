namespace WhMgr
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    using HealthChecks.UI.Client;
    using Microsoft.AspNetCore.Antiforgery;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Diagnostics.HealthChecks;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.SpaServices;
    using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.FileProviders;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.OpenApi.Models;

    using WhMgr.Configuration;
    using WhMgr.Data.Contexts;
    using WhMgr.Extensions;
    using WhMgr.HostedServices;
    using WhMgr.HostedServices.TaskQueue;
    using WhMgr.IO;
    using WhMgr.Localization;
    using WhMgr.Services;
    using WhMgr.Services.Alarms;
    using WhMgr.Services.Alarms.Models;
    using WhMgr.Services.Cache;
    using WhMgr.Services.Discord;
    using WhMgr.Services.Geofence;
    using WhMgr.Services.Subscriptions;
    using WhMgr.Services.Webhook;
    using WhMgr.Services.Webhook.Queue;
    using WhMgr.Web.Extensions;
    using WhMgr.Web.Filters;
    using WhMgr.Web.Middleware;
    
    // TODO: Reload embeds and filters on change
    // TODO: Simplify alarm and subscription filter checks
    // TODO: Allow pokemon names and ids for pokemon/raid alarm filters

    public class Startup
    {
        private IReadOnlyDictionary<ulong, ChannelAlarmsManifest> _alarms;
        private readonly ConfigHolder _config;

        public IConfiguration Configuration { get; }

        public static Config Config { get; set; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            _config = new ConfigHolder(Config);
            _config.Reloaded += () =>
            {
                Console.WriteLine($"Config file '{Config.FileName}' reloaded!");
                Console.WriteLine($"Reloading Discord servers config...");
                _config.Instance.LoadDiscordServers();
                Console.WriteLine($"Reloading Discord server geofences...");
                foreach (var (discordId, discordConfig) in _config.Instance.Servers)
                {
                    discordConfig.LoadGeofences();
                }
                // TODO: filters and embeds
                Console.WriteLine($"Reloading Discord server alarms...");
                _alarms = ChannelAlarmsManifest.LoadAlarms(Config.Servers);
            };
            var fullPath = Path.GetFullPath(_config.Instance.FileName);
            var configWatcher = new FileWatcher(fullPath);
            configWatcher.Changed += (sender, e) => _config.Instance = Config.Load(e.FullPath);
            configWatcher.Start();

            _alarms = ChannelAlarmsManifest.LoadAlarms(Config.Servers);

            // Create locale translation files
            try
            {
                Translator.Instance.CreateLocaleFiles().ConfigureAwait(false).GetAwaiter().GetResult();
                Translator.Instance.SetLocale(_config.Instance.Locale);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to generate locale files, make sure the base locales exist: {ex}");
            }
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHealthChecks()
                .AddMySql(Config.Database.Main.ToString(), "Subscriptions Database")
                .AddMySql(Config.Database.Scanner.ToString(), "Scanner Database")
                .AddMySql(Config.Database.Nests.ToString(), "Nests Database")
                .AddProcessHealthCheck(Process.GetCurrentProcess().ProcessName, p => p.Length >= 1, "Process")
                .AddProcessAllocatedMemoryHealthCheck((int)Environment.WorkingSet, "Allocated Memory")
                .AddDiskStorageHealthCheck(setup =>
                {
                    DriveInfo.GetDrives()
                             .Where(drive => drive.IsReady && drive.DriveType == DriveType.Fixed)
                             .ToList()
                             .ForEach(drive => setup.AddDrive(drive.RootDirectory.FullName));
                }, "Local Disk Storage")
                //.AddDnsResolveHealthCheck(setup => setup.ResolveHost("https://google.com"))
                .AddPingHealthCheck(setup => setup.AddHost("discord.com", 10), "Discord Status");

            services.AddHealthChecksUI(settings =>
            {
                settings.AddHealthCheckEndpoint("Main Health Check", "/health");
                settings.MaximumHistoryEntriesPerEndpoint(50);
            })
                .AddInMemoryStorage();

            services.AddSingleton<IGeofenceService>(new GeofenceService());
            services.AddSingleton<IAlarmControllerService, AlarmControllerService>();
            //services.AddSingleton<ISubscriptionProcessorQueueService, SubscriptionProcessorQueueService>();
            services.AddSingleton<ISubscriptionProcessorService, SubscriptionProcessorService>();
            services.AddSingleton<ISubscriptionManagerService, SubscriptionManagerService>();
            services.AddSingleton<IWebhookProcessorService, WebhookProcessorService>();
            services.AddSingleton<IWebhookQueueManager, WebhookQueueManager>();
            services.AddSingleton<ChannelAlarmsManifest, ChannelAlarmsManifest>();
            services.AddSingleton(_config);
            services.AddSingleton<IReadOnlyDictionary<ulong, ChannelAlarmsManifest>>(_alarms);
            services.AddSingleton<IMapDataCache, MapDataCache>();
            services.AddSingleton<IStaticsticsService, StatisticsService>();
            services.AddSingleton<IDiscordClientService, DiscordClientService>();

            services.AddHostedService<SubscriptionProcessorService>();
            // Subscription processor queue
            // TODO: Use scoped background services
            services.AddSingleton<IBackgroundTaskQueue>(_ =>
            {
                // TODO: Get max subscription queue capacity config value
                var maxQueueCapacity = 2048;
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

            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(60);
                options.Cookie.Name = "whmgr.session";
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            // Add csrf middleware
            services.AddAntiforgery(options =>
            {
                // Set Cookie properties using CookieBuilder properties.
                options.FormFieldName = "csrf-token";
                options.HeaderName = "X-CSRF-TOKEN-WHMGR";
                options.SuppressXFrameOptionsHeader = false;
            });

            // Cross origin resource sharing configuration
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyHeader()
                           .AllowAnyMethod();
                });
            });

            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = Strings.ClientBuildFolder;
            });

            //services.AddControllers();
            services.AddControllers(options => options.Filters.Add<LogRequestTimeFilterAttribute>());
            services.AddControllersWithViews();

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
            app.UseMiddleware<RequestsMiddleware>();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "WhMgr v1"));
            }
            else
            {
                app.UseExceptionHandler("/error");
                app.UseHsts();
            }

            // Initialize and start Discord clients
            Task.Run(async () => await discordClientService.Start());

            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(
                    Path.Combine(env.WebRootPath, "static")),
                //RequestPath = ""
            });

            app.UseCors();

            // Initialize Spa for React app
            app.UseSpaStaticFiles();
            if (env.IsDevelopment())
            {
                app.MapWhen(y => y.Request.Path.StartsWithSegments(Strings.AdminDashboardEndpoint), client =>
                {
                    client.UseSpa(spa =>
                    {
                        spa.Options.SourcePath = Strings.ClientBuildFolder;
                        spa.UseReactDevelopmentServer(npmScript: "start");
                    });
                });
            }
            else
            {
                app.Map(new PathString(Strings.AdminDashboardEndpoint), client =>
                {
                    client.UseSpaStaticFiles();
                    client.UseSpa(spa => { });
                });
            }

            app.UseStaticFiles();
            app.UseRouting();
            if (Config.EnableSentry)
            {
                app.UseSentryTracing();
            }
            app.UseAuthorization();

            app.UseSession();

            // TODO: if (config.Discord.Enabled)
            // TODO: app.UseMiddleware<DiscordAuthMiddleware>();
            // TODO: app.UseMiddleware<UserPassportMiddleware>();

            /*
            // Anti forgery middleware using csrf tokens
            var antiforgery = app.ApplicationServices.GetRequiredService<IAntiforgery>();
            app.Use((context, next) =>
            {
                var requestPath = context.Request.Path.Value;
                if (string.Equals(requestPath, "/dashboard", StringComparison.OrdinalIgnoreCase))
                {
                    var tokenSet = antiforgery.GetAndStoreTokens(context);
                    context.Response.Cookies.Append(
                        "XSRF-TOKEN",
                        tokenSet.RequestToken!,
                        new CookieOptions { HttpOnly = false }
                    );
                }
                return next(); // context);
            });
            */

            //app.UseCsrfTokens();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();

                // Register health check backend endpoint path
                endpoints.MapHealthChecks("/health", new HealthCheckOptions
                {
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
                });
                // Register halth check frontend UI path
                endpoints.MapHealthChecksUI(opt =>
                {
                    opt.UIPath = "/health-ui";
                    opt.ResourcesPath = "/health";
                });
            });

            // Initialize webhook processor service
            while (!discordClientService.Initialized)
            {
                System.Threading.Thread.Sleep(50);
            }
            webhookProcessorService.Start();
        }
    }
}
