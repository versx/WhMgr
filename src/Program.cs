using System;
using System.IO;

namespace T
{
    class Program
    {
        static void Main(string[] args)
        {
            new System.Threading.Thread(() => ReadFromFile()).Start();
            while(true){}

            var logger = Diagnostics.EventLogger.GetLogger();
            var whConfig = Configuration.WhConfig.Load("config.json");
            if (whConfig == null)
            {
                logger.Error("Failed to load config.");
                return;
            }

            var bot = new Bot(whConfig);
            bot.Start();

            System.Console.Read();
            while (true) {}
        }
    }
}