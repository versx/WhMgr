namespace WhMgr
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.Tracing;
    using System.IO;
    using System.Threading.Tasks;

    using HandlebarsDotNet.ViewEngine;
    using Microsoft.AspNetCore.Antiforgery;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Http.Extensions;
    using Microsoft.AspNetCore.Mvc.Filters;
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
    using WhMgr.Web.Middleware;

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
                Console.WriteLine($"Config file '{Config.FileName}' reloaded!");
                _config.Instance.LoadDiscordServers();
                // TODO: filters and embeds
            };
            var configWatcher = new FileWatcher(_config.Instance.FileName);
            configWatcher.Changed += (sender, e) => _config.Instance = Config.Load(e.FullPath);
            configWatcher.Start();

            _alarms = ChannelAlarmsManifest.LoadAlarms(Config.Servers);

            // Create locale translation files
            Translator.Instance.CreateLocaleFiles().ConfigureAwait(false).GetAwaiter().GetResult();
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

            services.AddMvc()
                    .AddHandlebars(options =>
                    {
                        // Views/Shared/layout.hbs
                        options.DefaultLayout = "Views/Layout/default.hbs";
                        options.RegisterHelpers = TemplateRenderer.GetHelpers();
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

            app.UseRouting();
            app.UseSentryTracing();
            app.UseAuthorization();

            app.UseSession();

            // TODO: if (config.Discord.Enabled)
            //app.UseMiddleware<DiscordAuthMiddleware>();
            //app.UseMiddleware<UserPassportMiddleware>();

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
                endpoints.MapHealthChecks("/health");
            });

            // Initialize webhook processor service
            while (!discordClientService.Initialized)
            {
                System.Threading.Thread.Sleep(50);
            }
            webhookProcessorService.Start();
        }
    }

    public class LogRequestTimeFilterAttribute : ActionFilterAttribute
    {
        private readonly Stopwatch _stopwatch = new();

        public override void OnActionExecuting(ActionExecutingContext context) => _stopwatch.Start();

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            _stopwatch.Stop();

            MinimalEventCounterSource.Log.Request(
                context.HttpContext.Request.GetDisplayUrl(),
                _stopwatch.ElapsedMilliseconds
            );
        }
    }

    [EventSource(Name = "Sample.EventCounter.Minimal")]
    public sealed class MinimalEventCounterSource : EventSource
    {
        public static readonly MinimalEventCounterSource Log = new();

        private EventCounter _requestCounter;

        private MinimalEventCounterSource() =>
            _requestCounter = new EventCounter("request-time", this)
            {
                DisplayName = "Request Processing Time",
                DisplayUnits = "ms"
            };

        public void Request(string url, long elapsedMilliseconds)
        {
            WriteEvent(1, url, elapsedMilliseconds);
            Console.WriteLine($"Request {url} time elapsed: {elapsedMilliseconds} ms");
            _requestCounter?.WriteMetric(elapsedMilliseconds);
        }

        protected override void Dispose(bool disposing)
        {
            _requestCounter?.Dispose();
            _requestCounter = null;

            base.Dispose(disposing);
        }
    }
}