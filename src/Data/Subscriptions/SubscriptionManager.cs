namespace WhMgr.Data.Subscriptions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using ServiceStack.OrmLite;

    using WhMgr.Configuration;
    using WhMgr.Data.Subscriptions.Models;
    using WhMgr.Diagnostics;
    using WhMgr.Net.Models;

    public class SubscriptionManager
    {
        #region Variables

        private static readonly IEventLogger _logger = EventLogger.GetLogger("MANAGER");

        private readonly WhConfig _whConfig;
        private List<SubscriptionObject> _subscriptions;

        private readonly OrmLiteConnectionFactory _connFactory;
        //private readonly OrmLiteConnectionFactory _scanConnFactory;

        #endregion

        #region Properties

        public IReadOnlyList<SubscriptionObject> Subscriptions => _subscriptions;

        #endregion

        #region Constructor

        public SubscriptionManager(WhConfig whConfig)
        {
            _logger.Trace($"SubscriptionManager::SubscriptionManager");

            _whConfig = whConfig;

            if (_whConfig?.Database?.Main == null)
            {
                var err = "Main database is not configured in config.json file.";
                _logger.Error(err);
                throw new NullReferenceException(err);
            }

            if (_whConfig?.Database?.Scanner == null)
            {
                var err = "Scanner database is not configured in config.json file.";
                _logger.Error(err);
                throw new NullReferenceException(err);
            }

            _connFactory = new OrmLiteConnectionFactory(_whConfig.Database.Main.ToString(), MySqlDialect.Provider);
            //_scanConnFactory = new OrmLiteConnectionFactory(_whConfig.Database.Scanner.ToString(), MySqlDialect.Provider);

            CreateDefaultTables();
            ReloadSubscriptions();
        }

        #endregion

        #region User

        public SubscriptionObject GetUserSubscriptions(ulong guildId, ulong userId)
        {
            if (!IsDbConnectionOpen())
            {
                throw new Exception("Not connected to database.");
            }

            try
            {
                var conn = GetConnection();
                var expression = conn?.From<SubscriptionObject>();
                var where = expression?.Where(x => x.GuildId == guildId && x.UserId == userId);
                var query = conn?.LoadSelect(where);
                var sub = query?.FirstOrDefault();
                return sub ?? new SubscriptionObject { UserId = userId, GuildId = guildId };
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                _logger.Error(ex);
                return GetUserSubscriptions(guildId, userId);
            }
        }

        public List<SubscriptionObject> GetUserSubscriptionsByPokemonId(int pokeId)
        {
            return _subscriptions?.Where(x =>
                x.Enabled && x.Pokemon != null &&
                x.Pokemon.Exists(y => y.PokemonId == pokeId)
            )?.ToList();
        }

        public List<SubscriptionObject> GetUserSubscriptionsByPvPPokemonId(int pokeId)
        {
            return _subscriptions?.Where(x =>
                x.Enabled && x.Pokemon != null &&
                x.PvP.Exists(y => y.PokemonId == pokeId)
            )?.ToList();
        }

        public List<SubscriptionObject> GetUserSubscriptionsByRaidBossId(int pokeId)
        {
            return _subscriptions?.Where(x =>
                x.Enabled && x.Raids != null &&
                x.Raids.Exists(y => y.PokemonId == pokeId)
            )?.ToList();
        }

        public List<SubscriptionObject> GetUserSubscriptionsByGruntType(InvasionGruntType gruntType)
        {
            return _subscriptions?.Where(x =>
                x.Enabled && x.Invasions != null &&
                x.Invasions.Exists(y => y.GruntType == gruntType)
            )?.ToList();
        }

        public List<SubscriptionObject> GetUserSubscriptions()
        {
            try
            {
                if (!IsDbConnectionOpen())
                {
                    throw new Exception("Not connected to database.");
                }

                var conn = GetConnection();
                var expression = conn?.From<SubscriptionObject>();
                var where = expression?.Where(x => x.Enabled);
                var query = conn?.LoadSelect(where);
                var list = query?.ToList();
                return list;
            }
            catch (OutOfMemoryException mex)
            {
                _logger.Debug($"-------------------OUT OF MEMORY EXCEPTION!");
                _logger.Error(mex);
                Environment.FailFast($"Out of memory: {mex}");
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }

            return null;
        }

        public void ReloadSubscriptions()
        {
            var subs = GetUserSubscriptions();
            if (subs == null)
                return;

            _subscriptions = subs;
        }

        #endregion

        #region Remove

        public static bool RemoveAllUserSubscriptions(ulong guildId, ulong userId)
        {
            _logger.Trace($"SubscriptionManager::RemoveAllUserSubscription [GuildId={guildId}, UserId={userId}]");

            try
            {
                using (var conn = DataAccessLayer.CreateFactory().Open())
                {
                    conn.Delete<PokemonSubscription>(x => x.GuildId == guildId && x.UserId == userId);
                    conn.Delete<RaidSubscription>(x => x.GuildId == guildId && x.UserId == userId);
                    conn.Delete<QuestSubscription>(x => x.GuildId == guildId && x.UserId == userId);
                    conn.Delete<GymSubscription>(x => x.GuildId == guildId && x.UserId == userId);
                    conn.Delete<InvasionSubscription>(x => x.GuildId == guildId && x.UserId == userId);
                    conn.Delete<SubscriptionObject>(x => x.GuildId == guildId && x.UserId == userId);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }

            return false;
        }

        #endregion

        #region Private Methods

        private void CreateDefaultTables()
        {
            _logger.Trace($"SubscriptionManager::CreateDefaultTables");

            if (!IsDbConnectionOpen())
            {
                throw new Exception("Not connected to database.");
            }

            try
            {
                var conn = GetConnection();
                if (!conn.CreateTableIfNotExists<SubscriptionObject>())
                {
                    _logger.Info($"Table SubscriptionObject already exists.");
                }
                if (!conn.CreateTableIfNotExists<PokemonSubscription>())
                {
                    _logger.Info($"Table PokemonSubscription already exists.");
                }
                if (!conn.CreateTableIfNotExists<PvPSubscription>())
                {
                    _logger.Info($"Table PvPSubscription already exists.");
                }
                if (!conn.CreateTableIfNotExists<RaidSubscription>())
                {
                    _logger.Info($"Table RaidSubscription already exists.");
                }
                if (!conn.CreateTableIfNotExists<GymSubscription>())
                {
                    _logger.Info($"Table GymSubscription already exists.");
                }
                if (!conn.CreateTableIfNotExists<QuestSubscription>())
                {
                    _logger.Info($"Table QuestSubscription already exists.");
                }
                if (!conn.CreateTableIfNotExists<InvasionSubscription>())
                {
                    _logger.Info($"Table InvasionSubscription already exists.");
                }

                _logger.Info($"Database tables created.");
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }

        private System.Data.IDbConnection GetConnection()
        {
            return _connFactory.Open();
        }

        private bool IsDbConnectionOpen()
        {
            return _connFactory != null;
        }

        #endregion
    }
}