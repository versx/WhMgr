namespace WhMgr.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net.Mime;
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
                return SendErrorResponse($"Config '{fileName}' does not exist.");
            }
            var config = LoadFromFile<Config>(filePath);
            var discordFiles = Directory.GetFiles(Strings.DiscordsFolder, "*.json");
            var discords = discordFiles.ToDictionary(
                x => Path.GetFileName(x),
                y => System.IO.File.ReadAllText(y).FromJson<DiscordServerConfig>().Bot.GuildId.ToString());
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
                    discords,// = discordFiles.Select(file => Path.GetFileName(file)),
                    locales,
                },
            });
        }

        [HttpPost("config/new")]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<IActionResult> CreateConfig()
        {
            var data = await Request.GetRawBodyStringAsync();
            var dict = data.FromJson<Dictionary<string, object>>();

            // Validate keys exist
            if (!dict.ContainsKey("name"))
            {
                return SendErrorResponse($"One or more required properties not specified.");
            }

            var name = dict["name"].ToString();
            var config = data.FromJson<Config>();

            // Save json
            var json = config.ToJson();
            var path = Path.Combine(Strings.ConfigsFolder, name + ".json");
            if (System.IO.File.Exists(path))
            {
                return SendErrorResponse($"Failed to create config '{name}', config already exists.");
            }

            await WriteDataAsync(path, json);

            return new JsonResult(new
            {
                status = "OK",
                message = $"Config '{name}' succuessfully created.",
            });
        }

        [HttpPut("config/{fileName}")]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<IActionResult> UpdateConfig(string fileName)
        {
            var path = Path.Combine(Strings.ConfigsFolder, fileName + ".json");
            if (!System.IO.File.Exists(path))
            {
                return SendErrorResponse($"Failed to update config '{fileName}', config does not exist.");
            }

            var data = await Request.GetRawBodyStringAsync();
            var dict = data.FromJson<Dictionary<string, object>>();

            // Validate keys exist
            if (!dict.ContainsKey("name"))
            {
                return SendErrorResponse($"One or more required properties not specified.");
            }

            var newName = dict["name"].ToString();
            var config = data.FromJson<Config>();

            // TODO: Check if new alarm already exists or not
            var newFileName = $"{newName}.json";
            var newFilePath = Path.Combine(Strings.ConfigsFolder, newFileName);
            if (!string.Equals(fileName + ".json", newFileName))
            {
                // Move file to new path
                System.IO.File.Move(
                    Path.Combine(Strings.DiscordsFolder, fileName + ".json"),
                    newFilePath
                );
            }

            // Save json
            var json = config.ToJson();
            await WriteDataAsync(newFilePath, json);

            return new JsonResult(new
            {
                status = "OK",
                message = $"Config '{fileName}' successfully updated.",
            });
        }

        [HttpDelete("config/{fileName}")]
        [Produces(MediaTypeNames.Application.Json)]
        public IActionResult DeleteConfig(string fileName)
        {
            var path = Path.Combine(Strings.ConfigsFolder, fileName + ".json");
            if (!System.IO.File.Exists(path))
            {
                return SendErrorResponse($"Failed to delete config '{fileName}', config does not exist.");
            }

            // Delete config
            System.IO.File.Delete(path);

            return new JsonResult(new
            {
                status = "OK",
                message = $"Config '{fileName}' succuessfully deleted.",
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
                return SendErrorResponse($"Discord '{fileName}' does not exist.");
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

        [HttpPost("discord/new")]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<IActionResult> CreateDiscord()
        {
            var data = await Request.GetRawBodyStringAsync();
            var dict = data.FromJson<Dictionary<string, object>>();

            // Validate keys exist
            if (!dict.ContainsKey("name"))
            {
                return SendErrorResponse($"One or more required properties not specified.");
            }

            var name = dict["name"].ToString();
            var discord = data.FromJson<DiscordServerConfig>();

            // Save json
            var json = discord.ToJson();
            var path = Path.Combine(Strings.DiscordsFolder, name + ".json");
            if (System.IO.File.Exists(path))
            {
                return SendErrorResponse($"Failed to create discord server '{name}', discord server already exists.");
            }

            await WriteDataAsync(path, json);

            return new JsonResult(new
            {
                status = "OK",
                message = $"Discord server '{name}' succuessfully created.",
            });
        }

        [HttpPut("discord/{fileName}")]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<IActionResult> UpdateDiscord(string fileName)
        {
            var path = Path.Combine(Strings.DiscordsFolder, fileName + ".json");
            if (!System.IO.File.Exists(path))
            {
                return SendErrorResponse($"Failed to update Discord server '{fileName}', Discord server does not exist.");
            }

            var data = await Request.GetRawBodyStringAsync();
            var dict = data.FromJson<Dictionary<string, object>>();

            // Validate keys exist
            if (!dict.ContainsKey("name"))// ||
                //!dict.ContainsKey("discord"))
            {
                return SendErrorResponse($"One or more required properties not specified.");
            }

            var newName = dict["name"].ToString();
            var discord = data.FromJson<DiscordServerConfig>();

            // TODO: Check if new alarm already exists or not
            var newFileName = $"{newName}.json";
            var newFilePath = Path.Combine(Strings.DiscordsFolder, newFileName);
            if (!string.Equals(fileName + ".json", newFileName))
            {
                // Move file to new path
                System.IO.File.Move(
                    Path.Combine(Strings.DiscordsFolder, fileName + ".json"),
                    newFilePath
                );
            }

            // Save json
            var json = discord.ToJson();
            await WriteDataAsync(newFilePath, json);

            return new JsonResult(new
            {
                status = "OK",
                message = $"Discord server '{fileName}' successfully updated.",
            });
        }

        [HttpDelete("discord/{fileName}")]
        [Produces(MediaTypeNames.Application.Json)]
        public IActionResult DeleteDiscord(string fileName)
        {
            var path = Path.Combine(Strings.DiscordsFolder, fileName + ".json");
            if (!System.IO.File.Exists(path))
            {
                return SendErrorResponse($"Failed to delete discord server '{fileName}', discord server does not exist.");
            }

            // Delete discord server config
            System.IO.File.Delete(path);

            return new JsonResult(new
            {
                status = "OK",
                message = $"Discord server '{fileName}' succuessfully deleted.",
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

        [HttpPost("alarm/new")]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<IActionResult> CreateAlarm()
        {
            var data = await Request.GetRawBodyStringAsync();
            var dict = data.FromJson<Dictionary<string, object>>();

            // Validate keys exist
            if (!dict.ContainsKey("name") ||
                !dict.ContainsKey("alarm"))
            {
                return SendErrorResponse($"One or more required properties not specified.");
            }

            var name = dict["name"].ToString();
            var alarmJson = dict["alarm"].ToString();
            var alarm = alarmJson.FromJson<ChannelAlarmsManifest>();

            // Save json
            var json = alarm.ToJson();
            var path = Path.Combine(Strings.AlarmsFolder, name + ".json");
            if (System.IO.File.Exists(path))
            {
                return SendErrorResponse($"Failed to create alarm '{name}', alarm already exists.");
            }

            await WriteDataAsync(path, json);

            return new JsonResult(new
            {
                status = "OK",
                message = $"Alarm '{name}' succuessfully created.",
            });
        }

        [HttpPut("alarm/{fileName}")]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<IActionResult> UpdateAlarm(string fileName)
        {
            var path = Path.Combine(Strings.AlarmsFolder, fileName + ".json");
            if (!System.IO.File.Exists(path))
            {
                return SendErrorResponse($"Failed to update alarm '{fileName}', alarm does not exist.");
            }

            var data = await Request.GetRawBodyStringAsync();
            var dict = data.FromJson<Dictionary<string, object>>();

            // Validate keys exist
            if (!dict.ContainsKey("name") ||
                !dict.ContainsKey("alarm"))
            {
                return SendErrorResponse($"One or more required properties not specified.");
            }

            var newName = dict["name"].ToString();

            var alarmJson = dict["alarm"].ToString();
            var alarm = alarmJson.FromJson<ChannelAlarmsManifest>();

            // TODO: Check if new alarm already exists or not
            var newFileName = $"{newName}.json";
            var newFilePath = Path.Combine(Strings.AlarmsFolder, newFileName);
            if (!string.Equals(fileName + ".json", newFileName))
            {
                // Move file to new path
                System.IO.File.Move(
                    Path.Combine(Strings.AlarmsFolder, fileName + ".json"),
                    newFilePath
                );
            }

            // Save json
            var json = alarm.ToJson();
            await WriteDataAsync(newFilePath, json);

            return new JsonResult(new
            {
                status = "OK",
                message = $"Alarm '{fileName}' successfully updated.",
            });
        }

        [HttpDelete("alarm/{fileName}")]
        [Produces(MediaTypeNames.Application.Json)]
        public IActionResult DeleteAlarm(string fileName)
        {
            var path = Path.Combine(Strings.AlarmsFolder, fileName + ".json");
            if (!System.IO.File.Exists(path))
            {
                return SendErrorResponse($"Failed to delete alarm '{fileName}', alarm does not exist.");
            }

            // Delete geofence
            System.IO.File.Delete(path);

            return new JsonResult(new
            {
                status = "OK",
                message = $"Alarm '{fileName}' succuessfully deleted.",
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
                return SendErrorResponse($"Filter '{fileName}' does not exist.");
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

        // TODO: Create filter

        [HttpPut("filter/{fileName}")]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<IActionResult> UpdateFilter(string fileName)
        {
            var path = Path.Combine(Strings.FiltersFolder, fileName + ".json");
            if (!System.IO.File.Exists(path))
            {
                return SendErrorResponse($"Failed to update filter '{fileName}', filter does not exist.");
            }

            var data = await Request.GetRawBodyStringAsync();
            var dict = data.FromJson<Dictionary<string, object>>();

            // Validate keys exist
            if (!dict.ContainsKey("name") ||
                !dict.ContainsKey("filter"))
            {
                return SendErrorResponse($"One or more required properties not specified.");
            }

            var newName = dict["name"].ToString();

            var filterJson = dict["filter"].ToString();
            var filter = filterJson.FromJson<WebhookFilter>();

            // TODO: Check if new filter already exists or not
            var newFileName = $"{newName}.json";
            var newFilePath = Path.Combine(Strings.FiltersFolder, newFileName);
            if (!string.Equals(fileName + ".json", newFileName))
            {
                // Move file to new path
                System.IO.File.Move(
                    Path.Combine(Strings.FiltersFolder, fileName + ".json"),
                    newFilePath
                );
            }

            // Save json
            var json = filter.ToJson();
            await WriteDataAsync(newFilePath, json);

            return new JsonResult(new
            {
                status = "OK",
                message = $"Filter '{fileName}' succuessfully updated.",
            });
        }

        [HttpDelete("filter/{fileName}")]
        [Produces(MediaTypeNames.Application.Json)]
        public IActionResult DeleteFilter(string fileName)
        {
            var path = Path.Combine(Strings.FiltersFolder, fileName + ".json");
            if (!System.IO.File.Exists(path))
            {
                return SendErrorResponse($"Failed to delete filter '{fileName}', filter does not exist.");
            }

            // Delete geofence
            System.IO.File.Delete(path);

            return new JsonResult(new
            {
                status = "OK",
                message = $"Filter '{fileName}' succuessfully deleted.",
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
                return SendErrorResponse($"Embed '{fileName}' does not exist.");
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

        [HttpPost("embed/new")]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<IActionResult> CreateEmbed()
        {
            var data = await Request.GetRawBodyStringAsync();
            var dict = data.FromJson<Dictionary<string, object>>();

            // Validate keys exist
            if (!dict.ContainsKey("name") ||
                !dict.ContainsKey("embed"))
            {
                return SendErrorResponse($"One or more required properties not specified.");
            }

            var name = dict["name"].ToString();
            var embedJson = dict["embed"].ToString();
            var embed = embedJson.FromJson<EmbedMessage>();

            // Save json
            var json = embed.ToJson();
            var path = Path.Combine(Strings.EmbedsFolder, name + ".json");
            if (System.IO.File.Exists(path))
            {
                return SendErrorResponse($"Failed to create embed '{name}', embed already exists.");
            }

            await WriteDataAsync(path, json);

            return new JsonResult(new
            {
                status = "OK",
                message = $"Embed '{name}' succuessfully created.",
            });
        }

        [HttpPut("embed/{fileName}")]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<IActionResult> UpdateEmbed(string fileName)
        {
            var path = Path.Combine(Strings.EmbedsFolder, fileName + ".json");
            if (!System.IO.File.Exists(path))
            {
                return SendErrorResponse($"Failed to update embed '{fileName}', embed does not exist.");
            }

            var data = await Request.GetRawBodyStringAsync();
            var dict = data.FromJson<Dictionary<string, object>>();

            // Validate keys exist
            if (!dict.ContainsKey("name") ||
                !dict.ContainsKey("embed"))
            {
                return SendErrorResponse($"One or more required properties not specified.");
            }

            var newName = dict["name"].ToString();

            var embedJson = dict["embed"].ToString();
            var embed = embedJson.FromJson<EmbedMessage>();

            // TODO: Check if new embed already exists or not
            var newFileName = $"{newName}.json";
            var newFilePath = Path.Combine(Strings.EmbedsFolder, newFileName);
            if (!string.Equals(fileName + ".json", newFileName))
            {
                // Move file to new path
                System.IO.File.Move(
                    Path.Combine(Strings.EmbedsFolder, fileName + ".json"),
                    newFilePath
                );
            }

            // Save json
            var json = embed.ToJson();
            await WriteDataAsync(newFilePath, json);

            return new JsonResult(new
            {
                status = "OK",
                message = $"Embed '{fileName}' succuessfully updated.",
            });
        }

        [HttpDelete("embed/{fileName}")]
        [Produces(MediaTypeNames.Application.Json)]
        public IActionResult DeleteEmbed(string fileName)
        {
            var path = Path.Combine(Strings.EmbedsFolder, fileName + ".json");
            if (!System.IO.File.Exists(path))
            {
                return SendErrorResponse($"Failed to delete embed '{fileName}', embed does not exist.");
            }

            // Delete geofence
            System.IO.File.Delete(path);

            return new JsonResult(new
            {
                status = "OK",
                message = $"Embed '{fileName}' succuessfully deleted.",
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
                status = "OK",
                data = new
                {
                    name = Path.GetFileNameWithoutExtension(fileName),
                    geofence,
                    format = Path.GetExtension(fileName)
                },
            });
        }

        [HttpPost("geofence/new")]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<IActionResult> CreateGeofence()
        {
            var data = await Request.GetRawBodyStringAsync();
            var dict = data.FromJson<Dictionary<string, object>>();
            // Validate keys exist
            if (!dict.ContainsKey("name") ||
                !dict.ContainsKey("format") ||
                !dict.ContainsKey("geofence"))
            {
                return SendErrorResponse($"One or more required properties not specified.");
            }

            var name = dict["name"].ToString();
            var saveFormat = dict["format"].ToString();
            var geofenceData = dict["geofence"].ToString();

            var fileName = name + saveFormat;
            var path = Path.Combine(Strings.GeofencesFolder, fileName);
            if (System.IO.File.Exists(path))
            {
                return SendErrorResponse($"Failed to create geofence '{fileName}', geofence already exists.");
            }

            await SaveGeofence(name, name, geofenceData, saveFormat);
            return new JsonResult(new
            {
                status = "OK",
                message = $"Geofence '{name}' succuessfully created.",
            });
        }

        [HttpPut("geofence/{fileName}")]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<IActionResult> UpdateGeofence(string fileName)
        {
            var data = await Request.GetRawBodyStringAsync();
            var dict = data.FromJson<Dictionary<string, object>>();
            // Validate keys exist
            if (!dict.ContainsKey("name") ||
                !dict.ContainsKey("format") ||
                !dict.ContainsKey("geofence"))
            {
                return SendErrorResponse($"One or more required properties not specified.");
            }

            var name = dict["name"].ToString();
            var saveFormat = dict["format"].ToString();
            var geofenceData = dict["geofence"].ToString();

            await SaveGeofence(fileName, name, geofenceData, saveFormat);
            return new JsonResult(new
            {
                status = "OK",
                message = $"Geofence '{name}' succuessfully updated.",
            });
        }

        [HttpDelete("geofence/{fileName}")]
        [Produces(MediaTypeNames.Application.Json)]
        public IActionResult DeleteGeofence(string fileName)
        {
            var path = Path.Combine(Strings.GeofencesFolder, fileName);
            if (!System.IO.File.Exists(path))
            {
                return SendErrorResponse($"Failed to delete geofence '{fileName}', geofence does not exist.");
            }

            // Delete geofence
            System.IO.File.Delete(path);

            return new JsonResult(new
            {
                status = "OK",
                message = $"Geofence '{fileName}' succuessfully deleted.",
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
                    moderator = role.IsModerator,
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
            if (role == null)
            {
                return SendErrorResponse($"Failed to get Discord role '{name}', role does not exist.");
            }
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
                message = $"Discord role '{name}' succuessfully created.",
            });
        }

        [HttpPut("role/{name}")]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<IActionResult> UpdateDiscordRole(string name)
        {
            var data = await Request.GetRawBodyStringAsync();
            var dict = data.FromJson<Dictionary<string, object>>();
            // Validate keys exist
            if (!dict.ContainsKey("name") ||
                !dict.ContainsKey("roleId") ||
                !dict.ContainsKey("permissions"))
            {
                return SendErrorResponse($"One or more required properties not specified.");
            }

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
                message = $"Discord role '{name}' succuessfully updated.",
            });
        }

        [HttpDelete("role/{id}")]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<IActionResult> DeleteDiscordRole(ulong id)
        {
            var roles = GetRoles();
            if (!roles.ContainsKey(id))
            {
                return SendErrorResponse($"Failed to delete Discord role '{id}', role does not exist.");
            }

            roles.Remove(id);

            var path = Strings.BasePath + "wwwroot/static/data/roles.json";
            await WriteDataAsync(path, roles);
            return new JsonResult(new
            {
                status = "OK",
                message = $"Discord role '{id}' succuessfully deleted.",
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

        #region Helper API

        [HttpGet("alarm/data")]
        public IActionResult GetAlarmHelper()
        {
            var embedFiles = Directory.GetFiles(Strings.EmbedsFolder, "*.json");
            var filterFiles = Directory.GetFiles(Strings.FiltersFolder, "*.json");
            var geofenceFiles = Directory.GetFiles(Strings.GeofencesFolder);
            return new JsonResult(new
            {
                status = "OK",
                data = new
                {
                    embeds = embedFiles.Select(file => Path.GetFileName(file)),
                    filters = filterFiles.Select(file => Path.GetFileName(file)),
                    geofences = geofenceFiles.Select(file => Path.GetFileName(file)),
                },
            });
        }

        [HttpGet("discord/data")]
        public IActionResult GetDiscordHelper()
        {
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


        [HttpGet("config/data")]
        public IActionResult GetConfigHelper()
        {
            var discords = Directory.GetFiles(Strings.DiscordsFolder, "*.json")
                .Select(file => Path.GetFileName(file));

            return new JsonResult(new
            {
                status = "OK",
                data = new
                {
                    discords,
                },
            });
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
            var path = Strings.WwwRoot + "/static/data/dts_placeholders.json";
            if (!System.IO.File.Exists(path))
            {
                return new Dictionary<string, List<DtsPlaceholder>>();
            }
            var placeholders = LoadFromFile<Dictionary<string, List<DtsPlaceholder>>>(path);
            return placeholders;
        }

        private static Dictionary<ulong, RoleConfig> GetRoles()
        {
            var path = Strings.WwwRoot + "/static/data/roles.json";
            if (System.IO.File.Exists(path))
            {
                return new Dictionary<ulong, RoleConfig>();
            }
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
            var newFileName = $"{newName}{geofenceType}";
            var newFilePath = Path.Combine(Strings.GeofencesFolder, newFileName);
            // TODO: Convert geofence to ini or geojson
            if (!string.Equals(fileName + geofenceType, newFileName))
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

        #region Response Helpers

        private static IActionResult SendErrorResponse(string message)
        {
            return new JsonResult(new
            {
                status = "Error",
                error = message,
            });
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

        // examples: String, Char, Boolean, Number, Array
        [JsonPropertyName("type")]
        public string Type { get; set; }
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