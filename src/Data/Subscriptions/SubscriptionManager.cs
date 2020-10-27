namespace WhMgr.Data.Subscriptions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Timers;

    using Microsoft.EntityFrameworkCore;

    using WhMgr.Configuration;
    using WhMgr.Data.Factories;
    using WhMgr.Data.Subscriptions.Models;
    using WhMgr.Diagnostics;

    public class SubscriptionManager
    {
        #region Variables

        private static readonly IEventLogger _logger = EventLogger.GetLogger("MANAGER", Program.LogLevel);

        private readonly WhConfig _whConfig;
        private List<SubscriptionObject> _subscriptions;

        private readonly Timer _reloadTimer;

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

            //_connFactory = new OrmLiteConnectionFactory(_whConfig.Database.Main.ToString(), MySqlDialect.Provider);
            //_scanConnFactory = new OrmLiteConnectionFactory(_whConfig.Database.Scanner.ToString(), MySqlDialect.Provider);

            // Reload subscriptions every 60 seconds to account for UI changes
            _reloadTimer = new Timer(_whConfig.ReloadSubscriptionChangesMinutes * 1000);
            _reloadTimer.Elapsed += OnReloadTimerElapsed;
            _reloadTimer.Start();

            ReloadSubscriptions();
        }

        private void OnReloadTimerElapsed(object sender, ElapsedEventArgs e)
        {
            // TODO: Only reload based on last_changed timestamp in metadata table
            ReloadSubscriptions();
        }

        #endregion

        #region User

        public SubscriptionObject GetUserSubscriptions(ulong guildId, ulong userId)
        {
            /*
            if (!IsDbConnectionOpen())
            {
                throw new Exception("Not connected to database.");
            }
            */

            try
            {
                using (var ctx = DbContextFactory.CreateSubscriptionContext(_whConfig.Database.Main.ToString()))
                {
                    var sub = ctx.Subscriptions
                        .FirstOrDefault(x => x.GuildId == guildId && x.UserId == userId);
                    return sub ?? new SubscriptionObject { UserId = userId, GuildId = guildId };
                }
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
                x.Enabled && x.PvP != null &&
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

        public List<SubscriptionObject> GetUserSubscriptionsByQuestReward(string reward)
        {
            return _subscriptions?.Where(x =>
                x.Enabled && x.Quests != null &&
                x.Quests.Exists(y => reward.Contains(y.RewardKeyword))
            )?.ToList();
        }

        public List<SubscriptionObject> GetUserSubscriptionsByEncounterReward(List<int> encounterRewards)
        {
            return _subscriptions?.Where(x =>
                x.Enabled && x.Invasions != null &&
                x.Invasions.Exists(y => encounterRewards.Contains(y.RewardPokemonId))
            )?.ToList();
        }

        public List<SubscriptionObject> GetUserSubscriptions()
        {
            try
            {
                /*
                if (!IsDbConnectionOpen())
                {
                    throw new Exception("Not connected to database.");
                }
                */

                using (var ctx = DbContextFactory.CreateSubscriptionContext(_whConfig.Database.Main.ToString()))
                {
                    return ctx.Subscriptions
                        .Where(x => x.Enabled)
                        .ToList();
                }
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

        public static async Task<bool> RemoveAllUserSubscriptions(ulong guildId, ulong userId)
        {
            _logger.Trace($"SubscriptionManager::RemoveAllUserSubscription [GuildId={guildId}, UserId={userId}]");

            try
            {
                // Delete all user subscriptions for guild
                using (var db = DbContextFactory.CreateSubscriptionContext(DbContextFactory.ConnectionString))
                {
                    await db.Pokemon.Where(x => x.GuildId == guildId && x.UserId == userId).ForEachAsync(x => db.Pokemon.Remove(x));
                    await db.PvP.Where(x => x.GuildId == guildId && x.UserId == userId).ForEachAsync(x => db.PvP.Remove(x));
                    await db.Raids.Where(x => x.GuildId == guildId && x.UserId == userId).ForEachAsync(x => db.Raids.Remove(x));
                    await db.Quests.Where(x => x.GuildId == guildId && x.UserId == userId).ForEachAsync(x => db.Quests.Remove(x));
                    await db.Gyms.Where(x => x.GuildId == guildId && x.UserId == userId).ForEachAsync(x => db.Gyms.Remove(x));
                    await db.Invasions.Where(x => x.GuildId == guildId && x.UserId == userId).ForEachAsync(x => db.Invasions.Remove(x));
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
    }
}