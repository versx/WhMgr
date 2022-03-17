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
            foreach (var (timezone, midnightTimer) in _tzMidnightTimers)
            {
                midnightTimer.Stop();
                midnightTimer.Dispose();
            }
            return Task.CompletedTask;
        }

        private void OnMidnightTimerTimeReached(DateTime time, string timezone)
        {
            using (WebClient wc = new WebClient())
            {
                var url = "https://raw.githubusercontent.com/WatWowMap/Masterfile-Generator/master/master-latest.json";
                var filePath = Path.Combine(Strings.DataFolder, "masterfile.json");
                wc.Proxy = null;
                wc.DownloadFile(new System.Uri(url), filePath);
            }
            GameMaster.ReloadMasterFile();
        }
    }
}
