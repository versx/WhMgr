namespace WhMgr.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

    using WhMgr.Configuration;
    using WhMgr.Extensions;
    using WhMgr.Services.Alarms.Embeds;
    using WhMgr.Services.Alarms.Filters.Models;
    using WhMgr.Services.Alarms.Models;

    [
        Controller,
        Route("/dashboard/"),
    ]
    public class AdminDashboardController : Controller
    {
        private readonly ILogger<AdminDashboardController> _logger;

        public AdminDashboardController(ILogger<AdminDashboardController> logger)
        {
            _logger = logger;            
        }

        //[Route("/")]
        [HttpGet]
        public IActionResult Index()
        {
            var obj = new
            {
                template = "dashboard",
                title = "Dashboard",
                favicon = "dotnet.png",
                people = new List<dynamic>
                {
                    new { first = "Jeff", last = "Bezos" },
                    new { first = "Elon", last = "Musk" },
                },
                stats = new
                {
                    configs = Directory.GetFiles(Strings.ConfigsFolder, "*.json").Length,
                    discords = Directory.GetFiles(Strings.DiscordsFolder, "*.json").Length,
                    alarms = Directory.GetFiles(Strings.AlarmsFolder, "*.json").Length,
                    filters = Directory.GetFiles(Strings.FiltersFolder, "*.json").Length,
                    embeds = Directory.GetFiles(Strings.EmbedsFolder, "*.json").Length,
                    geofences = Directory.GetFiles(Strings.GeofencesFolder).Length,
                    roles = 0,
                    templates = 0,
                    users = 0,
                },
            };
            return View("index", obj);
        }

        #region Configs

        [HttpGet]
        [Route("configs")]
        public IActionResult Configs()
        {
            var files = Directory.GetFiles(Strings.ConfigsFolder, "*.json");
            var configs = new Dictionary<string, Config>();
            foreach (var file in files)
            {
                try
                {
                    var name = Path.GetFileNameWithoutExtension(file);
                    var config = LoadFromFile<Config>(file);
                    config.LoadDiscordServers();
                    configs.Add(name, config);
                    Console.WriteLine($"Config: {config.ListeningHost}:{config.WebhookPort}");
                }
                catch (Exception ex)
                {
                    _logger.Error($"Failed to parse config: {file}\nError: {ex}");
                }
            }
            var obj = new
            {
                template = "configs",
                title = "Configs",
                favicon = "dotnet.png",
                configs,
            };
            return View("configs", obj);
        }

        [HttpGet]
        [HttpPost]
        [Route("configs/new")]
        public IActionResult NewConfig()
        {
            if (Request.Method == "GET")
            {
                var obj = new
                {
                    template = "configs-new",
                    title = $"New Config",
                    favicon = "dotnet.png",
                };
                return View("Configs/new", obj);
            }
            else if (Request.Method == "POST")
            {
            }
            return Unauthorized();
        }

        [HttpGet]
        [HttpPost]
        [Route("configs/edit/{fileName}")]
        public IActionResult EditConfig(string fileName)
        {
            if (Request.Method == "GET")
            {
                var obj = new
                {
                    template = "configs-edit",
                    title = $"Edit Config \"{fileName}\"",
                    favicon = "dotnet.png",
                    name = fileName,
                    config = LoadFromFile<Config>(Path.Combine(Strings.ConfigsFolder, fileName + ".json")),
                };
                return View("Configs/edit", obj);
            }
            else if (Request.Method == "POST")
            {
                var obj = new
                {
                };
                return View("Configs/edit", obj);
            }
            return Unauthorized();
        }

        [HttpDelete("configs/delete/{fileName}")]
        public IActionResult DeleteConfig(string fileName)
        {
            var path = Path.Combine(Strings.ConfigsFolder, fileName + ".json");
            try
            {
                System.IO.File.Delete(path);
                return Redirect("configs");
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to delete config: {path}\nError: {ex}");
            }
            return Unauthorized();
        }

        #endregion

        #region Discord Servers

        [HttpGet]
        [Route("discords")]
        public IActionResult Discords()
        {
            var files = Directory.GetFiles(Strings.DiscordsFolder, "*.json");
            var discords = new Dictionary<string, DiscordServerConfig>();
            foreach (var file in files)
            {
                try
                {
                    var name = Path.GetFileNameWithoutExtension(file);
                    var discord = LoadFromFile<DiscordServerConfig>(file);
                    discord.LoadGeofences();
                    discords.Add(name, discord);
                }
                catch (Exception ex)
                {
                    _logger.Error($"Failed to parse discord: {file}\nError: {ex}");
                }
            }
            var obj = new
            {
                template = "discords",
                title = "Discord Servers",
                favicon = "dotnet.png",
                discords,
            };
            return View("discords", obj);
        }

        [HttpGet]
        [HttpPost]
        [Route("discords/new")]
        public IActionResult NewDiscord()
        {
            if (Request.Method == "GET")
            {
                var obj = new
                {
                    template = "discords-mew",
                    title = $"New Discord Server",
                    favicon = "dotnet.png",
                };
                return View("Discords/new", obj);
            }
            else if (Request.Method == "POST")
            {
            }
            return Unauthorized();
        }

        [HttpGet]
        [HttpPost]
        [Route("discords/edit/{fileName}")]
        public IActionResult EditDiscord(string fileName)
        {
            if (Request.Method == "GET")
            {
                var config = LoadFromFile<DiscordServerConfig>(Path.Combine(Strings.DiscordsFolder, fileName + ".json"));
                var alarmFiles = Directory.GetFiles(Strings.AlarmsFolder);
                var geofenceFiles = Directory.GetFiles(Strings.GeofencesFolder);
                var embedFiles = Directory.GetFiles(Strings.EmbedsFolder);
                var iconStyles = Startup.Config.IconStyles;
                var obj = new
                {
                    template = "discords-edit",
                    title = $"Edit Discord Server \"{fileName}\"",
                    favicon = "dotnet.png",
                    name = fileName,
                    config,
                    alarms = alarmFiles.Select(file => Path.GetFileName(file)),
                    geofences = geofenceFiles.Select(file => Path.GetFileName(file)),
                    embeds = embedFiles.Select(file => Path.GetFileName(file)),
                    iconStyles,
                };
                return View("Discords/edit", obj);
            }
            else if (Request.Method == "POST")
            {
                // TODO: Read discord file, deserialize, edit properties, save
                var obj = new
                {
                };
                // TODO: If error show Discords/edit view, otherwise redirect from succussful edit
                return View("Discords/edit", obj);
            }
            return Unauthorized();
        }

        [HttpDelete("discords/delete/{fileName}")]
        public IActionResult DeleteDiscord(string fileName)
        {
            var path = Path.Combine(Strings.DiscordsFolder, fileName + ".json");
            try
            {
                System.IO.File.Delete(path);
                return Redirect("discords");
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to delete discord: {path}\nError: {ex}");
            }
            return Unauthorized();
        }

        #endregion

        #region Alarms

        [HttpGet]
        [Route("alarms")]
        public IActionResult Alarms()
        {
            var files = Directory.GetFiles(Strings.AlarmsFolder, "*.json");
            var alarms = new Dictionary<string, ChannelAlarmsManifest>();
            foreach (var file in files)
            {
                try
                {
                    var name = Path.GetFileNameWithoutExtension(file);
                    var alarm = LoadFromFile<ChannelAlarmsManifest>(file);
                    alarms.Add(name, alarm);
                }
                catch (Exception ex)
                {
                    _logger.Error($"Failed to parse alarm: {file}\nError: {ex}");
                }
            }

            var obj = new
            {
                template = "alarms",
                title = "Channel Alarms",
                favicon = "dotnet.png",
                alarms,
            };
            return View("alarms", obj);
        }

        [HttpGet]
        [HttpPost]
        [Route("alarms/new")]
        public IActionResult NewAlarm()
        {
            if (Request.Method == "GET")
            {
                var obj = new
                {
                    template = "alarms-mew",
                    title = $"New Channel Alarms",
                    favicon = "dotnet.png",
                };
                return View("Alarms/new", obj);
            }
            else if (Request.Method == "POST")
            {
            }
            return Unauthorized();
        }

        [HttpGet]
        [HttpPost]
        [Route("alarms/edit/{fileName}")]
        public IActionResult EditAlarm(string fileName)
        {
            if (Request.Method == "GET")
            {
                var obj = new
                {
                    template = "alarms-edit",
                    title = $"Edit Channel Alarms \"{fileName}\"",
                    favicon = "dotnet.png",
                    name = fileName,
                    alarm = LoadFromFile<ChannelAlarmsManifest>(Path.Combine(Strings.AlarmsFolder, fileName + ".json")),
                };
                return View("Alarms/edit", obj);
            }
            else if (Request.Method == "POST")
            {
                var obj = new
                {
                };
                return View("Alarms/edit", obj);
            }
            return Unauthorized();
        }

        [HttpDelete("alarms/delete/{fileName}")]
        public IActionResult DeleteAlarm(string fileName)
        {
            var path = Path.Combine(Strings.AlarmsFolder, fileName + ".json");
            try
            {
                System.IO.File.Delete(path);
                return Redirect("alarms");
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to delete alarm: {path}\nError: {ex}");
            }
            return Unauthorized();
        }

        #endregion

        #region Filters

        [HttpGet]
        [Route("filters")]
        public IActionResult Filters()
        {
            var files = Directory.GetFiles(Strings.FiltersFolder, "*.json");
            var filters = new Dictionary<string, WebhookFilter>();
            foreach (var file in files)
            {
                try
                {
                    var name = Path.GetFileNameWithoutExtension(file);
                    var filter = LoadFromFile<WebhookFilter>(file);
                    filters.Add(name, filter);
                }
                catch (Exception ex)
                {
                    _logger.Error($"Failed to prase filter: {file}\nError: {ex}");
                }
            }
            var obj = new
            {
                template = "filters",
                title = "Alarm Filters",
                favicon = "dotnet.png",
                filters,
            };
            return View("filters", obj);
        }

        [HttpGet]
        [HttpPost]
        [Route("filters/new")]
        public IActionResult NewFilter()
        {
            if (Request.Method == "GET")
            {
                var obj = new
                {
                    template = "filters-mew",
                    title = $"New Webhook Filter",
                    favicon = "dotnet.png",
                };
                return View("Filters/new", obj);
            }
            else if (Request.Method == "POST")
            {
            }
            return Unauthorized();
        }

        [HttpGet]
        [HttpPost]
        [Route("filters/edit/{fileName}")]
        public IActionResult EditFilter(string fileName)
        {
            if (Request.Method == "GET")
            {
                var obj = new
                {
                    template = "filters-edit",
                    title = $"Edit Webhook Filter \"{fileName}\"",
                    favicon = "dotnet.png",
                    name = fileName,
                    filter = LoadFromFile<WebhookFilter>(Path.Combine(Strings.FiltersFolder, fileName + ".json")),
                };
                return View("Filters/edit", obj);
            }
            else if (Request.Method == "POST")
            {
                var obj = new
                {
                };
                return View("Filters/edit", obj);
            }
            return Unauthorized();
        }

        [HttpDelete("filters/delete/{fileName}")]
        public IActionResult DeleteFilter(string fileName)
        {
            var path = Path.Combine(Strings.FiltersFolder, fileName + ".json");
            try
            {
                System.IO.File.Delete(path);
                return Redirect("filters");
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to delete filter: {path}\nError: {ex}");
            }
            return Unauthorized();
        }

        #endregion

        #region Embeds

        [HttpGet]
        [Route("embeds")]
        public IActionResult Embeds()
        {
            var files = Directory.GetFiles(Strings.EmbedsFolder, "*.json");
            var embeds = new Dictionary<string, EmbedMessage>();
            foreach (var file in files)
            {
                try
                {
                    var name = Path.GetFileNameWithoutExtension(file);
                    var embed = LoadFromFile<EmbedMessage>(file);
                    embeds.Add(name, embed);
                }
                catch (Exception ex)
                {
                    _logger.Error($"Failed to parse embed: {file}\nError: {ex}");
                }
            }
            var obj = new
            {
                template = "embeds",
                title = "Embeds",
                favicon = "dotnet.png",
                embeds,
            };
            return View("embeds", obj);
        }

        [HttpGet]
        [HttpPost]
        [Route("embeds/new")]
        public IActionResult NewEmbed()
        {
            if (Request.Method == "GET")
            {
                var obj = new
                {
                    template = "embeds-mew",
                    title = $"New Message Embed",
                    favicon = "dotnet.png",
                };
                return View("Embeds/new", obj);
            }
            else if (Request.Method == "POST")
            {
            }
            return Unauthorized();
        }

        [HttpGet]
        [HttpPost]
        [Route("embeds/edit/{fileName}")]
        public IActionResult EditEmbed(string fileName)
        {
            if (Request.Method == "GET")
            {
                var obj = new
                {
                    template = "embeds-edit",
                    title = $"Edit Message Embed \"{fileName}\"",
                    favicon = "dotnet.png",
                    name = fileName,
                    embed = LoadFromFile<EmbedMessage>(Path.Combine(Strings.EmbedsFolder, fileName + ".json")),
                };
                return View("Embeds/edit", obj);
            }
            else if (Request.Method == "POST")
            {
                var obj = new
                {
                };
                return View("Embeds/edit", obj);
            }
            return Unauthorized();
        }

        [HttpDelete("embeds/delete/{fileName}")]
        public IActionResult DeleteEmbed(string fileName)
        {
            var path = Path.Combine(Strings.EmbedsFolder, fileName + ".json");
            try
            {
                System.IO.File.Delete(path);
                return Redirect("embeds");
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to delete embed: {path}\nError: {ex}");
            }
            return Unauthorized();
        }

        #endregion

        #region Geofences

        [HttpGet]
        [Route("geofences")]
        public IActionResult Geofences()
        {
            var files = Directory.GetFiles(Strings.GeofencesFolder);
            var obj = new
            {
                template = "geofences",
                title = "Geofences",
                favicon = "dotnet.png",
                files = files.Select(file => Path.GetFileName(file)),
            };
            return View("geofences", obj);
        }

        [HttpGet]
        [HttpPost]
        [Route("geofences/new")]
        public IActionResult NewGeofence()
        {
            if (Request.Method == "GET")
            {
                var obj = new
                {
                    template = "geofences-mew",
                    title = $"New Geofence",
                    favicon = "dotnet.png",
                };
                return View("Geofences/new", obj);
            }
            else if (Request.Method == "POST")
            {
            }
            return Unauthorized();
        }

        [HttpGet]
        [HttpPost]
        [Route("geofences/edit/{fileName}")]
        public IActionResult EditGeofence(string fileName)
        {
            if (Request.Method == "GET")
            {
                var obj = new
                {
                    template = "geofences-edit",
                    title = $"Edit Geofence \"{fileName}\"",
                    favicon = "dotnet.png",
                    name = fileName,
                    geofence = LoadFromFile(Path.Combine(Strings.GeofencesFolder, fileName)),
                };
                return View("Geofences/edit", obj);
            }
            else if (Request.Method == "POST")
            {
                var obj = new
                {
                };
                return View("Geofences/edit", obj);
            }
            return Unauthorized();
        }

        [HttpDelete("geofences/delete/{fileName}")]
        public IActionResult DeleteGeofence(string fileName)
        {
            var path = Path.Combine(Strings.GeofencesFolder, fileName + ".json");
            try
            {
                System.IO.File.Delete(path);
                return Redirect("geofences");
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to delete geofence: {path}\nError: {ex}");
            }
            return Unauthorized();
        }

        #endregion

        #region Discord Roles

        [HttpGet]
        [Route("roles")]
        public IActionResult DiscordRoles()
        {
            var obj = new
            {
                template = "roles",
                title = "Discord Roles",
                favicon = "dotnet.png",
            };
            return View("roles", obj);
        }

        #endregion

        #region Users

        [HttpGet]
        [Route("users")]
        public IActionResult Users()
        {
            var obj = new
            {
                template = "users",
                title = "Users",
                favicon = "dotnet.png",
            };
            return View("users", obj);
        }

        #endregion

        #region Settings

        [HttpGet]
        [Route("settings")]
        public IActionResult Settings()
        {
            var obj = new
            {
                template = "settings",
                title = "Settings",
                favicon = "dotnet.png",
            };
            return View("settings", obj);
        }

        #endregion

        #region Helpers

        private static T LoadFromFile<T>(string filePath)
        {
            var json = System.IO.File.ReadAllText(filePath);
            var filter = json.FromJson<T>();
            return filter;
        }

        private static string LoadFromFile(string filePath)
        {
            var data = System.IO.File.ReadAllText(filePath);
            return data;
        }

        #endregion
    }
}