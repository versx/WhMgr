namespace WhMgr.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.Json.Serialization;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

    using WhMgr.Configuration;
    using WhMgr.Extensions;
    using WhMgr.Services.Alarms.Embeds;
    using WhMgr.Services.Alarms.Filters.Models;
    using WhMgr.Services.Alarms.Models;
    using WhMgr.Services.Geofence;
    using WhMgr.Utilities;

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
        public async Task<IActionResult> EditConfig(string fileName)
        {
            if (Request.Method == "GET")
            {
                var filePath = Path.Combine(Strings.ConfigsFolder, fileName + ".json");
                if (!System.IO.File.Exists(filePath))
                {
                    return BadRequest($"Config '{fileName}' does not exist");
                }
                var config = LoadFromFile<Config>(filePath);
                var discordFiles = Directory.GetFiles(Strings.DiscordsFolder, "*.json");
                var locales = Directory.GetFiles(
                    Path.Combine(
                        Path.Combine(
                            Strings.BasePath,
                            Strings.LocaleFolder
                        )
                    ),
                    "*.json"
                ).Select(file => Path.GetFileNameWithoutExtension(file));
                var obj = new
                {
                    template = "configs-edit",
                    title = $"Edit Config \"{fileName}\"",
                    favicon = "dotnet.png",
                    locales,
                    name = fileName,
                    config,
                    discords = discordFiles.Select(file => Path.GetFileName(file)),
                };
                return View("Configs/edit", obj);
            }
            else if (Request.Method == "POST")
            {
                var filePath = Path.Combine(Strings.ConfigsFolder, fileName + ".json");
                var config = LoadFromFile<Config>(filePath);
                var configForm = ConfigFromForm(config, Request.Form);
                var json = configForm.ToJson();
                // Save json
                await WriteDataAsync(filePath, json);
                return Redirect("/dashboard/configs");
            }
            return Unauthorized();
        }

        [HttpGet("configs/delete/{fileName}")]
        public IActionResult DeleteConfig(string fileName)
        {
            var filePath = Path.Combine(Strings.ConfigsFolder, fileName + ".json");
            try
            {
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
                return Redirect("/dashboard/configs");
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to delete config: {filePath}\nError: {ex}");
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
        public async Task<IActionResult> EditDiscord(string fileName)
        {
            if (Request.Method == "GET")
            {
                var filePath = Path.Combine(Strings.DiscordsFolder, fileName + ".json");
                if (!System.IO.File.Exists(filePath))
                {
                    return BadRequest($"Discord config '{fileName}' does not exist");
                }
                var config = LoadFromFile<DiscordServerConfig>(filePath);
                var alarmFiles = Directory.GetFiles(Strings.AlarmsFolder);
                var geofenceFiles = Directory.GetFiles(Strings.GeofencesFolder);
                var embedFiles = Directory.GetFiles(Strings.EmbedsFolder);
                var iconStyles = Startup.Config.IconStyles;
                var roles = GetRoles();
                var obj = new
                {
                    template = "discords-edit",
                    title = $"Edit Discord Server \"{fileName}\"",
                    favicon = "dotnet.png",
                    name = fileName,
                    config,
                    alarms = alarmFiles.Select(file =>
                    {
                        var name = Path.GetFileName(file);
                        return new
                        {
                            file = name,
                            selected = string.Equals(name, config.AlarmsFile, StringComparison.InvariantCultureIgnoreCase),
                        };
                    }),
                    geofences = geofenceFiles.Select(file =>
                    {
                        var name = Path.GetFileName(file);
                        return new
                        {
                            file = name,
                            selected = config.GeofenceFiles.Contains(name),
                        };
                    }),
                    embeds = embedFiles.Select(file =>
                    {
                        var name = Path.GetFileName(file);
                        return new
                        {
                            file = name,
                            selected = string.Equals(name, config.DmEmbedsFile, StringComparison.InvariantCultureIgnoreCase),
                        };
                    }),
                    iconStyles,
                    roles,
                };
                return View("Discords/edit", obj);
            }
            else if (Request.Method == "POST")
            {
                // TODO: If error show Discords/edit view, otherwise redirect from succussful edit
                var filePath = Path.Combine(Strings.DiscordsFolder, fileName + ".json");
                var discord = LoadFromFile<DiscordServerConfig>(filePath);
                var discordForm = DiscordFromForm(discord, Request.Form);
                var json = discordForm.ToJson();
                // Save json
                await WriteDataAsync(filePath, json);
                return Redirect("/dashboard/discords");
            }
            return Unauthorized();
        }

        [HttpGet("discords/delete/{fileName}")]
        public IActionResult DeleteDiscord(string fileName)
        {
            var filePath = Path.Combine(Strings.DiscordsFolder, fileName + ".json");
            try
            {
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
                return Redirect("/dashboard/discords");
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to delete discord: {filePath}\nError: {ex}");
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
        public async Task<IActionResult> EditAlarm(string fileName)
        {
            if (Request.Method == "GET")
            {
                var filePath = Path.Combine(Strings.AlarmsFolder, fileName + ".json");
                if (!System.IO.File.Exists(filePath))
                {
                    return BadRequest($"Alarm '{fileName}' does not exist");
                }
                var embedFiles = Directory.GetFiles(Strings.EmbedsFolder, "*.json");
                var filterFiles = Directory.GetFiles(Strings.FiltersFolder, "*.json");
                var geofenceFiles = Directory.GetFiles(Strings.GeofencesFolder);
                var alarm = LoadFromFile<ChannelAlarmsManifest>(filePath);
                var obj = new
                {
                    template = "alarms-edit",
                    title = $"Edit Channel Alarms \"{fileName}\"",
                    favicon = "dotnet.png",
                    name = fileName,
                    alarm,
                    /*
                    embeds = embedFiles.Select(file =>
                    {
                        var name = Path.GetFileName(file);
                        return new
                        {
                            file = name,
                            selected = alarm.Alarms.Exists(x => string.Equals(name, x.EmbedsFile, StringComparison.InvariantCultureIgnoreCase)),
                        };
                    }),
                    */
                    embeds = embedFiles.Select(file => Path.GetFileName(file)),
                    filters = filterFiles.Select(file => Path.GetFileName(file)),
                    geofences = geofenceFiles.Select(file => Path.GetFileName(file)),
                };
                return View("Alarms/edit", obj);
            }
            else if (Request.Method == "POST")
            {
                // TODO: Check if exists
                var filePath = Path.Combine(Strings.AlarmsFolder, fileName + ".json");
                var alarms = LoadFromFile<ChannelAlarmsManifest>(filePath);
                var alarmsForm = AlarmsFromForm(alarms, Request.Form);
                var json = alarmsForm.ToJson();
                // Save json
                await WriteDataAsync(filePath, json);
                return Redirect("/dashboard/alarms");
            }
            return Unauthorized();
        }

        [HttpGet("alarms/delete/{fileName}")]
        public IActionResult DeleteAlarm(string fileName)
        {
            var filePath = Path.Combine(Strings.AlarmsFolder, fileName + ".json");
            try
            {
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
                return Redirect("/dashboard/alarms");
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to delete alarm: {filePath}\nError: {ex}");
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
        public async Task<IActionResult> EditFilter(string fileName)
        {
            if (Request.Method == "GET")
            {
                var filePath = Path.Combine(Strings.FiltersFolder, fileName + ".json");
                if (!System.IO.File.Exists(filePath))
                {
                    return BadRequest($"Filter '{fileName}' does not exist");
                }
                var obj = new
                {
                    template = "filters-edit",
                    title = $"Edit Webhook Filter \"{fileName}\"",
                    favicon = "dotnet.png",
                    name = fileName,
                    filter = LoadFromFile<WebhookFilter>(filePath),
                };
                return View("Filters/edit", obj);
            }
            else if (Request.Method == "POST")
            {
                // TODO: Check if exists
                var filePath = Path.Combine(Strings.FiltersFolder, fileName + ".json");
                var filter = LoadFromFile<WebhookFilter>(filePath);
                var filterForm = FilterFromForm(filter, Request.Form);
                var json = filterForm.ToJson();
                // Save json
                await WriteDataAsync(filePath, json);
                return Redirect("/dashboard/filters");
            }
            return Unauthorized();
        }

        [HttpGet("filters/delete/{fileName}")]
        public IActionResult DeleteFilter(string fileName)
        {
            var filePath = Path.Combine(Strings.FiltersFolder, fileName + ".json");
            try
            {
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
                return Redirect("/dashboard/filters");
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to delete filter: {filePath}\nError: {ex}");
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
        public async Task<IActionResult> NewEmbed()
        {
            if (Request.Method == "GET")
            {
                var obj = new
                {
                    template = "embeds-mew",
                    title = $"New Message Embed",
                    favicon = "dotnet.png",
                    embed = EmbedMessage.Defaults,
                    placeholders = GetDtsPlaceholders(),
                };
                return View("Embeds/new", obj);
            }
            else if (Request.Method == "POST")
            {
                var fileName = Request.Form["name"].ToString();
                // TODO: Check if exists or not
                var embed = EmbedMessage.Defaults;
                var embedForm = EmbedFromForm(embed, Request.Form);
                var json = embedForm.ToJson();
                // Save json
                var filePath = Path.Combine(Strings.EmbedsFolder, fileName + ".json");
                await WriteDataAsync(filePath, json);
                return Redirect("/dashboard/embeds");
            }
            return Unauthorized();
        }

        [HttpGet]
        [HttpPost]
        [Route("embeds/edit/{fileName}")]
        public async Task<IActionResult> EditEmbed(string fileName)
        {
            if (Request.Method == "GET")
            {
                var filePath = Path.Combine(Strings.EmbedsFolder, fileName + ".json");
                if (!System.IO.File.Exists(filePath))
                {
                    return BadRequest($"Embed '{fileName}' does not exist");
                }
                var obj = new
                {
                    template = "embeds-edit",
                    title = $"Edit Message Embed \"{fileName}\"",
                    favicon = "dotnet.png",
                    name = fileName,
                    embed = LoadFromFile<EmbedMessage>(filePath),
                    placeholders = GetDtsPlaceholders(),
                };
                return View("Embeds/edit", obj);
            }
            else if (Request.Method == "POST")
            {
                // TODO: Check if exists or not
                var filePath = Path.Combine(Strings.EmbedsFolder, fileName + ".json");
                var embed = LoadFromFile<EmbedMessage>(filePath);
                var embedForm = EmbedFromForm(embed, Request.Form);
                var json = embedForm.ToJson();
                // Save json
                await WriteDataAsync(filePath, json);
                return Redirect("/dashboard/embeds");
            }
            return Unauthorized();
        }

        [HttpGet("embeds/delete/{fileName}")]
        public IActionResult DeleteEmbed(string fileName)
        {
            var filePath = Path.Combine(Strings.EmbedsFolder, fileName + ".json");
            try
            {
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
                return Redirect("/dashboard/embeds");
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to delete embed: {filePath}\nError: {ex}");
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
        public async Task<IActionResult> EditGeofence(string fileName)
        {
            if (Request.Method == "GET")
            {
                var filePath = Path.Combine(Strings.GeofencesFolder, fileName);
                if (!System.IO.File.Exists(filePath))
                {
                    return BadRequest($"Geofence '{fileName}' does not exist");
                }
                var geofence = LoadFromFile(filePath);
                var obj = new
                {
                    template = "geofences-edit",
                    title = $"Edit Geofence \"{fileName}\"",
                    favicon = "dotnet.png",
                    name = Path.GetFileNameWithoutExtension(fileName),
                    geofence,
                    format = Path.GetExtension(fileName),
                };
                return View("Geofences/edit", obj);
            }
            else if (Request.Method == "POST")
            {
                var name = Request.Form["name"].ToString();
                var geofenceType = Request.Form["geofenceType"].ToString();
                var geofenceData = Request.Form["geofence"].ToString();
                // TODO: Check if exists or not
                var newFileName = $"{name}.{geofenceType}";
                var newFilePath = Path.Combine(Strings.GeofencesFolder, newFileName);
                if (!string.Equals(fileName, newFileName))
                {
                    // TODO: Move file
                    System.IO.File.Move(
                        Path.Combine(Strings.GeofencesFolder, fileName),
                        newFilePath
                    );
                }
                // Save json
                await WriteDataAsync(newFilePath, geofenceData);
                return Redirect("/dashboard/geofences");
            }
            return Unauthorized();
        }

        [HttpGet("geofences/delete/{fileName}")]
        public IActionResult DeleteGeofence(string fileName)
        {
            var filePath = Path.Combine(Strings.GeofencesFolder, fileName);
            try
            {
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
                return Redirect("/dashboard/geofences");
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to delete geofence: {filePath}\nError: {ex}");
            }
            return Unauthorized();
        }

        #endregion

        #region Discord Roles

        [HttpGet]
        [Route("roles")]
        public IActionResult DiscordRoles()
        {
            var roles = GetRoles();
            var obj = new
            {
                template = "roles",
                title = "Discord Roles",
                favicon = "dotnet.png",
                roles,
            };
            return View("roles", obj);
        }

        [HttpGet]
        [HttpPost]
        [Route("roles/edit/{name}")]
        public async Task<IActionResult> EditDiscordRole(string name)
        {
            if (Request.Method == "GET")
            {
                var roles = GetRoles();
                var role = roles.FirstOrDefault(role => string.Equals(role.Value.Name, name, StringComparison.InvariantCultureIgnoreCase));
                var obj = new
                {
                    template = "roles-edit",
                    title = $"Edit Discord Role \"{name}\"",
                    favicon = "dotnet.png",
                    name,
                    roleId = role.Key,
                    role = role.Value,
                };
                return View("Roles/edit", obj);
            }
            else if (Request.Method == "POST")
            {
                // TODO: Check if exists or not
                var roles = GetRoles();
                var rolesForm = RolesFromForm(roles, Request.Form);
                var json = rolesForm.ToJson();
                // Save json
                var filePath = "wwwroot/static/data/roles.json";
                await WriteDataAsync(filePath, json);
                return Redirect("/dashboard/roles");
            }
            return Unauthorized();
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

        [HttpGet]
        [HttpPost]
        [Route("users/new")]
        public IActionResult NewUser()
        {
            if (Request.Method == "GET")
            {
                var obj = new
                {
                    template = "users-mew",
                    title = $"New Admin",
                    favicon = "dotnet.png",
                };
                return View("Users/new", obj);
            }
            else if (Request.Method == "POST")
            {
            }
            return Unauthorized();
        }

        [HttpGet]
        [HttpPost]
        [Route("users/edit/{id}")]
        public IActionResult EditUser(uint id)
        {
            var obj = new
            {
                template = "users-edit",
                title = "Edit Admin " + id,
                favicon = "dotnet.png",
            };
            return View("Users/edit", obj);
        }

        [HttpGet("users/delete/{id}")]
        public IActionResult DeleteUser(uint id)
        {
            return Unauthorized();
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

        private static Dictionary<string, List<DtsPlaceholder>> GetDtsPlaceholders()
        {
            var path = "wwwroot/static/data/dts_placeholders.json";
            var placeholders = LoadFromFile<Dictionary<string, List<DtsPlaceholder>>>(path);
            return placeholders;
        }

        private static Dictionary<ulong, RoleConfig> GetRoles()
        {
            var path = "wwwroot/static/data/roles.json";
            var roles = LoadFromFile<Dictionary<ulong, RoleConfig>>(path);
            return roles;
        }

        private static async Task WriteDataAsync(string path, string data)
        {
            await System.IO.File.WriteAllTextAsync(path, data, Encoding.UTF8);
        }

        #region Form Helpers

        private static Config ConfigFromForm(Config config, IFormCollection form)
        {
            config.ListeningHost = form["host"].ToString();
            config.WebhookPort = ushort.Parse(form["port"].ToString());
            config.Locale = form["locale"].ToString();
            config.Debug = form["debug"].ToString() == "on";
            config.ShortUrlApi = new UrlShortenerConfig
            {
                ApiUrl = form["shortUrlApiUrl"].ToString(),
                Signature = form["shortUrlApiSignature"].ToString(),
            };
            config.StripeApi = new StripeConfig
            {
                ApiKey = form["stripeApiKey"].ToString(),
            };
            var discordServerFiles = form["discordServers"].ToString();
            if (!string.IsNullOrEmpty(discordServerFiles))
            {
                var discordFiles = discordServerFiles.Split(',')
                                                     .ToList();
                config.ServerConfigFiles.Clear();
                foreach (var discordServerFile in discordFiles)
                {
                    var discordFilePath = Path.Combine(Strings.DiscordsFolder, discordServerFile);
                    var discordServer = LoadFromFile<DiscordServerConfig>(discordFilePath);
                    if (!config.ServerConfigFiles.ContainsKey(discordServer.Bot.GuildId.ToString()))
                    {
                        config.ServerConfigFiles.Add(discordServer.Bot.GuildId.ToString(), discordServerFile);
                    }
                }
            }
            //config.ServerConfigFiles = form["discordServers"].ToString();
            config.Database = new ConnectionStringsConfig
            {
                Main = new DatabaseConfig
                {
                    Host = form["dbMainHost"].ToString(),
                    Port = ushort.Parse(form["dbMainPort"].ToString()),
                    Username = form["dbMainUsername"].ToString(),
                    Password = form["dbMainPassword"].ToString(),
                    Database = form["dbMainDatabase"].ToString(),
                },
                Scanner = new DatabaseConfig
                {
                    Host = form["dbScannerHost"].ToString(),
                    Port = ushort.Parse(form["dbScannerPort"].ToString()),
                    Username = form["dbScannerUsername"].ToString(),
                    Password = form["dbScannerPassword"].ToString(),
                    Database = form["dbScannerDatabase"].ToString(),
                },
                Nests = new DatabaseConfig
                {
                    Host = form["dbNestsHost"].ToString(),
                    Port = ushort.Parse(form["dbNestsPort"].ToString()),
                    Username = form["dbNestsUsername"].ToString(),
                    Password = form["dbNestsPassword"].ToString(),
                    Database = form["dbNestsDatabase"].ToString(),
                },
            };
            config.Urls = new UrlConfig
            {
                ScannerMap = form["scannerMapUrl"].ToString(),
            };
            config.EventPokemon = new EventPokemonConfig
            {
                PokemonIds = form["eventPokemonIds"].ToString()
                                                    .Split(',')
                                                    .Select(int.Parse)
                                                    .ToList(),
                MinimumIV = int.Parse(form["eventPokemonMinIV"].ToString()),
                FilterType = form["eventPokemonFilterType"].ToString() == "Include"
                    ? Services.Alarms.Filters.FilterType.Include
                    : Services.Alarms.Filters.FilterType.Exclude,
            };
            // TODO: config.IconStyles
            /*
            TODO config.StaticMaps = new Dictionary<StaticMapType, StaticMapConfig>
            {
                {
                    StaticMapType.Pokemon, new StaticMapConfig
                    {
                        TemplateName = "",
                        Url = "",
                        IncludeNearbyGyms = false,
                        IncludeNearbyPokestops = false,
                    },
                },
            };
            */
            var twilioUserIds = form["twilioUserIds"].ToString();
            var twilioRoleIds = form["twilioRoleIds"].ToString();
            var twilioPokemonIds = form["twilioPokemonIds"].ToString();
            config.Twilio = new TwilioConfig
            {
                Enabled = form["twilioEnabled"].ToString() == "on",
                AccountSid = form["twilioAccountSid"].ToString(),
                AuthToken = form["twilioAuthToken"].ToString(),
                FromNumber = form["twilioFromNumber"].ToString(),
                UserIds = string.IsNullOrEmpty(twilioUserIds)
                    ? new List<ulong>()
                    : twilioUserIds.Split(',')
                                   .Select(ulong.Parse)
                                   .ToList(),
                RoleIds = string.IsNullOrEmpty(twilioRoleIds)
                    ? new List<ulong>()
                    : twilioRoleIds.Split(',')
                                   .Select(ulong.Parse)
                                   .ToList(),
                PokemonIds = string.IsNullOrEmpty(twilioPokemonIds)
                    ? new List<uint>()
                    : twilioPokemonIds.Split(',')
                                      .Select(uint.Parse)
                                      .ToList(),
                MinimumIV = int.Parse(form["twilioMinIV"].ToString()),
            };
            config.ReverseGeocoding = new ReverseGeocodingConfig
            {
                Provider = form["provider"].ToString() == "google"
                    ? Services.Geofence.Geocoding.ReverseGeocodingProvider.GMaps
                    : Services.Geofence.Geocoding.ReverseGeocodingProvider.Osm,
                CacheToDisk = form["cacheToDisk"].ToString() == "on",
                GoogleMaps = new GoogleMapsConfig
                {
                    Key = form["gmapsKey"].ToString(),
                    Schema = form["gmapsSchema"].ToString(),
                },
                Nominatim = new NominatimConfig
                {
                    Endpoint = form["nominatimEndpoint"].ToString(),
                    Schema = form["nominatimSchema"].ToString(),
                },
            };
            config.DespawnTimeMinimumMinutes = ushort.Parse(form["despawnTimeMinimumMinutes"].ToString());
            config.CheckForDuplicates = form["checkForDuplicates"] == "on";
            config.Debug = form["debug"] == "on";
            config.LogLevel = (LogLevel)int.Parse(form["logLevel"].ToString());
            //config.LogLevel
            return config;
        }

        private static DiscordServerConfig DiscordFromForm(DiscordServerConfig discord, IFormCollection form)
        {
            var donorRoleIds = form["donorRoleIds"].ToString();
            if (!string.IsNullOrEmpty(donorRoleIds))
            {
                var availableRoles = GetRoles().Where(x => !x.Value.IsModerator);
                var roleIds = donorRoleIds.Split(',')
                                          .Select(ulong.Parse)
                                          .ToList();
                var donorRoles = availableRoles.ToDictionary(key => key.Key, value => value.Value.Permissions);
                discord.DonorRoleIds = (Dictionary<ulong, IEnumerable<SubscriptionAccessType>>)donorRoles;
            }
            var moderatorRoleIds = form["moderatorRoleIds"].ToString();
            if (!string.IsNullOrEmpty(moderatorRoleIds))
            {
                discord.ModeratorRoleIds = moderatorRoleIds.Split(',')
                                                           .Select(ulong.Parse)
                                                           .ToList();
            }
            discord.FreeRoleName = form["freeRoleName"].ToString();
            discord.AlarmsFile = form["alarms"].ToString();
            discord.GeofenceFiles = form["geofences"].ToString().Split(',');
            discord.IconStyle = form["iconStyle"].ToString();
            discord.DmEmbedsFile = form["embed"].ToString();
            if (discord.Bot == null)
            {
                discord.Bot = new BotConfig();
            }
            discord.Bot.CommandPrefix = form["commandPrefix"].ToString();
            discord.Bot.GuildId = ulong.Parse(form["guildId"].ToString());
            discord.Bot.EmojiGuildId = ulong.Parse(form["emojiGuildId"].ToString());
            discord.Bot.Token = form["token"].ToString();
            //discord.Bot.ChannelIds = form["channelIds"].ToString();
            discord.Bot.Status = form["status"].ToString();
            discord.Bot.OwnerId = ulong.Parse(form["ownerId"].ToString());
            if (discord.Subscriptions == null)
            {
                discord.Subscriptions = new SubscriptionsConfig();
            }
            discord.Subscriptions.Enabled = form["subscriptionsEnabled"].ToString() == "on";
            discord.Subscriptions.MaxPokemonSubscriptions = int.Parse(form["maxPokemonSubscriptions"].ToString());
            discord.Subscriptions.MaxPvPSubscriptions = int.Parse(form["maxPvPSubscriptions"].ToString());
            discord.Subscriptions.MaxRaidSubscriptions = int.Parse(form["maxRaidSubscriptions"].ToString());
            discord.Subscriptions.MaxQuestSubscriptions = int.Parse(form["maxQuestSubscriptions"].ToString());
            discord.Subscriptions.MaxLureSubscriptions = int.Parse(form["maxLureSubscriptions"].ToString());
            discord.Subscriptions.MaxInvasionSubscriptions = int.Parse(form["maxInvasionSubscriptions"].ToString());
            discord.Subscriptions.MaxGymSubscriptions = int.Parse(form["maxGymSubscriptions"].ToString());
            discord.Subscriptions.MaxNotificationsPerMinute = ushort.Parse(form["maxNotificationsPerMinute"].ToString());
            discord.Subscriptions.Url = form["subscriptionsUiUrl"].ToString();
            if (discord.GeofenceRoles == null)
            {
                discord.GeofenceRoles = new GeofenceRolesConfig();
            }
            discord.GeofenceRoles.Enabled = form["geofenceRolesEnabled"].ToString() == "on";
            discord.GeofenceRoles.AutoRemove = form["geofenceRolesAutoRemove"].ToString() == "on";
            discord.GeofenceRoles.RequiresDonorRole = form["geofenceRolesRequiresDonorRole"].ToString() == "on";
            if (discord.QuestsPurge == null)
            {
                discord.QuestsPurge = new QuestsPurgeConfig();
            }
            discord.QuestsPurge.Enabled = form["questsPurgeEnabled"].ToString() == "on";
            // questsPurgeChannels = timezone: channelIds[]
            // TODO: discord.QuestsPurge.ChannelIds = form["questsPurgeChannels"].ToString();
            if (discord.Nests == null)
            {
                discord.Nests = new NestsConfig();
            }
            discord.Nests.Enabled = form["nestsEnabled"].ToString() == "on";
            discord.Nests.ChannelId = ulong.Parse(form["nestsChannelId"].ToString());
            discord.Nests.MinimumPerHour = int.Parse(form["nestsMinimumPerHour"].ToString());
            if (discord.DailyStats == null)
            {
                discord.DailyStats = new DailyStatsConfig();
            }
            // TODO: discord.dailyStats.iv and discord.dailyStats.shiny
            return discord;
        }

        private static ChannelAlarmsManifest AlarmsFromForm(ChannelAlarmsManifest alarms, IFormCollection form)
        {
            // TODO: Set alarms
            return alarms;
        }

        private static EmbedMessage EmbedFromForm(EmbedMessage embed, IFormCollection form)
        {
            embed[EmbedMessageType.Pokemon].AvatarUrl = form["pokemonAvatarUrl"].ToString();
            embed[EmbedMessageType.Pokemon].ContentList = form["pokemonContent"].ToString()
                                                                                .Split('\n')
                                                                                .ToList();
            embed[EmbedMessageType.Pokemon].IconUrl = form["pokemonIconUrl"].ToString();
            embed[EmbedMessageType.Pokemon].ImageUrl = form["pokemonImageUrl"].ToString();
            embed[EmbedMessageType.Pokemon].Title = form["pokemonTitle"].ToString();
            embed[EmbedMessageType.Pokemon].Url = form["pokemonUrl"].ToString();
            embed[EmbedMessageType.Pokemon].Username = form["pokemonUsername"].ToString();
            embed[EmbedMessageType.Pokemon].Footer = new EmbedMessageFooter
            {
                Text = form["pokemonFooterText"].ToString(),
                IconUrl = form["pokemonFooterIconUrl"].ToString(),
            };
            // TODO: Raids, Gyms, Pokestops, etc
            return embed;
        }

        private static WebhookFilter FilterFromForm(WebhookFilter filter, IFormCollection form)
        {
            if (form.ContainsKey("pokemonEnabled"))
            {
                if (filter.Pokemon == null)
                {
                    filter.Pokemon = new WebhookFilterPokemon();
                }
                filter.Pokemon.Enabled = form["pokemonEnabled"].ToString() == "on";
                var pokemonList = form["pokemonPokemonList"].ToString();
                if (!string.IsNullOrEmpty(pokemonList))
                {
                    filter.Pokemon.Pokemon = pokemonList.Split(',')?.Select(uint.Parse).ToList() ?? new List<uint>();
                }
                var formsList = form["pokemonFormsList"].ToString();
                if (!string.IsNullOrEmpty(formsList))
                {
                    filter.Pokemon.Forms = formsList.Split(',').ToList();
                }
                var costumesList = form["pokemonCostumesList"].ToString();
                if (!string.IsNullOrEmpty(costumesList))
                {
                    filter.Pokemon.Costumes = costumesList.Split(',').ToList();
                }
                filter.Pokemon.MinimumCP = uint.Parse(form["pokemonMinCP"].ToString());
                filter.Pokemon.MaximumCP = uint.Parse(form["pokemonMaxCP"].ToString());
                filter.Pokemon.MinimumIV = uint.Parse(form["pokemonMinIV"].ToString());
                filter.Pokemon.MaximumIV = uint.Parse(form["pokemonMaxIV"].ToString());
                filter.Pokemon.MinimumLevel = uint.Parse(form["pokemonMinLevel"].ToString());
                filter.Pokemon.MaximumLevel = uint.Parse(form["pokemonMaxLevel"].ToString());
                filter.Pokemon.Gender = form["pokemonGender"].ToString().FirstOrDefault();
                // TODO: Convert size filter.Pokemon.Size = form["pokemonSize"].ToString();
                filter.Pokemon.IsPvpGreatLeague = form["pokemonGreatLeague"].ToString() == "on";
                filter.Pokemon.IsPvpUltraLeague = form["pokemonUltraLeague"].ToString() == "on";
                filter.Pokemon.MinimumRank = uint.Parse(form["pokemonMinRank"].ToString());
                filter.Pokemon.MaximumRank = uint.Parse(form["pokemonMaxRank"].ToString());
                filter.Pokemon.IsEvent = form["pokemonIsEvent"].ToString() == "on";
                filter.Pokemon.FilterType = form["pokemonFilterType"].ToString() == "Include"
                    ? Services.Alarms.Filters.FilterType.Include
                    : Services.Alarms.Filters.FilterType.Exclude;
                filter.Pokemon.IgnoreMissing = form["pokemonIgnoreMissing"].ToString() == "on";
            }
            if (form.ContainsKey("raidsEnabled"))
            {
                if (filter.Raids == null)
                {
                    filter.Raids = new WebhookFilterRaid();
                }
                filter.Raids.Enabled = form["raidsEnabled"].ToString() == "on";
                var pokemonList = form["raidsPokemonList"].ToString();
                if (!string.IsNullOrEmpty(pokemonList))
                {
                    filter.Raids.Pokemon = pokemonList.Split(',')?.Select(uint.Parse).ToList() ?? new List<uint>();
                }
                var formsList = form["raidsFormsList"].ToString();
                if (!string.IsNullOrEmpty(formsList))
                {
                    filter.Raids.Forms = formsList.Split(',').ToList();
                }
                var costumesList = form["raidsCostumesList"].ToString();
                if (!string.IsNullOrEmpty(costumesList))
                {
                    filter.Raids.Costumes = costumesList.Split(',').ToList();
                }
                filter.Raids.MinimumLevel = uint.Parse(form["raidsMinLevel"].ToString());
                filter.Raids.MaximumLevel = uint.Parse(form["raidsMaxLevel"].ToString());
                filter.Raids.OnlyEx = form["raidsOnlyEx"].ToString() == "on";
                // TODO: Convert team filter.Raids.Team = form["raidsTeam"].ToString();
                filter.Raids.FilterType = form["raidsFilterType"].ToString() == "Include"
                    ? Services.Alarms.Filters.FilterType.Include
                    : Services.Alarms.Filters.FilterType.Exclude;
                filter.Raids.IgnoreMissing = form["raidsIgnoreMissing"].ToString() == "on";
            }
            if (form.ContainsKey("eggsEnabled"))
            {
                if (filter.Eggs == null)
                {
                    filter.Eggs = new WebhookFilterEgg();
                }
                filter.Eggs.Enabled = form["eggsEnabled"].ToString() == "on";
                filter.Eggs.MinimumLevel = uint.Parse(form["eggsMinLevel"].ToString());
                filter.Eggs.MaximumLevel = uint.Parse(form["eggsMaxLevel"].ToString());
                filter.Eggs.OnlyEx = form["eggsOnlyEx"].ToString() == "on";
                // TODO: Convert team filter.Eggs.Team = form["eggsTeam"].ToString();
            }
            if (form.ContainsKey("questsEnabled"))
            {
                if (filter.Quests == null)
                {
                    filter.Quests = new WebhookFilterQuest();
                }
                filter.Quests.Enabled = form["questsEnabled"].ToString() == "on";
                filter.Quests.RewardKeywords = form["questsRewards"].ToString().Split(',').ToList();
                filter.Quests.IsShiny = form["questsIsShiny"].ToString() == "on";
                filter.Quests.FilterType = form["questsFilterType"].ToString() == "Include"
                    ? Services.Alarms.Filters.FilterType.Include
                    : Services.Alarms.Filters.FilterType.Exclude;
            }
            if (form.ContainsKey("pokestopsEnabled"))
            {
                if (filter.Pokestops == null)
                {
                    filter.Pokestops = new WebhookFilterPokestop();
                }
                filter.Pokestops.Enabled = form["pokestopsEnabled"].ToString() == "on";
                filter.Pokestops.Lured = form["pokestopsLured"].ToString() == "on";
                // TODO: Convert lure types filter.Pokestops.LureTypes = form["pokestopsLureTypes"].ToString();
                filter.Pokestops.Invasions = form["pokestopsInvasions"].ToString() == "on";
                // TODO: Convert invasion types filter.Pokestops.InvasionTypes = form["pokestopsInvasionTypes"].ToString() == "on";
            }
            if (form.ContainsKey("gymsEnabled"))
            {
                if (filter.Gyms == null)
                {
                    filter.Gyms = new WebhookFilterGym();
                }
                filter.Gyms.Enabled = form["gymsEnabled"] == "on";
                filter.Gyms.UnderAttack = form["gymsUnderAttack"].ToString() == "on";
                // TODO: Convert team filter.Gyms.Team = form["gymsTeam"].ToString();
            }
            if (form.ContainsKey("weatherEnabled"))
            {
                if (filter.Weather == null)
                {
                    filter.Weather = new WebhookFilterWeather();
                }
                filter.Weather.Enabled = form["weatherEnabled"] == "on";
                // TODO: Convert weather types filter.Weather.WeatherTypes = form["weatherTypes"].ToString();
            }
            return filter;
        }

        private static Dictionary<ulong, RoleConfig> RolesFromForm(Dictionary<ulong, RoleConfig> roles, IFormCollection form)
        {
            var id = ulong.Parse(form["id"].ToString());
            var name = form["name"].ToString();
            var permissions = form["permissions"].ToString();
            var isModerator = form["moderator"].ToString() == "on";
            Console.WriteLine($"Id: {id}, Name: {name}, Permissions: {permissions}, Moderator: {isModerator}");
            if (roles.ContainsKey(id))
            {
                roles[id].Name = name;
                roles[id].Permissions = (IReadOnlyList<SubscriptionAccessType>)permissions.Split(',').Select(x => x.Cast<SubscriptionAccessType>()).ToList();
                roles[id].IsModerator = isModerator;
            }
            else
            {
                roles.Add(id, new RoleConfig
                {
                    Name = name,
                    Permissions = (IReadOnlyList<SubscriptionAccessType>)permissions.Split(',').Select(x => x.Cast<SubscriptionAccessType>()).ToList(),
                    IsModerator = isModerator,
                });
            }
            return roles;
        }

        #endregion

        #endregion
    }

    public class DtsPlaceholder
    {
        [JsonPropertyName("placeholder")]
        public string Placeholder { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("example")]
        public object Example { get; set; }
    }

    public class RoleConfig
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("moderator")]
        public bool IsModerator { get; set; }

        [JsonPropertyName("permissions")]
        public IEnumerable<SubscriptionAccessType> Permissions { get; set; }
    }
}