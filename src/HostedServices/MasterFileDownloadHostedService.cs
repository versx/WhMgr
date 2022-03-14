namespace WhMgr.HostedServices
{
    using Microsoft.Extensions.Hosting;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using WhMgr.Data;
    public class MasterFileDownloaderHostedService : IHostedService, IDisposable
    {
        private readonly Dictionary<string, MidnightTimer> _tzMidnightTimers;

        public MasterFileDownloaderHostedService()
        {
            _tzMidnightTimers = new Dictionary<string, MidnightTimer>();
        }
        void IDisposable.Dispose()
        {
            _tzMidnightTimers.Clear();

            GC.SuppressFinalize(this);
        }

        Task IHostedService.StartAsync(CancellationToken cancellationToken)
        {
            TimeZone localZone = TimeZone.CurrentTimeZone;
            var timezone = localZone.StandardName;

            var midnightTimer = new MidnightTimer(0, timezone);
            midnightTimer.TimeReached += OnMidnightTimerTimeReached;
            midnightTimer.Start();

            _tzMidnightTimers.Add(timezone, midnightTimer);
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

        Task IHostedService.StopAsync(CancellationToken cancellationToken)
        {
            foreach (var (timezone, midnightTimer) in _tzMidnightTimers)
            {
                midnightTimer.Stop();
                midnightTimer.Dispose();
            }
            return Task.CompletedTask;
        }
    }
}
