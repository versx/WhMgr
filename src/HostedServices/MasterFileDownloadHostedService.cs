namespace WhMgr.HostedServices
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.Extensions.Hosting;

    using WhMgr.Data;

    public class MasterFileDownloaderHostedService : IHostedService, IDisposable
    {
        private readonly Dictionary<string, MidnightTimer> _tzMidnightTimers;

        public MasterFileDownloaderHostedService()
        {
            _tzMidnightTimers = new Dictionary<string, MidnightTimer>();
        }

        public void Dispose()
        {
            _tzMidnightTimers.Clear();

            GC.SuppressFinalize(this);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
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
            foreach (var (_, midnightTimer) in _tzMidnightTimers)
            {
                midnightTimer.Stop();
                midnightTimer.Dispose();
            }
            return Task.CompletedTask;
        }

        private void OnMidnightTimerTimeReached(DateTime time, string timezone)
        {
            using (var wc = new WebClient())
            {
                var filePath = Path.Combine(Strings.DataFolder, GameMaster.MasterFileName);
                wc.Proxy = null;
                wc.DownloadFile(new Uri(Strings.LatestGameMasterFileUrl), filePath);
            }
            GameMaster.ReloadMasterFile();
        }
    }
}
