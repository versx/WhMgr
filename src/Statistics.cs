namespace WhMgr
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using ServiceStack.OrmLite;

    using WhMgr.Data;
    using WhMgr.Data.Models;

    public class Statistics
    {
        #region Singleton

        private static Statistics _instance;
        public static Statistics Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Statistics();
                }

                return _instance;
            }
        }

        #endregion

        #region Properties

        public long PokemonSent { get; set; }

        public long RaidsSent { get; set; }

        public long QuestsSent { get; set; }

        public long SubscriptionPokemonSent { get; set; }

        public long SubscriptionRaidsSent { get; set; }

        public long SubscriptionQuestsSent { get; set; }

        public List<PokemonStats> Top25Pokemon => GetPokemonStats(DateTime.Now)?.Take(25).ToList();

        public List<RaidStats> Top25Raids => GetRaidStats(DateTime.Now)?.Take(25).ToList();

        public List<QuestStats> Top25Quests => GetQuestStats(DateTime.Now)?.Take(25).ToList();

        #endregion

        #region Public Methods

        public void WriteOut()
        {
            if (!Directory.Exists(Strings.StatsFolder))
            {
                Directory.CreateDirectory(Strings.StatsFolder);
            }

            var stats = Statistics.Instance;
            var sb = new System.Text.StringBuilder();
            var header = "Pokemon Alarms,Raid Alarms,Quest Alarms,Pokemon Subscriptions,Raid Subscriptions,Quest Subscriptions,Top 25 Pokemon,Top 25 Raids";
            sb.AppendLine(header);

            sb.Append(stats.PokemonSent);
            sb.Append(",");
            sb.Append(stats.RaidsSent);
            sb.Append(",");
            sb.Append(stats.QuestsSent);
            sb.Append(",");
            sb.Append(stats.SubscriptionPokemonSent);
            sb.Append(",");
            sb.Append(stats.SubscriptionRaidsSent);
            sb.Append(",");
            sb.Append(stats.SubscriptionQuestsSent);
            sb.Append(",");
            sb.Append(string.Join(Environment.NewLine, stats.Top25Pokemon.Select(x => $"{Database.Instance.Pokemon[x.PokemonId].Name}: {x.Count.ToString("N0")}")));
            sb.Append(",");
            sb.Append(string.Join(Environment.NewLine, stats.Top25Raids.Select(x => $"{Database.Instance.Pokemon[x.PokemonId].Name}: {x.Count.ToString("N0")}")));

            File.WriteAllText(Path.Combine(Strings.StatsFolder, string.Format(Strings.StatsFileName, DateTime.Now.ToString("yyyy-MM-dd_hhmmss"))), sb.ToString());
        }

        public void Reset()
        {
            PokemonSent = 0;
            RaidsSent = 0;
            QuestsSent = 0;
            SubscriptionPokemonSent = 0;
            SubscriptionRaidsSent = 0;
            SubscriptionQuestsSent = 0;
        }

        #endregion

        #region Private Methods

        private static List<PokemonStats> GetPokemonStats(DateTime dateTime)
        {
            using (var db = DataAccessLayer.CreateFactory())
            {
                var stats = db.LoadSelect<PokemonStats>()
                    .Where(x =>
                        x.Date.Year == dateTime.Year &&
                        x.Date.Month == dateTime.Month &&
                        x.Date.Day == dateTime.Day)
                    .OrderByDescending(x => x.Count).ToList();
                return stats;
            }
        }

        private static List<RaidStats> GetRaidStats(DateTime dateTime)
        {
            using (var db = DataAccessLayer.CreateFactory())
            {
                var stats = db.LoadSelect<RaidStats>()
                    .Where(x =>
                        x.Date.Year == dateTime.Year &&
                        x.Date.Month == dateTime.Month &&
                        x.Date.Day == dateTime.Day)
                    .OrderByDescending(x => x.Count).ToList();
                return stats;
            }
        }

        private static List<QuestStats> GetQuestStats(DateTime dateTime)
        {
            using (var db = DataAccessLayer.CreateFactory())
            {
                var stats = db.LoadSelect<QuestStats>()
                    .Where(x =>
                        x.Date.Year == dateTime.Year &&
                        x.Date.Month == dateTime.Month &&
                        x.Date.Day == dateTime.Day)
                    .OrderByDescending(x => x.Count).ToList();
                return stats;
            }
        }

        #endregion
    }
}