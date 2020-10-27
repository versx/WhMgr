namespace WhMgr.Data.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    using Newtonsoft.Json;
    using WhMgr.Data.Factories;
    using WhMgr.Diagnostics;
    using WhMgr.Net.Models;

    [Table("pokestop")]
    public class Pokestop
    {
        private static Dictionary<string, Pokestop> _pokestops;
        private static readonly IEventLogger _logger = EventLogger.GetLogger("POKESTOP", Program.LogLevel);

        #region Database Properties

        [Column("id")]
        public string Id { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("url")]
        public string Url { get; set; }

        [Column("lat")]
        public double Latitude { get; set; }

        [Column("lon")]
        public double Longitude { get; set; }

        [Column("lure_expire_timestamp")]
        public long LureExpireTimestamp { get; set; }

        [Column("enabled")]
        public bool Enabled { get; set; }

        [Column("quest_type")]
        public QuestType QuestType { get; set; }

        [Column("quest_timestamp")]
        public long QuestTimestamp { get; set; }

        [Column("quest_target")]
        public int QuestTarget { get; set; }

        [Column("quest_conditions")]
        public string QuestConditionsJson { get; set; }

        private List<QuestConditionMessage> _questConditions;
        [NotMapped]
        public List<QuestConditionMessage> QuestConditions
        {
            get
            {
                if (_questConditions == null && !string.IsNullOrEmpty(QuestConditionsJson))
                {
                    _questConditions = JsonConvert.DeserializeObject<List<QuestConditionMessage>>(QuestConditionsJson);
                }

                return _questConditions;
            }
        }

        [Column("quest_rewards")]
        public string QuestRewardsJson { get; set; }

        private List<QuestRewardMessage> _questRewards;
        [NotMapped]
        public List<QuestRewardMessage> QuestRewards
        {
            get
            {
                if (_questRewards == null && !string.IsNullOrEmpty(QuestRewardsJson))
                {
                    _questRewards = JsonConvert.DeserializeObject<List<QuestRewardMessage>>(QuestRewardsJson);
                }

                return _questRewards;
            }
        }

        [Column("quest_template")]
        public string QuestTemplate { get; set; }

        [Column("quest_pokemon_id")]
        public int QuestPokemonId { get; set; }

        [Column("quest_reward_type")]
        public QuestRewardType QuestRewardType { get; set; }

        [Column("quest_item_id")]
        public ItemId QuestItemId { get; set; }

        [Column("cell_id")]
        public long CellId { get; set; }

        #endregion

        public static IReadOnlyDictionary<string, Pokestop> Pokestops
        {
            get
            {
                if (_pokestops == null)
                {
                    _pokestops = GetPokestops(DbContextFactory.ScannerConnectionString);
                }
                return _pokestops;
            }
        }

        private static Dictionary<string, Pokestop> GetPokestops(string connectionString = "")
        {
            if (string.IsNullOrEmpty(connectionString))
                return null;

            try
            {
                using (var db = DbContextFactory.CreateScannerDbContext(connectionString))
                {
                    var dict = db.Pokestops?.ToDictionary(x => x.Id, x => x);
                    return dict;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }

            return null;
        }
    }
}