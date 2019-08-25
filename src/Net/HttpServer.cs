namespace WhMgr.Net
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Net.NetworkInformation;
    using System.Net.Sockets;
    using System.Security.Principal;
    using System.Text;
    using System.Threading;

    using Newtonsoft.Json;

    using WhMgr.Diagnostics;
    using WhMgr.Net.Models;

    public class HttpServer
    {
        #region Variables

        private static readonly IEventLogger _logger = EventLogger.GetLogger();
        private static readonly object _lock = new object();
        private HttpListener _server;
        //private Thread _requestThread;

        #endregion

        #region Properties

        public ushort Port { get; }

        public bool IsDebug { get; set; }

        public bool SkipEggs { get; set; }

        #endregion

        #region Events

        public event EventHandler<PokemonDataEventArgs> PokemonReceived;

        private void OnPokemonReceived(PokemonData pokemon) => PokemonReceived?.Invoke(this, new PokemonDataEventArgs(pokemon));

        public event EventHandler<RaidDataEventArgs> RaidReceived;

        private void OnRaidReceived(RaidData raid) => RaidReceived?.Invoke(this, new RaidDataEventArgs(raid));

        public event EventHandler<QuestDataEventArgs> QuestReceived;

        private void OnQuestReceived(QuestData quest) => QuestReceived?.Invoke(this, new QuestDataEventArgs(quest));

        public event EventHandler<PokestopDataEventArgs> PokestopReceived;

        private void OnPokestopReceived(PokestopData pokestop) => PokestopReceived?.Invoke(this, new PokestopDataEventArgs(pokestop));

        public event EventHandler<GymDataEventArgs> GymReceived;

        private void OnGymReceived(GymData gym) => GymReceived?.Invoke(this, new GymDataEventArgs(gym));

        public event EventHandler<GymDetailsDataEventArgs> GymDetailsReceived;

        private void OnGymDetailsReceived(GymDetailsData gymDetails) => GymDetailsReceived?.Invoke(this, new GymDetailsDataEventArgs(gymDetails));

        #endregion

        #region Constructor

        public HttpServer(ushort port)
        {
            Port = port;

            Initialize();
        }

        #endregion

        #region Public Methods

        public void Start()
        {
            _logger.Trace($"HttpServer::Start");

            if (_server.IsListening)
            {
                _logger.Debug($"HttpServer is already listening, failed to start...");
                return;
            }

            _logger.Info($"HttpServer is starting...");
            _server.Start();

            if (_server.IsListening)
            {
                _logger.Debug($"Listening on port {Port}...");
            }

            _logger.Info($"Starting HttpServer request handler...");
            //if (_requestThread == null)
            //{
                var requestThread = new Thread(RequestHandler) { IsBackground = true };
            //}
            //if (_requestThread.ThreadState != ThreadState.Running)
            //{
                requestThread.Start();
            //}
        }

        public void Stop()
        {
            _logger.Trace($"HttpServer::Stop");

            if (!_server.IsListening)
            {
                _logger.Debug($"HttpServer is not running, failed to stop...");
                return;
            }

            _logger.Info($"HttpServer is stopping...");
            _server.Stop();

            //if (_requestThread != null)
            //{
            //    _logger.Info($"Existing HttpServer main thread...");
            //    _requestThread.Abort();
            //    _requestThread = null;
            //}
        }

        #endregion

        #region Data Parsing Methods

        private void RequestHandler()
        {
            while (_server.IsListening)
            {
                var context = _server.GetContext();
                var response = context.Response;

                using (var sr = new StreamReader(context.Request.InputStream))
                {
                    try
                    {
                        //if (sr.Peek() > -1)
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
                    var buffer = Encoding.UTF8.GetBytes(Strings.DefaultResponseMessage);
                    response.ContentLength64 = buffer.Length;
                    response.OutputStream.Write(buffer, 0, buffer.Length);
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
                if (messages == null)
                    return;

                foreach (var message in messages)
                {
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
                    _logger.Error($"Failed to parse pokestop webhook object: {message}");
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
                    _logger.Error($"Failed to parse pokestop webhook object: {message}");
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

        #endregion

        #region Private Methods

        private void Initialize()
        {
            try
            {
                _server = CreateListener();

                var addresses = GetLocalIPv4Addresses(NetworkInterfaceType.Wireless80211);
                if (addresses.Count == 0)
                {
                    addresses = GetLocalIPv4Addresses(NetworkInterfaceType.Ethernet);
                }

                if (IsAdministrator())
                {
                    for (var i = 0; i < addresses.Count; i++)
                    {
                        var endpoint = PrepareEndPoint(addresses[i], Port);
                        if (!_server.Prefixes.Contains(endpoint))
                            _server.Prefixes.Add(endpoint);

                        _logger.Debug($"[IP ADDRESS] {endpoint}");
                    }
                }

                for (var i = 0; i < Strings.LocalEndPoint.Length; i++)
                {
                    var endpoint = PrepareEndPoint(Strings.LocalEndPoint[i], Port);
                    if (!_server.Prefixes.Contains(endpoint))
                        _server.Prefixes.Add(endpoint);

                    _logger.Debug($"[IP ADDRESS] {endpoint}");
                }
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
            //    _logger.Info($"Existing HttpServer main thread...");
            //    _requestThread.Abort();
            //    _requestThread = null;
            //}

            _logger.Warn("Starting back up...");
            Start();

            _logger.Debug("Disconnect handled.");
        }

        #endregion

        #region Static Methods

        private static List<string> GetLocalIPv4Addresses(NetworkInterfaceType type)
        {
            var list = new List<string>();
            var interfaces = NetworkInterface.GetAllNetworkInterfaces();

            for (int i = 0; i < interfaces.Length; i++)
            {
                var netInterface = interfaces[i];
                var isUp = netInterface.NetworkInterfaceType == type &&
                           netInterface.OperationalStatus == OperationalStatus.Up;
                if (!isUp) continue;

                foreach (var ipAddress in netInterface.GetIPProperties().UnicastAddresses)
                {
                    var isIpv4 = ipAddress.Address.AddressFamily == AddressFamily.InterNetwork;
                    if (isIpv4)
                    {
                        list.Add(ipAddress.Address.ToString());
                        break;
                    }
                }
            }
            return list;
        }

        private static string PrepareEndPoint(string ip, ushort port)
        {
            return $"http://{ip}:{port}/";
        }

        private static bool IsAdministrator()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
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