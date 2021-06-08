namespace WhMgr.Services.Alarms
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    using DSharpPlus;
    using Microsoft.Extensions.Logging;

    using WhMgr.Configuration;
    using WhMgr.Services.Alarms.Filters;
    using WhMgr.Services.Alarms.Models;
    using WhMgr.Services.Geofence;
    using WhMgr.Services.Webhook.Models;
    using WhMgr.Utilities;

    public class AlarmControllerService : IAlarmControllerService
    {
        private readonly ILogger<AlarmControllerService> _logger;
        private readonly Dictionary<ulong, ChannelAlarmsManifest> _alarms;
        private readonly Dictionary<ulong, DiscordClient> _discordClients;
        private readonly ConfigHolder _config;

        public AlarmControllerService(
            ILogger<AlarmControllerService> logger,
            Dictionary<ulong, ChannelAlarmsManifest> alarms,
            Dictionary<ulong, DiscordClient> discordClients,
            ConfigHolder config)
        {
            _logger = logger;
            _alarms = alarms;
            _discordClients = discordClients;
            _config = config;
            _logger.LogInformation($"Alarms {_alarms?.Keys?.Count:N0}");
        }

        public void ProcessPokemonAlarms(PokemonData pokemon)
        {
            if (pokemon == null)
                return;

            /*
            Statistics.Instance.TotalReceivedPokemon++;
            if (pokemon.IsMissingStats)
                Statistics.Instance.TotalReceivedPokemonMissingStats++;
            else
                Statistics.Instance.TotalReceivedPokemonWithStats++;
            */

            foreach (var (guildId, alarms) in _alarms.Where(x => x.Value.EnablePokemon))
            {
                var pokemonAlarms = alarms.Alarms?.FindAll(x => x.Filters?.Pokemon?.Pokemon != null && x.Filters.Pokemon.Enabled);
                if (pokemonAlarms == null)
                    continue;

                for (var i = 0; i < pokemonAlarms.Count; i++)
                {
                    var alarm = pokemonAlarms[i];
                    if (alarm.Filters.Pokemon == null)
                        continue;

                    if (!alarm.Filters.Pokemon.Enabled)
                    {
                        //_logger.Info($"[{alarm.Name}] Skipping pokemon {pkmn.Id}: Pokemon filter not enabled.");
                        continue;
                    }

                    var geofence = GeofenceService.GetGeofence(alarm.GeofenceItems, new Coordinate(pokemon.Latitude, pokemon.Longitude));
                    if (geofence == null)
                    {
                        //_logger.Info($"[{alarm.Name}] Skipping pokemon {pkmn.Id}: not in geofence.");
                        continue;
                    }

                    if ((alarm.Filters.Pokemon.IsEvent && !(pokemon.IsEvent.HasValue && pokemon.IsEvent.Value)) ||
                        (!alarm.Filters.Pokemon.IsEvent && pokemon.IsEvent.HasValue && pokemon.IsEvent.Value))
                    {
                        // Pokemon does not have event flag indicating it was checked with event account and event filter is set, skip.
                        // or Pokemon has event but filter is set to not include them
                        continue;
                    }

                    if (alarm.Filters.Pokemon.FilterType == FilterType.Exclude && alarm.Filters.Pokemon.Pokemon.Contains(pokemon.Id))
                    {
                        //_logger.Info($"[{alarm.Name}] [{geofence.Name}] Skipping pokemon {pkmn.Id}: filter {alarm.Filters.Pokemon.FilterType}.");
                        continue;
                    }

                    if (alarm.Filters.Pokemon.FilterType == FilterType.Include && alarm.Filters.Pokemon.Pokemon?.Count > 0 && !alarm.Filters.Pokemon.Pokemon.Contains(pokemon.Id))
                    {
                        //_logger.Info($"[{alarm.Name}] [{geofence.Name}] Skipping pokemon {pkmn.Id}: filter {alarm.Filters.Pokemon.FilterType}.");
                        continue;
                    }

                    var formName = pokemon.FormId.ToString();// TODO: Translator.Instance.GetFormName(pokemon.FormId)?.ToLower();
                    if (alarm.Filters.Pokemon.FilterType == FilterType.Exclude && alarm.Filters.Pokemon.Forms.Select(x => x.ToLower()).Contains(formName))
                    {
                        //_logger.Info($"[{alarm.Name}] [{geofence.Name}] Skipping pokemon {pkmn.Id} with form {pkmn.FormId} ({formName}): filter {alarm.Filters.Pokemon.FilterType}.");
                        continue;
                    }

                    if (alarm.Filters.Pokemon.FilterType == FilterType.Include && alarm.Filters.Pokemon.Forms?.Count > 0 && !alarm.Filters.Pokemon.Forms.Select(x => x.ToLower()).Contains(formName))
                    {
                        //_logger.Info($"[{alarm.Name}] [{geofence.Name}] Skipping pokemon {pkmn.Id} with form {pkmn.FormId} ({formName}): filter {alarm.Filters.Pokemon.FilterType}.");
                        continue;
                    }

                    var costumeName = pokemon.Costume.ToString();//Translator.Instance.GetCostumeName(pokemon.Costume)?.ToLower();
                    if (alarm.Filters.Pokemon.FilterType == FilterType.Exclude && alarm.Filters.Pokemon.Costumes.Select(x => x.ToLower()).Contains(costumeName))
                    {
                        //_logger.Info($"[{alarm.Name}] [{geofence.Name}] Skipping pokemon {pkmn.Id} with costume {pkmn.Costume} ({costumeName}): filter {alarm.Filters.Pokemon.FilterType}.");
                        continue;
                    }

                    if (alarm.Filters.Pokemon.FilterType == FilterType.Include && alarm.Filters.Pokemon.Costumes?.Count > 0 && !alarm.Filters.Pokemon.Costumes.Select(x => x.ToLower()).Contains(costumeName))
                    {
                        //_logger.Info($"[{alarm.Name}] [{geofence.Name}] Skipping pokemon {pkmn.Id} with costume {pkmn.Costume} ({costumeName}): filter {alarm.Filters.Pokemon.FilterType}.");
                        continue;
                    }

                    if (alarm.Filters.Pokemon.IgnoreMissing && pokemon.IsMissingStats)
                    {
                        //_logger.Info($"[{alarm.Name}] [{geofence.Name}] Skipping pokemon {pkmn.Id}: IgnoreMissing=true.");
                        continue;
                    }

                    if (!Filters.Filters.MatchesIV(pokemon.IV, alarm.Filters.Pokemon.MinimumIV, alarm.Filters.Pokemon.MaximumIV))
                    {
                        //_logger.Info($"[{alarm.Name}] [{geofence.Name}] Skipping pokemon {pkmn.Id}: MinimumIV={alarm.Filters.Pokemon.MinimumIV} and MaximumIV={alarm.Filters.Pokemon.MaximumIV} and IV={pkmn.IV}.");
                        continue;
                    }

                    if (!Filters.Filters.MatchesCP(pokemon.CP, alarm.Filters.Pokemon.MinimumCP, alarm.Filters.Pokemon.MaximumCP))
                    {
                        //_logger.Info($"[{alarm.Name}] [{geofence.Name}] Skipping pokemon {pkmn.Id}: MinimumCP={alarm.Filters.Pokemon.MinimumCP} and MaximumCP={alarm.Filters.Pokemon.MaximumCP} and CP={pkmn.CP}.");
                        continue;
                    }

                    if (!Filters.Filters.MatchesLvl(pokemon.Level, alarm.Filters.Pokemon.MinimumLevel, alarm.Filters.Pokemon.MaximumLevel))
                    {
                        //_logger.Info($"[{alarm.Name}] [{geofence.Name}] Skipping pokemon {pkmn.Id}: MinimumLevel={alarm.Filters.Pokemon.MinimumLevel} and MaximumLevel={alarm.Filters.Pokemon.MaximumLevel} and Level={pkmn.Level}.");
                        continue;
                    }

                    var skipGreatLeague = alarm.Filters.Pokemon.IsPvpGreatLeague &&
                        !(pokemon.MatchesGreatLeague && pokemon.GreatLeague.Exists(x =>
                            Filters.Filters.MatchesPvPRank(x.Rank ?? 4096, alarm.Filters.Pokemon.MinimumRank, alarm.Filters.Pokemon.MaximumRank)
                            && x.CP >= Strings.MinimumGreatLeagueCP && x.CP <= Strings.MaximumGreatLeagueCP));
                    if (skipGreatLeague)
                        continue;

                    var skipUltraLeague = alarm.Filters.Pokemon.IsPvpUltraLeague &&
                        !(pokemon.MatchesUltraLeague && pokemon.UltraLeague.Exists(x =>
                            Filters.Filters.MatchesPvPRank(x.Rank ?? 4096, alarm.Filters.Pokemon.MinimumRank, alarm.Filters.Pokemon.MaximumRank)
                            && x.CP >= Strings.MinimumUltraLeagueCP && x.CP <= Strings.MaximumUltraLeagueCP));
                    if (skipUltraLeague)
                        continue;

                    //if (!Filters.MatchesGender(pkmn.Gender, alarm.Filters.Pokemon.Gender.ToString()))
                    //{
                    //    //_logger.Info($"[{alarm.Name}] [{geofence.Name}] Skipping pokemon {pkmn.Id}: DesiredGender={alarm.Filters.Pokemon.Gender} and Gender={pkmn.Gender}.");
                    //    continue;
                    //}

                    //
                    // TODO: if ((alarm.Filters?.Pokemon?.IgnoreMissing ?? false) && !(pokemon.Height != null && pokemon.Weight != null && Filters.Filters.MatchesSize(pokemon.Id.GetSize(pokemon.Height, pokemon.Weight), alarm.Filters?.Pokemon?.Size)))
                    //{
                    //    continue;
                    //}

                    //OnPokemonAlarmTriggered(pokemon, alarm, guildId);
                    // TODO: ThreadPool.QueueUSerWorkItem
                    if (!ThreadPool.QueueUserWorkItem(x => SendPokemonEmbed(guildId, alarm, pokemon, geofence.Name)))
                    {
                        Console.WriteLine($"Failed to queue Pokemon alarm: {alarm.Name} for Pokemon {pokemon.Id} ({pokemon.EncounterId})");
                        continue;
                    }
                    Console.WriteLine($"Sending pokemon {pokemon.Id} {pokemon.IV} to Discord {guildId}");
                }
            }
        }

        private void SendPokemonEmbed(ulong guildId, ChannelAlarm alarm, PokemonData pokemon, string city)
        {
            if (string.IsNullOrEmpty(alarm.Webhook))
                return;

            _logger.LogInformation($"Pokemon Found [Alarm: {alarm.Name}, Pokemon: {pokemon.Id}, Despawn: {pokemon.DespawnTime}]");

            if (!_discordClients.ContainsKey(guildId))
                return;

            if (!_config.Instance.Servers.ContainsKey(guildId))
                return;

            try
            {
                var server = _config.Instance.Servers[guildId];
                var client = _discordClients[guildId];
                var eb = pokemon.GenerateEmbedMessage(new AlarmMessageSettings
                {
                    GuildId = guildId,
                    Client = client,
                    Config = _config,
                    Alarm = alarm,
                    City = city,
                });
                var json = eb.Build();
                NetUtils.SendWebhook(alarm.Webhook, json);
                /*
                Statistics.Instance.PokemonAlarmsSent++;

                if (pokemon.IV == "100%")
                {
                    Statistics.Instance.AddHundredIV(pokemon);
                }
                */
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex}");
            }
        }
    }
}