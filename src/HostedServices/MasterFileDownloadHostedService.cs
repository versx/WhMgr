namespace WhMgr.HostedServices
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    using WhMgr.Data;
    using WhMgr.Extensions;
    using WhMgr.Utilities;

    public class MasterFileDownloaderHostedService : IHostedService, IDisposable
    {
        private readonly ILogger<MasterFileDownloaderHostedService> _logger;
        private readonly Dictionary<string, MidnightTimer> _tzMidnightTimers;

        public MasterFileDownloaderHostedService(
            ILogger<MasterFileDownloaderHostedService> logger)
        {
            _tzMidnightTimers = new Dictionary<string, MidnightTimer>();
            _logger = logger;
        }

        public void Dispose()
        {
            _tzMidnightTimers.Clear();

            GC.SuppressFinalize(this);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.Debug($"Starting masterfile.json downloader hosted service...");

            var localZone = TimeZoneInfo.Local;
            var timezone = localZone.StandardName;

            var midnightTimer = new MidnightTimer(0, timezone);
            midnightTimer.TimeReached += OnMidnightTimerTimeReached;
            midnightTimer.Start();

            _tzMidnightTimers.Add(timezone, midnightTimer);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.Debug($"Stopping masterfile.json downloader hosted service...");

            foreach (var (_, midnightTimer) in _tzMidnightTimers)
            {
                midnightTimer.Stop();
                midnightTimer.Dispose();
            }
            return Task.CompletedTask;
        }

        private void OnMidnightTimerTimeReached(object sender, TimeReachedEventArgs e)
        {
            _logger.Debug($"Downloading latest masterfile.json...");

            var data = NetUtils.Get(Strings.LatestGameMasterFileUrl);
            if (string.IsNullOrEmpty(data))
            {
                _logger.Error($"Latest masterfile.json downloaded but contents were empty...");
                return;
            }

            var filePath = Path.Combine(Strings.DataFolder, GameMaster.MasterFileName);
            File.WriteAllText(filePath, data);
            _logger.Information($"Latest masterfile.json downloaded to '{filePath}', reloading masterfile.json with latest version...");

            GameMaster.ReloadMasterFile();
        }
    }
}