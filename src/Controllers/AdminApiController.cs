namespace WhMgr.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.IO;
    using System.Linq;
    using System.Net.Mime;
    using System.Text;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

    using WhMgr.Configuration;
    using WhMgr.Extensions;
    using WhMgr.Services.Alarms.Embeds;
    using WhMgr.Services.Alarms.Filters.Models;
    using WhMgr.Services.Alarms.Models;

    [ApiController]
    [Route("/api/v1/admin/")]
    public class AdminApiController : ControllerBase
    {
        private readonly ILogger<SubscriptionApiController> _logger;

        public AdminApiController(
            ILogger<SubscriptionApiController> logger)
        {
            _logger = logger;
        }

        [HttpGet("dashboard")]
        [Produces(MediaTypeNames.Application.Json)]
        public IActionResult GetDashboard()
        {
            return new JsonResult(new List<dynamic>
            {
                new { name = "Configs", count = Directory.GetFiles(Strings.ConfigsFolder, "*.json").Length },
                new { name = "Discords", count = Directory.GetFiles(Strings.DiscordsFolder, "*.json").Length },
                new { name = "Alarms", count = Directory.GetFiles(Strings.AlarmsFolder, "*.json").Length },
                new { name = "Filters", count = Directory.GetFiles(Strings.FiltersFolder, "*.json").Length },
                new { name = "Embeds", count = Directory.GetFiles(Strings.EmbedsFolder, "*.json").Length },
                new { name = "Geofences", count = Directory.GetFiles(Strings.GeofencesFolder).Length },
                new { name = "Roles", count = GetRoles().Count },
                new { name = "Users", count = 0 },
            });
        }

        #region Config API

        [HttpGet("configs")]
        [Produces(MediaTypeNames.Application.Json)]
        public IActionResult GetConfigs()
        {
            var files = Directory.GetFiles(Strings.ConfigsFolder, "*.json");
            //var configs = new Dictionary<string, Config>();
            var configs = new List<dynamic>();
            foreach (var file in files)
            {
                try
                {
                    var name = Path.GetFileNameWithoutExtension(file);
                    var config = LoadFromFile<Config>(file);
                    configs.Add(new
                    {
                        id = name,
                        host = config.ListeningHost,
                        port = config.WebhookPort,
                        count = config.ServerConfigFiles.Count,
                    });
                    Console.WriteLine($"Config: {config.ListeningHost}:{config.WebhookPort}");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Failed to parse config: {file}\nError: {ex}");
                }
            }
            return new JsonResult(configs);
        }

        [HttpGet("config/{fileName}")]
        [Produces(MediaTypeNames.Application.Json)]
        public IActionResult GetConfig(string fileName)
        {
            var filePath = Path.Combine(Strings.ConfigsFolder, fileName + ".json");
            if (!System.IO.File.Exists(filePath))
            {
                return new JsonResult(new
                {
                    status = "Error",
                    error = $"Config '{fileName}' does not exist",
                });
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

            return new JsonResult(new
            {
                status = "OK",
                data = new
                {
                    config,
                    discords = discordFiles.Select(file => Path.GetFileName(file)),
                    locales,
                },
            });
        }

        [HttpPost("config/{fileName}")]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<IActionResult> UpdateConfig(string fileName)//, Config data)
        {
            var data = await Request.GetRawBodyStringAsync();
            Console.WriteLine($"data: {data}");
            var jsonStr = data.FromJson<dynamic>();
            Console.WriteLine($"json: {jsonStr}");
            // TODO: Construct config and save

            var filePath = Path.Combine(Strings.ConfigsFolder, fileName + ".json");
            if (!System.IO.File.Exists(filePath))
            {
                // Config file with name already exists
                return BadRequest($"Config file at location '{filePath}' does not exist");
            }
            var config = new Config();
            var configForm = ConfigFromForm(config, Request.Form);
            var json = configForm.ToJson();
            // Save json
            await WriteDataAsync(filePath, json);
            return new JsonResult(new
            {
                status = "OK",
                message = $"Config file {fileName} succuessfully updated.",
            });
        }

        #endregion

        #region Discord Servers API

        [HttpGet("discords")]
        [Produces(MediaTypeNames.Application.Json)]
        public IActionResult GetDiscords()
        {
            var files = Directory.GetFiles(Strings.DiscordsFolder, "*.json");
            var discords = new List<dynamic>();
            foreach (var file in files)
            {
                try
                {
                    var name = Path.GetFileNameWithoutExtension(file);
                    var discord = LoadFromFile<DiscordServerConfig>(file);
                    discords.Add(new
                    {
                        id = name,
                        alarms = discord.AlarmsFile,
                        geofences = discord.GeofenceFiles.Length,
                        subscriptions_enabled = discord.Subscriptions?.Enabled ?? false,
                        embeds = discord.Subscriptions.EmbedsFile,
                        icon_style = discord.IconStyle,
                    });
                }
                catch (Exception ex)
                {
                    _logger.Error($"Failed to parse discord: {file}\nError: {ex}");
                }
            }
            return new JsonResult(discords);
        }

        [HttpGet("discord/{fileName}")]
        [Produces(MediaTypeNames.Application.Json)]
        public IActionResult GetDiscord(string fileName)
        {
            var filePath = Path.Combine(Strings.DiscordsFolder, fileName + ".json");
            if (!System.IO.File.Exists(filePath))
            {
                return new JsonResult(new
                {
                    status = "Error",
                    error = $"Discord '{fileName}' does not exist",
                });
            }
            var discord = LoadFromFile<DiscordServerConfig>(filePath);

            var geofenceFiles = Directory.GetFiles(Strings.GeofencesFolder);
            var validGeofences = new[] { ".json", ".txt" };
            var geofences = geofenceFiles.Where(f => validGeofences.Contains(Path.GetExtension(f)))
                                         .Select(f => Path.GetFileName(f));

            var alarms = Directory.GetFiles(Strings.AlarmsFolder, "*.json")
                                  .Select(f => Path.GetFileName(f));
            var embeds = Directory.GetFiles(Strings.EmbedsFolder, "*.json")
                                  .Select(f => Path.GetFileName(f));

            var roles = GetRoles();
            var result = new List<dynamic>();
            foreach (var (roleId, role) in roles)
            {
                result.Add(new
                {
                    id = roleId,
                    name = role.Name,
                    permissions = role.Permissions,
                    isModerator = role.IsModerator,
                });
            }

            return new JsonResult(new
            {
                status = "OK",
                data = new
                {
                    discord,
                    allGeofences = geofences,
                    allAlarms = alarms,
                    allEmbeds = embeds,
                    allRoles = result,
                    allIconStyles = new List<string>
                    {
                        "Default",
                        "Test",
                    },
                    // TODO: Include icon styles
                },
            });
        }

        #endregion

        #region Alarms API

        [HttpGet("alarms")]
        [Produces(MediaTypeNames.Application.Json)]
        public IActionResult GetAlarms()
        {
            var files = Directory.GetFiles(Strings.AlarmsFolder, "*.json");
            var alarms = new List<dynamic>();
            foreach (var file in files)
            {
                try
                {
                    var name = Path.GetFileNameWithoutExtension(file);
                    var alarm = LoadFromFile<ChannelAlarmsManifest>(file);
                    alarms.Add(new
                    {
                        id = name,
                        enable_pokemon = alarm.EnablePokemon,
                        enable_raids = alarm.EnableRaids,
                        enable_gyms = alarm.EnableGyms,
                        enable_quests = alarm.EnableQuests,
                        enable_pokestops = alarm.EnablePokestops,
                        enable_weather = alarm.EnableWeather,
                        count = alarm.Alarms.Count,
                    });
                }
                catch (Exception ex)
                {
                    _logger.Error($"Failed to parse alarm: {file}\nError: {ex}");
                }
            }
            return new JsonResult(alarms);
        }

        [HttpGet("alarm/{fileName}")]
        [Produces(MediaTypeNames.Application.Json)]
        public IActionResult GetAlarm(string fileName)
        {
            var filePath = Path.Combine(Strings.AlarmsFolder, fileName + ".json");
            if (!System.IO.File.Exists(filePath))
            {
                return BadRequest($"Alarm '{fileName}' does not exist");
            }
            var alarm = LoadFromFile<ChannelAlarmsManifest>(filePath);
            var embedFiles = Directory.GetFiles(Strings.EmbedsFolder, "*.json");
            var filterFiles = Directory.GetFiles(Strings.FiltersFolder, "*.json");
            var geofenceFiles = Directory.GetFiles(Strings.GeofencesFolder);
            return new JsonResult(new
            {
                status = "OK",
                data = new
                {
                    alarm,
                    embeds = embedFiles.Select(file => Path.GetFileName(file)),
                    filters = filterFiles.Select(file => Path.GetFileName(file)),
                    geofences = geofenceFiles.Select(file => Path.GetFileName(file)),
                },
            });
        }

        #endregion

        #region Filters API

        [HttpGet("filters")]
        [Produces(MediaTypeNames.Application.Json)]
        public IActionResult GetFilters()
        {
            var files = Directory.GetFiles(Strings.FiltersFolder, "*.json");
            var filters = new List<dynamic>();
            foreach (var file in files)
            {
                try
                {
                    var name = Path.GetFileNameWithoutExtension(file);
                    var filter = LoadFromFile<WebhookFilter>(file);
                    filters.Add(new
                    {
                        id = name,
                        pokemon = filter.Pokemon != null,
                        raids = filter.Raids != null,
                        gyms = filter.Gyms != null,
                        quests = filter.Quests != null,
                        pokestops = filter.Pokestops != null,
                        weather = filter.Weather != null,
                    });
                }
                catch (Exception ex)
                {
                    _logger.Error($"Failed to prase filter: {file}\nError: {ex}");
                }
            }
            return new JsonResult(filters);
        }

        [HttpGet("filter/{fileName}")]
        [Produces(MediaTypeNames.Application.Json)]
        public IActionResult GetFilter(string fileName)
        {
            var filePath = Path.Combine(Strings.FiltersFolder, fileName + ".json");
            if (!System.IO.File.Exists(filePath))
            {
                return new JsonResult(new
                {
                    status = "Error",
                    error = $"Filter '{fileName}' does not exist",
                });
            }
            var filter = LoadFromFile<WebhookFilter>(filePath);

            return new JsonResult(new
            {
                status = "OK",
                data = new
                {
                    filter,
                },
            });
        }

        #endregion

        #region Embeds API

        [HttpGet("embeds")]
        [Produces(MediaTypeNames.Application.Json)]
        public IActionResult GetEmbeds()
        {
            var files = Directory.GetFiles(Strings.EmbedsFolder, "*.json");
            var embeds = new List<dynamic>();
            foreach (var file in files)
            {
                try
                {
                    var name = Path.GetFileNameWithoutExtension(file);
                    var embed = LoadFromFile<EmbedMessage>(file);
                    embeds.Add(new
                    {
                        id = name,
                        pokemon = embed.ContainsKey(EmbedMessageType.Pokemon),
                        pokemon_missing_stats = embed.ContainsKey(EmbedMessageType.PokemonMissingStats),
                        raids = embed.ContainsKey(EmbedMessageType.Raids),
                        eggs = embed.ContainsKey(EmbedMessageType.Eggs),
                        gyms = embed.ContainsKey(EmbedMessageType.Gyms),
                        pokestops = embed.ContainsKey(EmbedMessageType.Pokestops),
                        quests = embed.ContainsKey(EmbedMessageType.Quests),
                        lures = embed.ContainsKey(EmbedMessageType.Lures),
                        invasions = embed.ContainsKey(EmbedMessageType.Invasions),
                        nests = embed.ContainsKey(EmbedMessageType.Nests),
                        weather = embed.ContainsKey(EmbedMessageType.Weather),
                    });
                }
                catch (Exception ex)
                {
                    _logger.Error($"Failed to parse embed: {file}\nError: {ex}");
                }
            }
            return new JsonResult(embeds);
        }

        [HttpGet("embed/{fileName}")]
        [Produces(MediaTypeNames.Application.Json)]
        public IActionResult GetEmbed(string fileName)
        {
            var filePath = Path.Combine(Strings.EmbedsFolder, fileName + ".json");
            if (!System.IO.File.Exists(filePath))
            {
                return new JsonResult(new
                {
                    status = "Error",
                    error = $"Embed '{fileName}' does not exist",
                });
            }
            var embed = LoadFromFile<EmbedMessage>(filePath);

            var embedFiles = Directory.GetFiles(Strings.EmbedsFolder, "*.json")
                                      .Select(f => Path.GetExtension(f));

            return new JsonResult(new
            {
                status = "OK",
                data = new
                {
                    embed,
                    placeholders = GetDtsPlaceholders(),
                },
            });
        }

        #endregion

        #region Geofences API

        [HttpGet("geofences")]
        [Produces(MediaTypeNames.Application.Json)]
        public IActionResult GetGeofences()
        {
            var files = Directory.GetFiles(Strings.GeofencesFolder);
            var configs = files.Select(file => new {
                id = Path.GetFileName(file),
            });
            return new JsonResult(configs);
        }

        [HttpGet("geofence/{fileName}")]
        [Produces(MediaTypeNames.Application.Json)]
        public IActionResult GetGeofence(string fileName)
        {
            var filePath = Path.Combine(Strings.GeofencesFolder, fileName);
            if (!System.IO.File.Exists(filePath))
            {
                return BadRequest($"Geofence '{fileName}' does not exist");
            }
            var geofence = LoadFromFile(filePath);

            return new JsonResult(new
            {
                data = new
                {
                    name = Path.GetFileNameWithoutExtension(fileName),
                    geofence,
                    format = Path.GetExtension(fileName)
                },
            });
        }

        #endregion

        #region Discord Roles API

        [HttpGet("roles")]
        [Produces(MediaTypeNames.Application.Json)]
        public IActionResult GetDiscordRoles()
        {
            var roles = GetRoles();
            var result = new List<dynamic>();
            foreach (var (roleId, role) in roles)
            {
                result.Add(new
                {
                    id = roleId,
                    name = role.Name,
                    permissions = role.Permissions,
                    isModerator = role.IsModerator,
                });
            }
            return new JsonResult(result);
        }

        [HttpGet("role/{name}")]
        [Produces(MediaTypeNames.Application.Json)]
        public IActionResult GetDiscordRole(string name)
        {
            var roles = GetRoles();
            var (roleId, role) = roles.FirstOrDefault(role =>
                string.Equals(role.Value.Name, name, StringComparison.InvariantCultureIgnoreCase));
            return new JsonResult(new
            {
                status = "OK",
                data = new
                {
                    roleId = roleId.ToString(),
                    role,
                },
            });
        }

        [HttpPost("role/new")]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<IActionResult> CreateDiscordRole()
        {
            var data = await Request.GetRawBodyStringAsync();
            var dict = data.FromJson<Dictionary<string, object>>();
            var roles = GetRoles();

            var name = Convert.ToString(dict["name"]);
            var roleId = Convert.ToUInt64(dict["roleId"].ToString());
            var permissions = dict["permissions"].ToString();
            var permissionsList = permissions.FromJson<List<SubscriptionAccessType>>();
            var roleConfig = new RoleConfig
            {
                Name = name,
                IsModerator = Convert.ToBoolean(dict["moderator"].ToString()),
                Permissions = permissionsList,
            };

            // TODO: Check if already exists

            if (!roles.ContainsKey(roleId))
            {
                roles.Add(roleId, roleConfig);
            }
            else
            {
                roles[roleId] = roleConfig;
            }

            var path = Strings.BasePath + "wwwroot/static/data/roles.json";
            await WriteDataAsync(path, roles);
            return new JsonResult(new
            {
                status = "OK",
                message = $"Discord role {name} succuessfully created.",
            });
        }

        [HttpPost("role/{name}")]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<IActionResult> UpdateDiscordRole(string name)
        {
            var data = await Request.GetRawBodyStringAsync();
            var dict = data.FromJson<Dictionary<string, object>>();
            var roles = GetRoles();

            var newName = Convert.ToString(dict["name"]);
            var roleId = Convert.ToUInt64(dict["roleId"].ToString());
            var permissions = dict["permissions"].ToString();
            var permissionsList = permissions.FromJson<List<SubscriptionAccessType>>();
            var roleConfig = new RoleConfig
            {
                Name = newName,
                IsModerator = Convert.ToBoolean(dict["moderator"].ToString()),
                Permissions = permissionsList,
            };

            if (!roles.ContainsKey(roleId))
            {
                roles.Add(roleId, roleConfig);
            }
            else
            {
                roles[roleId] = roleConfig;
            }

            var path = Strings.BasePath + "wwwroot/static/data/roles.json";
            await WriteDataAsync(path, roles);
            return new JsonResult(new
            {
                status = "OK",
                message = $"Discord role {name} succuessfully updated.",
            });
        }

        #endregion

        #region Users API

        [HttpGet("users")]
        [Produces(MediaTypeNames.Application.Json)]
        public IActionResult GetUsers()
        {
            return new JsonResult(new { });
        }

        #endregion

        #region Settings API

        [HttpGet("settings")]
        [Produces(MediaTypeNames.Application.Json)]
        public IActionResult GetSettings()
        {
            return new JsonResult(new { });
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
            var path = Strings.BasePath + "wwwroot/static/data/dts_placeholders.json";
            var placeholders = LoadFromFile<Dictionary<string, List<DtsPlaceholder>>>(path);
            return placeholders;
        }

        private static Dictionary<ulong, RoleConfig> GetRoles()
        {
            var path = Strings.BasePath + "wwwroot/static/data/roles.json";
            var roles = LoadFromFile<Dictionary<ulong, RoleConfig>>(path);
            return roles;
        }

        private static async Task WriteDataAsync<T>(string path, T data)
        {
            var json = data.ToJson();
            await WriteDataAsync(path, json);
        }

        private static async Task WriteDataAsync(string path, string data)
        {
            await System.IO.File.WriteAllTextAsync(path, data, Encoding.UTF8);
        }

        private static async Task SaveGeofence(string fileName, string newName, string geofenceData, string geofenceType)
        {
            // TODO: Check if exists or not
            var newFileName = $"{newName}.{geofenceType}";
            var newFilePath = Path.Combine(Strings.GeofencesFolder, newFileName);
            if (!string.Equals(fileName, newFileName))
            {
                // Move file to new path
                System.IO.File.Move(
                    Path.Combine(Strings.GeofencesFolder, fileName),
                    newFilePath
                );
            }
            // Save json
            await WriteDataAsync(newFilePath, geofenceData);
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
                discord.DonorRoleIds = donorRoles;//(Dictionary<ulong, IEnumerable<SubscriptionAccessType>>)donorRoles;
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
            discord.Subscriptions.EmbedsFile = form["embed"].ToString();
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
            var permissionsList = permissions.Split(',')
                                             .Select(x => x.StringToObject<SubscriptionAccessType>())
                                             .ToList();
            if (roles.ContainsKey(id))
            {
                roles[id].Name = name;
                roles[id].Permissions = permissionsList;
                roles[id].IsModerator = isModerator;
            }
            else
            {
                roles.Add(id, new RoleConfig
                {
                    Name = name,
                    Permissions = permissionsList,
                    IsModerator = isModerator,
                });
            }
            return roles;
        }

        #endregion

        #endregion
    }
}