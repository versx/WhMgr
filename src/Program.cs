namespace WhMgr
{
    class Program
    {
        static void Main(string[] args)
        {
            var logger = Diagnostics.EventLogger.GetLogger();
            var whConfig = Configuration.WhConfig.Load(Strings.ConfigFileName);
            if (whConfig == null)
            {
                logger.Error("Failed to load config.");
                return;
            }

            var bot = new Bot(whConfig);
            bot.Start();

            System.Diagnostics.Process.GetCurrentProcess().WaitForExit();
        }
    }
}