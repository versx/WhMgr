namespace WhMgr.Data.Subscriptions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
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
        private readonly System.Timers.Timer _reloadTimer;

        #endregion

        #region Properties

        public List<SubscriptionObject> Subscriptions { get; private set; }

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

            // Reload subscriptions every 60 seconds to account for UI changes
            _reloadTimer = new System.Timers.Timer(_whConfig.ReloadSubscriptionChangesMinutes * 60 * 1000);
            _reloadTimer.Elapsed += OnReloadTimerElapsed;
            _reloadTimer.Start();

            ThreadPool.QueueUserWorkItem(x => ReloadSubscriptions());
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
            try
            {
                using (var ctx = DbContextFactory.CreateSubscriptionContext(_whConfig.Database.Main.ToString()))
                {
                    var sub = ctx.Subscriptions
                        //.Include(x => x.Pokemon)
                        //.Include(x => x.PvP)
                        //.Include(x => x.Raids)
                        //.Include(x => x.Quests)
                        //.Include(x => x.Gyms)
                        //.Include(x => x.Invasions)
                        .FirstOrDefault(x => x.GuildId == guildId && x.UserId == userId);
                    if (sub == null)
                    {
                        ctx.Add(new SubscriptionObject { UserId = userId, GuildId = guildId });
                        ctx.SaveChanges(false);
                        return GetUserSubscriptions(guildId, userId);
                    }
                    return sub;
                }
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                _logger.Error(ex);
                return GetUserSubscriptions(guildId, userId);
            }
        }

        public List<PokemonSubscription> GetUserPokemonSubscriptions(ulong guildId, ulong userId)
        {
            using (var ctx = DbContextFactory.CreateSubscriptionContext(_whConfig.Database.Main.ToString()))
            {
                return ctx.Pokemon.Where(x => x.GuildId == guildId && x.UserId == userId)?.ToList();
            }
        }

        public List<PvPSubscription> GetUserPvPSubscriptions(ulong guildId, ulong userId)
        {
            using (var ctx = DbContextFactory.CreateSubscriptionContext(_whConfig.Database.Main.ToString()))
            {
                return ctx.PvP.Where(x => x.GuildId == guildId && x.UserId == userId)?.ToList();
            }
        }

        public List<RaidSubscription> GetUserRaidSubscriptions(ulong guildId, ulong userId)
        {
            using (var ctx = DbContextFactory.CreateSubscriptionContext(_whConfig.Database.Main.ToString()))
            {
                return ctx.Raids.Where(x => x.GuildId == guildId && x.UserId == userId)?.ToList();
            }
        }

        public List<QuestSubscription> GetUserQuestSubscriptions(ulong guildId, ulong userId)
        {
            using (var ctx = DbContextFactory.CreateSubscriptionContext(_whConfig.Database.Main.ToString()))
            {
                return ctx.Quests.Where(x => x.GuildId == guildId && x.UserId == userId)?.ToList();
            }
        }

        public List<GymSubscription> GetUserGymSubscriptions(ulong guildId, ulong userId)
        {
            using (var ctx = DbContextFactory.CreateSubscriptionContext(_whConfig.Database.Main.ToString()))
            {
                return ctx.Gyms.Where(x => x.GuildId == guildId && x.UserId == userId)?.ToList();
            }
        }

        public List<InvasionSubscription> GetUserInvasionSubscriptions(ulong guildId, ulong userId)
        {
            using (var ctx = DbContextFactory.CreateSubscriptionContext(_whConfig.Database.Main.ToString()))
            {
                return ctx.Invasions.Where(x => x.GuildId == guildId && x.UserId == userId)?.ToList();
            }
        }

        public List<PokemonSubscription> GetUserSubscriptionsByPokemonId(int pokeId)
        {
            using (var ctx = DbContextFactory.CreateSubscriptionContext(_whConfig.Database.Main.ToString()))
            {
                return ctx.Pokemon.Where(x => x.PokemonId == pokeId).ToList();
            }
        }

        public List<PvPSubscription> GetUserSubscriptionsByPvPPokemonId(int pokeId)
        {
            using (var ctx = DbContextFactory.CreateSubscriptionContext(_whConfig.Database.Main.ToString()))
            {
                return ctx.PvP.Where(x => x.PokemonId == pokeId).ToList();
            }
        }

        public List<RaidSubscription> GetUserSubscriptionsByRaidBossId(int pokeId)
        {
            using (var ctx = DbContextFactory.CreateSubscriptionContext(_whConfig.Database.Main.ToString()))
            {
                return ctx.Raids.Where(x => x.PokemonId == pokeId).ToList();
            }
        }

        public List<QuestSubscription> GetUserSubscriptionsByQuestReward(string reward)
        {
            using (var ctx = DbContextFactory.CreateSubscriptionContext(_whConfig.Database.Main.ToString()))
            {
                return ctx.Quests.Where(x => reward.ToLower().Contains(x.RewardKeyword.ToLower())).ToList();
            }
        }

        public List<InvasionSubscription> GetUserSubscriptionsByEncounterReward(List<int> encounterRewards)
        {
            using (var ctx = DbContextFactory.CreateSubscriptionContext(_whConfig.Database.Main.ToString()))
            {
                return ctx.Invasions.Where(x => encounterRewards.Contains(x.RewardPokemonId)).ToList();
            }
        }

        public List<SubscriptionObject> GetUserSubscriptions()
        {
            try
            {
                using (var ctx = DbContextFactory.CreateSubscriptionContext(_whConfig.Database.Main.ToString()))
                {
                    Subscriptions = ctx.Subscriptions.ToList();
                    //.Include(sub => sub.Pokemon)
                    //.Include(sub => sub.PvP)
                    //.Include(sub => sub.Raids)
                    //.Include(sub => sub.Quests)
                    //.Include(sub => sub.Gyms)
                    //.Include(sub => sub.Invasions)
                    //.ToList();
                    return Subscriptions;
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
                        
            Subscriptions = subs;
        }

        public static bool SetUserSubscriptionsStatus(ulong guildId, ulong userId, bool enabled)
        {
            using (var db = DbContextFactory.CreateSubscriptionContext(DbContextFactory.ConnectionString))
            {
                var subscription = db.Subscriptions.FirstOrDefault(x => x.GuildId == guildId && x.UserId == userId);
                if (subscription == null)
                {
                    // Failed to get user subscription
                    return false;
                }                    
                subscription.Enabled = enabled;
                var result = db.SaveChanges();
                return result > 0;
            }
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
                    await db.Pokemon
                        .Where(x => x.GuildId == guildId && x.UserId == userId)
                        .ForEachAsync(x => db.Pokemon.Remove(x));
                    await db.PvP
                        .Where(x => x.GuildId == guildId && x.UserId == userId)
                        .ForEachAsync(x => db.PvP.Remove(x));
                    await db.Raids
                        .Where(x => x.GuildId == guildId && x.UserId == userId)
                        .ForEachAsync(x => db.Raids.Remove(x));
                    await db.Quests
                        .Where(x => x.GuildId == guildId && x.UserId == userId)
                        .ForEachAsync(x => db.Quests.Remove(x));
                    await db.Gyms
                        .Where(x => x.GuildId == guildId && x.UserId == userId)
                        .ForEachAsync(x => db.Gyms.Remove(x));
                    await db.Invasions
                        .Where(x => x.GuildId == guildId && x.UserId == userId)
                        .ForEachAsync(x => db.Invasions.Remove(x));
                    db.SaveChanges();
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
