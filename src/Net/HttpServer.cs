namespace WhMgr.Net
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Text;
    using System.Threading;

    using Newtonsoft.Json;

    using WhMgr.Diagnostics;
    using WhMgr.Net.Models;

    /// <summary>
    /// HTTP listener class
    /// </summary>
    public class HttpServer
    {
        #region Variables

        private static readonly IEventLogger _logger = EventLogger.GetLogger("HTTP", Program.LogLevel);
        private static readonly object _lock = new object();
        private readonly Dictionary<ulong, PokemonData> _processedPokemon;
        private readonly Dictionary<string, RaidData> _processedRaids;
        private readonly Dictionary<string, GymData> _processedGyms;
        private readonly Dictionary<string, PokestopData> _processedPokestops;
        private readonly Dictionary<string, QuestData> _processedQuests;
        private readonly Dictionary<string, TeamRocketInvasion> _processedInvasions;
        private readonly Dictionary<long, WeatherData> _processedWeather;
        private HttpListener _server;
        private bool _initialized = false;
        private readonly int _despawnTimerMinimumMinutes = 5;
        private static string _endpoint;

        #endregion

        #region Properties

        /// <summary>
        /// HTTP listening interface/host address
        /// </summary>
        public string Host { get; }

        /// <summary>
        /// Http listening port
        /// </summary>
        public ushort Port { get; }

        /// <summary>
        /// Logs incoming webhook data if set to <c>true</c>
        /// </summary>
        public bool IsDebug { get; set; }

        /// <summary>
        /// Skips webhook raid eggs
        /// </summary>
        public bool SkipEggs { get; set; }

        #endregion

        #region Events

        /// <summary>
        /// Trigged when a Pokemon webhook payload is received.
        /// </summary>
        public event EventHandler<DataReceivedEventArgs<PokemonData>> PokemonReceived;

        private void OnPokemonReceived(PokemonData pokemon) => PokemonReceived?.Invoke(this, new DataReceivedEventArgs<PokemonData>(pokemon));

        /// <summary>
        /// Trigged when a Raid or Raid Egg webhook payload is received.
        /// </summary>
        public event EventHandler<DataReceivedEventArgs<RaidData>> RaidReceived;

        private void OnRaidReceived(RaidData raid) => RaidReceived?.Invoke(this, new DataReceivedEventArgs<RaidData>(raid));

        /// <summary>
        /// Trigged when a Field Research Quest webhook payload is received.
        /// </summary>
        public event EventHandler<DataReceivedEventArgs<QuestData>> QuestReceived;

        private void OnQuestReceived(QuestData quest) => QuestReceived?.Invoke(this, new DataReceivedEventArgs<QuestData>(quest));

        /// <summary>
        /// Trigged when a Pokestop webhook (lure/invasion) payload is received.
        /// </summary>
        public event EventHandler<DataReceivedEventArgs<PokestopData>> PokestopReceived;

        private void OnPokestopReceived(PokestopData pokestop) => PokestopReceived?.Invoke(this, new DataReceivedEventArgs<PokestopData>(pokestop));

        /// <summary>
        /// Trigged when a Gym webhook payload is received.
        /// </summary>
        public event EventHandler<DataReceivedEventArgs<GymData>> GymReceived;

        private void OnGymReceived(GymData gym) => GymReceived?.Invoke(this, new DataReceivedEventArgs<GymData>(gym));

        /// <summary>
        /// Trigged when a Gym Details webhook payload is received.
        /// </summary>
        public event EventHandler<DataReceivedEventArgs<GymDetailsData>> GymDetailsReceived;

        private void OnGymDetailsReceived(GymDetailsData gymDetails) => GymDetailsReceived?.Invoke(this, new DataReceivedEventArgs<GymDetailsData>(gymDetails));

        /// <summary>
        /// Trigged when a Weather webhook payload is received.
        /// </summary>
        public event EventHandler<DataReceivedEventArgs<WeatherData>> WeatherReceived;

        private void OnWeatherReceived(WeatherData weather) => WeatherReceived?.Invoke(this, new DataReceivedEventArgs<WeatherData>(weather));

        #endregion

        #region Constructor

        /// <summary>
        /// Instantiates a new <see cref="HttpServer"/> class.
        /// </summary>
        /// <param name="host">Listing host interface</param>
        /// <param name="port">Listening port</param>
        /// <param name="despawnTimerMinimum">Minimum despawn timer amount in minutes to process Pokemon</param>
        public HttpServer(string host, ushort port, int despawnTimerMinimum)
        {
            // If no host is set use wildcard for all host interfaces
            Host = host ?? "*";
            Port = port;
            _processedPokemon = new Dictionary<ulong, PokemonData>();
            _processedRaids = new Dictionary<string, RaidData>();
            _processedGyms = new Dictionary<string, GymData>();
            _processedPokestops = new Dictionary<string, PokestopData>();
            _processedQuests = new Dictionary<string, QuestData>();
            _processedInvasions = new Dictionary<string, TeamRocketInvasion>();
            _processedWeather = new Dictionary<long, WeatherData>();
            _despawnTimerMinimumMinutes = despawnTimerMinimum;

            Initialize();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Starts the HTTP listener server
        /// </summary>
        public void Start()
        {
            _logger.Trace($"Start");

            if (!_initialized)
            {
                _logger.Error("HTTP listener not initalized, make sure you run as administrator or root.");
                return;
            }

            if (_server.IsListening)
            {
                _logger.Debug($"Already listening, failed to start...");
                return;
            }

            try
            {
                _logger.Info($"Starting...");
                _server.Start();
            }
            catch (HttpListenerException ex)
            {
                if (ex.ErrorCode == 5)
                {
                    _logger.Warn("You need to run the following command in order to not have to run as Administrator or root every start:");
                    _logger.Warn($"netsh http add urlacl url={_endpoint} user={Environment.UserDomainName}\\{Environment.UserName} listen=yes");
                }
                else
                {
                    throw;
                }
            }

            if (_server.IsListening)
            {
                _logger.Debug($"Listening on port {Port}...");
            }

            _logger.Info($"Starting HttpServer request handler...");
            var requestThread = new Thread(RequestHandler) { IsBackground = true };
            requestThread.Start();
        }

        /// <summary>
        /// Attempts to stop the HTTP listener server
        /// </summary>
        public void Stop()
        {
            _logger.Trace($"Stop");

            if (!_server.IsListening)
            {
                _logger.Debug($"Not running, failed to stop...");
                return;
            }

            _logger.Info($"Stopping...");
            _server.Stop();
        }

        #endregion

        #region Data Parsing Methods

        private void RequestHandler()
        {
            while (_server.IsListening)
            {
                var context = _server.GetContext();
                var response = context.Response;

                if (context.Request?.InputStream == null)
                    continue;

                // Read from the POST data input stream of the request
                using (var sr = new StreamReader(context.Request.InputStream))
                {
                    try
                    {
                        // Read to the end of the stream as a string
                        var data = sr.ReadToEnd();
                        ParseData(data);
                    }
                    catch (HttpListenerException hle)
                    {
                        _logger.Error(hle);

                        //Disconnected, reconnect.
                        HandleDisconnect();
                    }
                }

                try
                {
                    // Convert the default response message to UTF8 encoded bytes
                    var buffer = Encoding.UTF8.GetBytes(Strings.DefaultResponseMessage);
                    response.ContentLength64 = buffer.Length;
                    if (response?.OutputStream != null)
                    {
                        // Write the response buffer to the output stream
                        response.OutputStream.Write(buffer, 0, buffer.Length);
                    }
                    // Close the response
                    context.Response.Close();
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                }

                Thread.Sleep(10);
            }
        }

        private void ParseData(string data)
        {
            if (string.IsNullOrEmpty(data))
                return;

            try
            {
                if (IsDebug)
                {
                    lock (_lock)
                    {
                        File.AppendAllText(Strings.DebugLogFileName, data + Environment.NewLine);
                    }
                }

                var messages = JsonConvert.DeserializeObject<List<WebhookMessage>>(data);
                // If we fail to deserialize webhook payload, skip
                if (messages == null)
                    return;

                for (var i = 0; i < messages.Count; i++)
                {
                    var message = messages[i];
                    switch (message.Type)
                    {
                        case PokemonData.WebHookHeader:
                            ParsePokemon(message.Message);
                            break;
                        case GymData.WebhookHeader:
                            ParseGym(message.Message);
                            break;
                        case GymDetailsData.WebhookHeader:
                             ParseGymDetails(message.Message);
                            break;
                        case RaidData.WebHookHeader:
                            ParseRaid(message.Message);
                            break;
                        case QuestData.WebHookHeader:
                            ParseQuest(message.Message);
                            break;
                        case PokestopData.WebhookHeader:
                        case PokestopData.WebhookHeaderInvasion:
                            ParsePokestop(message.Message);
                            break;
                        case WeatherData.WebhookHeader:
                            ParseWeather(message.Message);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                _logger.Debug(Convert.ToString(data));
            }
        }

        private void ParsePokemon(dynamic message)
        {
            try
            {
                PokemonData pokemon = JsonConvert.DeserializeObject<PokemonData>(Convert.ToString(message));
                if (pokemon == null)
                {
                    _logger.Error($"Failed to parse Pokemon webhook object: {message}");
                    return;
                }

                pokemon.SetDespawnTime();

                // Only process Pokemon with despawn timers that meet a specific minimum
                if (pokemon.SecondsLeft.TotalMinutes < _despawnTimerMinimumMinutes)
                    return;

                /*
                if (_processedPokemon.ContainsKey(pokemon.EncounterId))
                {
                    // Pokemon already sent (check if IV set)
                    return;
                }
                */

                OnPokemonReceived(pokemon);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                _logger.Debug(Convert.ToString(message));
            }
        }

        private void ParseRaid(dynamic message)
        {
            try
            {
                RaidData raid = JsonConvert.DeserializeObject<RaidData>(Convert.ToString(message));
                if (raid == null)
                {
                    _logger.Error($"Failed to parse Pokemon webhook object: {message}");
                    return;
                }

                if (SkipEggs && raid.PokemonId == 0)
                {
                    _logger.Debug($"Level {raid.Level} Egg, skipping...");
                    return;
                }

                raid.SetTimes();

                if (_processedRaids.ContainsKey(raid.GymId))
                {
                    /*
                    if ((_processedRaids[raid.GymId].PokemonId == 0 || _processedRaids[raid.GymId].PokemonId == raid.PokemonId) &&
                        _processedRaids[raid.GymId].Form == raid.Form &&
                        _processedRaids[raid.GymId].Level == raid.Level &&
                        _processedRaids[raid.GymId].Start == raid.Start &&
                        _processedRaids[raid.GymId].End == raid.End)
                    {
                        _logger.Debug($"PROCESSED RAID ALREADY: Id: {raid.GymId} Name: {raid.GymName} Pokemon: {raid.PokemonId} Form: {raid.Form} Start: {raid.StartTime} End: {raid.EndTime}");
                        // Processed raid already
                        return;
                    }
                    */
                }
                else
                {
                    _processedRaids.Add(raid.GymId, raid);
                }

                OnRaidReceived(raid);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.StackTrace);
                _logger.Debug(Convert.ToString(message));
            }
        }

        private void ParseQuest(dynamic message)
        {
            try
            {
                QuestData quest = JsonConvert.DeserializeObject<QuestData>(Convert.ToString(message));
                if (quest == null)
                {
                    _logger.Error($"Failed to parse Quest webhook object: {message}");
                    return;
                }

                if (_processedQuests.ContainsKey(quest.PokestopId))
                {
                    if (_processedQuests[quest.PokestopId].Type == quest.Type &&
                        _processedQuests[quest.PokestopId].Rewards == quest.Rewards &&
                        _processedQuests[quest.PokestopId].Conditions == quest.Conditions)
                    {
                        // Processed quest already
                        return;
                    }
                }
                else
                {
                    _processedQuests.Add(quest.PokestopId, quest);
                }

                OnQuestReceived(quest);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                _logger.Debug(Convert.ToString(message));
            }
        }

        private void ParsePokestop(dynamic message)
        {
            try
            {
                PokestopData pokestop = JsonConvert.DeserializeObject<PokestopData>(Convert.ToString(message));
                if (pokestop == null)
                {
                    _logger.Error($"Failed to parse pokestop webhook object: {message}");
                    return;
                }

                pokestop.SetTimes();

                if (_processedPokestops.ContainsKey(pokestop.PokestopId))
                {
                    var processedLureAlready = _processedPokestops[pokestop.PokestopId].LureType == pokestop.LureType && _processedPokestops[pokestop.PokestopId].LureExpire == pokestop.LureExpire;
                    var processedInvasionAlready = _processedPokestops[pokestop.PokestopId].GruntType == pokestop.GruntType && _processedPokestops[pokestop.PokestopId].IncidentExpire == pokestop.IncidentExpire;
                    if (processedLureAlready || processedInvasionAlready)
                    {
                        _logger.Debug($"PROCESSED LURE OR INVASION ALREADY: Id: {pokestop.PokestopId} Name: {pokestop.Name} Lure: {pokestop.LureType} Expires: {pokestop.LureExpireTime} Grunt: {pokestop.GruntType} Expires: {pokestop.InvasionExpireTime}");
                        // Processed pokestop lure or invasion already
                        return;
                    }
                }
                else
                {
                    _processedPokestops.Add(pokestop.PokestopId, pokestop);
                }

                OnPokestopReceived(pokestop);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                _logger.Debug(Convert.ToString(message));
            }
        }

        private void ParseGym(dynamic message)
        {
            try
            {
                GymData gym = JsonConvert.DeserializeObject<GymData>(Convert.ToString(message));
                if (gym == null)
                {
                    _logger.Error($"Failed to parse gym webhook object: {message}");
                    return;
                }

                OnGymReceived(gym);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                _logger.Debug(Convert.ToString(message));
            }
        }

        private void ParseGymDetails(dynamic message)
        {
            try
            {
                GymDetailsData gymDetails = JsonConvert.DeserializeObject<GymDetailsData>(Convert.ToString(message));
                if (gymDetails == null)
                {
                    _logger.Error($"Failed to parse gym details webhook object: {message}");
                    return;
                }

                OnGymDetailsReceived(gymDetails);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                _logger.Debug(Convert.ToString(message));
            }
        }

        private void ParseWeather(dynamic message)
        {
            try
            {
                WeatherData weather = JsonConvert.DeserializeObject<WeatherData>(Convert.ToString(message));
                if (weather == null)
                {
                    _logger.Error($"Failed to parse weather webhook object: {message}");
                    return;
                }

                if (_processedWeather.ContainsKey(weather.Id))
                {
                    if (_processedWeather[weather.Id].GameplayCondition == weather.GameplayCondition &&
                        _processedWeather[weather.Id].CloudLevel == weather.CloudLevel &&
                        _processedWeather[weather.Id].FogLevel == weather.FogLevel &&
                        _processedWeather[weather.Id].RainLevel == weather.RainLevel &&
                        _processedWeather[weather.Id].Severity == weather.Severity &&
                        _processedWeather[weather.Id].SnowLevel == weather.SnowLevel &&
                        _processedWeather[weather.Id].WindLevel == weather.WindLevel &&
                        _processedWeather[weather.Id].SpecialEffectLevel == weather.SpecialEffectLevel &&
                        _processedWeather[weather.Id].WarnWeather == weather.WarnWeather &&
                        _processedWeather[weather.Id].WindDirection == weather.WindDirection)
                    {
                        // Processed weather already
                        return;
                    }

                    _processedWeather[weather.Id] = weather;
                }
                else
                {
                    _processedWeather.Add(weather.Id, weather);
                }

                OnWeatherReceived(weather);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                _logger.Debug(Convert.ToString(message));
            }
        }

        #endregion

        #region Private Methods

        private void Initialize()
        {
            _logger.Trace("Initialize");
            try
            {
                _server = CreateListener();
                _endpoint = $"http://{Host}:{Port}/";
                if (!_server.Prefixes.Contains(_endpoint))
                {
                    _server.Prefixes.Add(_endpoint);
                }
                _initialized = true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }

        private HttpListener CreateListener()
        {
            return new HttpListener();
        }

        private void HandleDisconnect()
        {
            _logger.Warn("!!!!! HTTP listener disconnected, handling reconnect...");
            _logger.Warn("Stopping existing listeners...");
            Stop();

            _logger.Warn("Disposing of old listener objects...");
            _server.Close();
            _server = null;

            _logger.Warn("Initializing...");
            Initialize();

            //if (_requestThread != null)
            //{
            //    _logger.Info($"Exiting HttpServer main thread...");
            //    _requestThread.Abort();
            //    _requestThread = null;
            //}

            _logger.Warn("Starting back up...");
            Start();

            _logger.Debug("Disconnect handled.");
        }

        #endregion

        private class WebhookMessage
        {
            [JsonProperty("type")]
            public string Type { get; set; }

            [JsonProperty("message")]
            public dynamic Message { get; set; }
        }
    }
}
