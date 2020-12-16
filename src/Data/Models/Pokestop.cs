namespace WhMgr.Data.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using ServiceStack.DataAnnotations;
    using ServiceStack.OrmLite;
    using Newtonsoft.Json;
    using POGOProtos.Enums;
    using POGOProtos.Inventory.Item;
    using QuestRewardType = POGOProtos.Data.Quests.QuestReward.Types.Type;

    using WhMgr.Diagnostics;
    using WhMgr.Net.Models;

    [Alias("pokestop")]
    public class Pokestop
    {
        private static Dictionary<string, Pokestop> _pokestops;
        private static readonly IEventLogger _logger = EventLogger.GetLogger("POKESTOP", Program.LogLevel);

        #region Database Properties

        [Alias("id")]
        public string Id { get; set; }

        [Alias("name")]
        public string Name { get; set; }

        [Alias("url")]
        public string Url { get; set; }

        [Alias("lat")]
        public double Latitude { get; set; }

        [Alias("lon")]
        public double Longitude { get; set; }

        [Alias("lure_expire_timestamp")]
        public long LureExpireTimestamp { get; set; }

        [Alias("enabled")]
        public bool Enabled { get; set; }

        [Alias("quest_type")]
        public QuestType QuestType { get; set; }

        [Alias("quest_timestamp")]
        public long QuestTimestamp { get; set; }

        [Alias("quest_target")]
        public int QuestTarget { get; set; }

        [Alias("quest_conditions")]
        public string QuestConditionsJson { get; set; }

        private List<QuestConditionMessage> _questConditions;
        [Ignore]
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

        [Alias("quest_rewards")]
        public string QuestRewardsJson { get; set; }

        private List<QuestRewardMessage> _questRewards;
        [Ignore]
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

        [Alias("quest_template")]
        public string QuestTemplate { get; set; }

        [Alias("quest_pokemon_id")]
        public int QuestPokemonId { get; set; }

        [Alias("quest_reward_type")]
        public QuestRewardType QuestRewardType { get; set; }

        [Alias("quest_item_id")]
        public ItemId QuestItemId { get; set; }

        [Alias("cell_id")]
        public ulong CellId { get; set; }

        #endregion

        public static IReadOnlyDictionary<string, Pokestop> Pokestops
        {
            get
            {
                if (_pokestops == null)
                {
                    _pokestops = GetPokestops(DataAccessLayer.ScannerConnectionString);
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
                using (var db = DataAccessLayer.CreateFactory(connectionString).Open())
                {
                    var pokestops = db.LoadSelect<Pokestop>();
                    var dict = pokestops?.ToDictionary(x => x.Id, x => x);
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