namespace WhMgr
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    class Program
    {
        public static string ManagerName { get; set; }

        static void Main(string[] args) => MainAsync(args).GetAwaiter().GetResult();

        static async Task MainAsync(string[] args)
        {
            var arguments = CommandLine.ParseArgs(new string[] { "--", "-" }, args);
            var alarmsFilePath = string.Empty;
            var configFilePath = string.Empty;
            var managerName = string.Empty;
            var keys = arguments.Keys.ToList();
            for (var i = 0; i < keys.Count; i++)
            {
                switch (keys[i])
                {
                    case "alarms":
                    case "a":
                        alarmsFilePath = arguments.ContainsKey(keys[i]) ? arguments[keys[i]]?.ToString() : Strings.AlarmsFileName;
                        break;
                    case "config":
                    case "c":
                        configFilePath = arguments.ContainsKey(keys[i]) ? arguments[keys[i]]?.ToString() : Strings.ConfigFileName;
                        break;
                    case "name":
                    case "n":
                        managerName = arguments.ContainsKey(keys[i]) ? arguments[keys[i]]?.ToString() : "Default";
                        break;
                }
            }

            alarmsFilePath = Path.Combine(Environment.CurrentDirectory, string.IsNullOrEmpty(alarmsFilePath) ? Strings.AlarmsFileName : alarmsFilePath);
            configFilePath = Path.Combine(Environment.CurrentDirectory, string.IsNullOrEmpty(configFilePath) ? Strings.ConfigFileName : configFilePath);
            ManagerName = managerName;
            var logger = Diagnostics.EventLogger.GetLogger();
            var whConfig = Configuration.WhConfig.Load(configFilePath);
            if (whConfig == null)
            {
                logger.Error($"Failed to load config {configFilePath}.");
                return;
            }
            whConfig.FileName = configFilePath;

            var bot = new Bot(whConfig);
            await bot.Start();

            System.Diagnostics.Process.GetCurrentProcess().WaitForExit();
        }
    }
}