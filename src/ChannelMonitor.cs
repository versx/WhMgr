namespace WhMgr
{
    using System;
    using System.Threading.Tasks;
    using System.Timers;

    using DSharpPlus;

    using WhMgr.Diagnostics;

    public class ChannelMonitorConfiguration
    {

    }

    public class ChannelMonitor
    {
        #region Constants

        public const int NotifyIntervalM = 15;
        public const int OneMinute = 1000 * 60;
        public const int MonitorIntervalM = 15;

        #endregion

        #region Variables

        private readonly DiscordClient _client;
        private readonly ChannelMonitorConfiguration _config;
        private readonly IEventLogger _logger;
        private readonly Timer _timer;

        #endregion

        #region Constructor

        public ChannelMonitor(DiscordClient client, ChannelMonitorConfiguration config, IEventLogger logger)
        {
            _client = client;
            _config = config;
            _logger = logger;

            _timer = new Timer { Interval = OneMinute * MonitorIntervalM };
            _timer.Elapsed += CheckFeedEventHandler;
        }

        #endregion

        #region Public Methods

        public void Start()
        {
            if (!_timer.Enabled)
            {
                _timer.Start();
            }
        }

        public void Stop()
        {
            if (_timer.Enabled)
            {
                _timer.Stop();
            }
        }

        #endregion

        #region Private Methods

        private async void CheckFeedEventHandler(object sender, ElapsedEventArgs e)
        {
            //if (!_config.Enabled) return;
            //if (_config.Channels.Count == 0) return;

            //for (int i = 0; i < _config.Channels.Count; i++)
            //{
            //    await CheckFeedChannelStatus(_config.Channels[i]);
            //}

            await Task.Delay(500);
        }

        private async Task CheckFeedChannelStatus(ulong channelId)
        {
            //var channel = await _client.GetChannelAsync(channelId);
            //if (channel == null)
            //{
            //    _logger.Error($"Failed to find Discord channel with id {channelId}.");
            //    return;
            //}

            //var mostRecent = await channel.GetMessage(channel.LastMessageId);
            //if (mostRecent == null)
            //{
            //    _logger.Error($"Failed to retrieve last message for channel {channel.Name}.");
            //    return;
            //}

            //if (IsFeedUp(mostRecent.Timestamp.DateTime, NotifyIntervalM))
            //    return;

            //var owner = await _client.GetUserAsync(_config.OwnerId);
            //if (owner == null)
            //{
            //    _logger.Error($"Failed to find owner with id {_config.OwnerId}.");
            //    return;
            //}

            //await _client.SendDirectMessage(owner, $"DISCORD FEED **{channel.Name}** IS DOWN!", null);
            await Task.Delay(200);
        }

        private bool IsFeedUp(DateTime created, int thresholdMinutes = MonitorIntervalM)
        {
            var diff = DateTime.Now.Subtract(created);
            var isUp = diff.Minutes < thresholdMinutes;
            return isUp;
        }

        #endregion
    }
}