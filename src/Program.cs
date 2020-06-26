namespace WhMgr
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    class Program
    {
        public static string ManagerName { get; set; } = "Main";

        static void Main(string[] args) => MainAsync(args).GetAwaiter().GetResult();

        static async Task MainAsync(string[] args)
        {
            var arguments = CommandLine.ParseArgs(new string[] { "--", "-" }, args);
            var configFilePath = string.Empty;
            var managerName = string.Empty;
            var keys = arguments.Keys.ToList();
            for (var i = 0; i < keys.Count; i++)
            {
                var key = keys[i];
                switch (key.ToLower())
                {
                    case "config":
                    case "c":
                        configFilePath = arguments.ContainsKey(key) ? arguments[key]?.ToString() : Strings.ConfigFileName;
                        break;
                    case "name":
                    case "n":
                        managerName = arguments.ContainsKey(key) ? arguments[key]?.ToString() : "Default";
                        break;
                }
            }

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

            Process.GetCurrentProcess().WaitForExit();
        }
    }
}