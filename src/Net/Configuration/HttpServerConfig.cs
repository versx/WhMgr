namespace WhMgr.Net.Configuration
{
    public class HttpServerConfig
    {
        public string Host { get; set; }
        
        public ushort Port { get; set; }

        public int DespawnTimerMinimum { get; set; }

        public bool CheckForDuplicates { get; set; }

        public HttpServerConfig()
        {
            Host = "*";
            Port = 8008;
            DespawnTimerMinimum = 5;
            CheckForDuplicates = true;
        }
    }
}