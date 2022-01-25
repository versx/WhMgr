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

        [HttpGet("configs")]
        [Produces("application/json")]
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

        [HttpGet("discords")]
        [Produces("application/json")]
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

        [HttpGet("alarms")]
        [Produces("application/json")]
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

        [HttpGet("filters")]
        [Produces("application/json")]
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

        [HttpGet("embeds")]
        [Produces("application/json")]
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

        [HttpGet("geofences")]
        [Produces("application/json")]
        public IActionResult GetGeofences()
        {
            var files = Directory.GetFiles(Strings.GeofencesFolder);
            var configs = files.Select(file => new {
                id = Path.GetFileName(file),
            });
            return new JsonResult(configs);
        }

        [HttpGet("roles")]
        [Produces("application/json")]
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
                    is_moderator = role.IsModerator,
                });
            }
            return new JsonResult(result);
        }

        [HttpGet("users")]
        [Produces("application/json")]
        public IActionResult GetUsers()
        {
            return new JsonResult(new { });
        }

        [HttpGet("settings")]
        [Produces("application/json")]
        public IActionResult GetSettings()
        {
            return new JsonResult(new { });
        }

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

        private static Dictionary<ulong, RoleConfig> GetRoles()
        {
            var path = "wwwroot/static/data/roles.json";
            var roles = LoadFromFile<Dictionary<ulong, RoleConfig>>(path);
            return roles;
        }


        #endregion
    }
}