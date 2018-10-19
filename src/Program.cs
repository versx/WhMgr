namespace T
{
    using System;

    class Program
    {
        static void Main(string[] args)
        {
            var logger = Diagnostics.EventLogger.GetLogger();
            var whConfig = Configuration.WhConfig.Load("config.json");
            if (whConfig == null)
            {
                logger.Error("Failed to load config.");
                return;
            }

            var bot = new Bot(whConfig);
            bot.Start();

            Console.Read();
            while (true) {}
        }
    }
}