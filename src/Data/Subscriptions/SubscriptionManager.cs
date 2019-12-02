namespace WhMgr.Data.Subscriptions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using ServiceStack.OrmLite;

    using WhMgr.Configuration;
    using WhMgr.Data.Subscriptions.Models;
    using Pokestop = WhMgr.Data.Models.Pokestop;
    using WhMgr.Diagnostics;
    using WhMgr.Extensions;
    using WhMgr.Net.Models;

    public class SubscriptionManager
    {
        #region Variables

        private static readonly IEventLogger _logger = EventLogger.GetLogger("MANAGER");

        private readonly WhConfig _whConfig;
        private List<SubscriptionObject> _subscriptions;

        private readonly OrmLiteConnectionFactory _connFactory;
        private readonly OrmLiteConnectionFactory _scanConnFactory;

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
            _scanConnFactory = new OrmLiteConnectionFactory(_whConfig.Database.Scanner.ToString(), MySqlDialect.Provider);

            CreateDefaultTables();
            ReloadSubscriptions();
        }

        #endregion

        /// <summary>
        /// Saves the subscription to the database.
        /// </summary>
        /// <param name="subscription">Subscription to save.</param>
        public void Save(SubscriptionObject subscription)
        {
            if (!IsDbConnectionOpen())
            {
                throw new Exception("Not connected to database.");
            }

            var conn = GetConnection();
            conn.Save(subscription, true);
        }

        public bool Set(ulong guildId, ulong userId, bool enabled)
        {
            _logger.Trace($"SubscriptionManager::Set [GuildId={guildId}, UserId={userId}, Enabled={enabled}]");

            if (!IsDbConnectionOpen())
            {
                throw new Exception("Not connected to database.");
            }

            var subscription = GetUserSubscriptions(guildId, userId);
            subscription.Enabled = enabled;
            Save(subscription);

            return subscription.Enabled == enabled;
        }

        public bool SetDistance(ulong guildId, ulong userId, int distance, double latitude, double longitude)
        {
            _logger.Trace($"SubscriptionManager::SetDistance [GuildId={guildId}, UserId={userId}, Distance={distance}, Latitude={latitude}, Longitude={longitude}]");

            if (!IsDbConnectionOpen())
            {
                throw new Exception("Not connected to database.");
            }

            var subscription = GetUserSubscriptions(guildId, userId);
            subscription.DistanceM = distance;
            subscription.Latitude = latitude;
            subscription.Longitude = longitude;
            Save(subscription);

            return subscription.DistanceM == distance &&
                Math.Abs(subscription.Latitude - latitude) < double.Epsilon &&
                Math.Abs(subscription.Longitude - longitude) < double.Epsilon;
        }

        public bool SetIconStyle(ulong guildId, ulong userId, string iconStyle)
        {
            _logger.Trace($"SubscriptionManager::Set [GuildId={guildId}, UserId={userId}, IconStyle={iconStyle}]");

            if (!IsDbConnectionOpen())
            {
                throw new Exception("Not connected to database.");
            }

            var subscription = GetUserSubscriptions(guildId, userId);
            subscription.IconStyle = iconStyle;
            Save(subscription);

            return subscription.IconStyle == iconStyle;
        }

        #region User

        public bool UserExists(ulong guildId, ulong userId)
        {
            if (!IsDbConnectionOpen())
            {
                throw new Exception("Not connected to database.");
            }

            try
            {
                var conn = GetConnection();
                return conn.Exists<SubscriptionObject>(x => x.GuildId == guildId && x.UserId == userId);
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                _logger.Error(ex);
                return UserExists(guildId, userId);
            }
        }

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

        public List<Pokestop> GetQuests()
        {
            try
            {
                List<Pokestop> pokestops;
                using (var db = _scanConnFactory.Open())
                {
                    pokestops = db.LoadSelect<Pokestop>();
                    pokestops = pokestops.Where(x => x.QuestTimestamp > 0).ToList();
                }
                return pokestops;
            }
            catch (OutOfMemoryException mex)
            {
                _logger.Debug($"-------------------OUT OF MEMORY EXCEPTION!");
                _logger.Error(mex);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }

            return null;
        }

        public List<Pokestop> GetQuests(List<string> rewards)
        {
            var quests = GetQuests();
            if (quests == null || quests.Count == 0)
                return null;

            var list = new List<Pokestop>();

            for (var i = 0; i < quests.Count; i++)
            {
                var quest = quests[i];
                var reward = quest.QuestRewardType.GetReward(quest.QuestPokemonId, quest.QuestRewards?[0]?.Info.Amount ?? 0, quest.QuestItemId, quest.QuestPokemonId == 132, quest.QuestRewards?[0]?.Info?.Shiny ?? false);
                var exists = rewards.Select(x => x.ToLower()).Where(x => reward.ToLower().Contains(x));
                list.Add(quest);
            }
            return list;
        }

        #region Add

        public bool AddPokemon(ulong guildId, ulong userId, int pokemonId, string form = null, int iv = 0, int lvl = 0, string gender = "*", int attack = 0, int defense = 0, int stamina = 0)
        {
            _logger.Trace($"SubscriptionManager::AddPokemon [GuildId={guildId}, UserId={userId}, PokemonId={pokemonId}, Form={form}, IV={iv}, Level={lvl}, Gender={gender}, Attack={attack}, Defense={defense}, Stamina={stamina}]");

            if (!IsDbConnectionOpen())
            {
                throw new Exception("Not connected to database.");
            }

            var subscription = GetUserSubscriptions(guildId, userId);
            var pkmnSub = subscription.Pokemon.FirstOrDefault(x => x.PokemonId == pokemonId && string.Compare(x.Form, form, true) == 0);
            if (pkmnSub == null)
            {
                //Create new pkmn subscription object.
                pkmnSub = new PokemonSubscription
                {
                    PokemonId = pokemonId,
                    Form = form,
                    GuildId = guildId,
                    UserId = userId,
                    MinimumIV = iv,
                    MinimumLevel = lvl,
                    Gender = gender,
                    Attack = attack,
                    Defense = defense,
                    Stamina = stamina
                };
                subscription.Pokemon.Add(pkmnSub);
            }
            else
            {
                //Pokemon subscription exists, check if values are the same.
                if (string.Compare(form, pkmnSub.Form, true) != 0 || 
                    iv != pkmnSub.MinimumIV ||
                    lvl != pkmnSub.MinimumLevel ||
                    gender != pkmnSub.Gender ||
                    attack != pkmnSub.Attack ||
                    defense != pkmnSub.Defense ||
                    stamina != pkmnSub.Stamina)
                {
                    pkmnSub.Form = form;
                    pkmnSub.MinimumIV = (pkmnSub.PokemonId == 201 ? 0 : iv);
                    pkmnSub.MinimumLevel = lvl;
                    pkmnSub.Gender = gender;
                    pkmnSub.Attack = attack;
                    pkmnSub.Defense = defense;
                    pkmnSub.Stamina = stamina;
                }
            }

            try
            {
                var conn = GetConnection();
                if (conn.Save(subscription, true))
                {
                    _logger.Debug($"Pokemon Added!");
                }
                else
                {
                    _logger.Debug("Pokemon Updated!");
                }

                _logger.Debug($"LastInsertId: {conn.LastInsertId()}");
                return true;
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                _logger.Error(ex);
                return AddPokemon(guildId, userId, pokemonId, form, iv, lvl, gender);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }

            return false;
        }

        public bool AddRaid(ulong guildId, ulong userId, int pokemonId, string form, List<string> cities)
        {
            _logger.Trace($"SubscriptionManager::AddRaid [GuildId={guildId}, UserId={userId}, PokemonId={pokemonId}, Form={form}, Cities={cities}]");

            using (var conn = DataAccessLayer.CreateFactory().Open())
            {
                var subscription = GetUserSubscriptions(guildId, userId);
                for (var i = 0; i < cities.Count; i++)
                {
                    var city = cities[i];
                    var raidSub = subscription.Raids.FirstOrDefault(x => x.PokemonId == pokemonId && 
                                                                         string.Compare(x.Form, form, true) == 0 &&
                                                                         string.Compare(x.City, city, true) == 0
                                                                     );
                    if (raidSub != null)
                        continue;

                    //Create new raid subscription object.
                    raidSub = new RaidSubscription
                    {
                        PokemonId = pokemonId,
                        Form = form,
                        GuildId = guildId,
                        UserId = userId,
                        City = city
                    };
                    subscription.Raids.Add(raidSub);
                }

                try
                {
                    if (conn.Save(subscription, true))
                    {
                        _logger.Debug($"Raid Added!");
                    }
                    else
                    {
                        _logger.Debug("Raid Updated!");
                    }
                }
                catch (MySql.Data.MySqlClient.MySqlException)
                {
                    return AddRaid(guildId, userId, pokemonId, form, cities);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                }
            }

            return true;
        }

        public bool AddQuest(ulong guildId, ulong userId, string rewardKeyword, List<string> cities)
        {
            _logger.Trace($"SubscriptionManager::AddQuest [GuildId={guildId}, UserId={userId}, RewardKeyword={rewardKeyword}, Cities={cities}]");

            using (var conn = DataAccessLayer.CreateFactory().Open())
            {
                var subscription = GetUserSubscriptions(guildId, userId);
                for (var i = 0; i < cities.Count; i++)
                {
                    var city = cities[i];
                    var questSub = subscription.Quests.FirstOrDefault(x => rewardKeyword.ToLower().Contains(x.RewardKeyword.ToLower()) && string.Compare(x.City, city, true) == 0);
                    if (questSub != null)
                        continue;

                    //Create new raid subscription object.
                    questSub = new QuestSubscription
                    {
                        GuildId = guildId,
                        UserId = userId,
                        RewardKeyword = rewardKeyword,
                        City = city
                    };
                    subscription.Quests.Add(questSub);
                }

                try
                {
                    if (conn.Save(subscription, true))
                    {
                        _logger.Debug($"Quest Added!");
                    }
                    else
                    {
                        _logger.Debug("Quest Updated!");
                    }
                }
                catch (MySql.Data.MySqlClient.MySqlException)
                {
                    return AddQuest(guildId, userId, rewardKeyword, cities);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                }
            }

            return true;
        }

        public bool AddInvasion(ulong guildId, ulong userId, InvasionGruntType gruntType, List<string> cities)
        {
            _logger.Trace($"SubscriptionManager::AddInvasion [GuildId={guildId}, UserId={userId}, GruntType={gruntType}, Cities={cities}]");

            using (var conn = DataAccessLayer.CreateFactory().Open())
            {
                var subscription = GetUserSubscriptions(guildId, userId);
                for (var i = 0; i < cities.Count; i++)
                {
                    var city = cities[i];
                    var invasionSub = subscription.Invasions.FirstOrDefault(x => gruntType == x.GruntType && string.Compare(x.City, city, true) == 0);
                    if (invasionSub != null)
                        continue;

                    //Create new Team Rocket invasion subscription object.
                    invasionSub = new InvasionSubscription
                    {
                        GuildId = guildId,
                        UserId = userId,
                        GruntType = gruntType,
                        City = city
                    };
                    subscription.Invasions.Add(invasionSub);
                }

                try
                {
                    if (conn.Save(subscription, true))
                    {
                        _logger.Debug($"Invasion Added!");
                    }
                    else
                    {
                        _logger.Debug("Invasion Updated!");
                    }
                }
                catch (MySql.Data.MySqlClient.MySqlException)
                {
                    return AddInvasion(guildId, userId, gruntType, cities);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                }
            }

            return true;
        }

        public bool AddGym(ulong guildId, ulong userId, string gymName)
        {
            _logger.Trace($"SubscriptionManager::AddGym [GuildId={guildId}, UserId={userId}, GymName={gymName}]");

            using (var conn = DataAccessLayer.CreateFactory().Open())
            {
                var subscription = GetUserSubscriptions(guildId, userId);

                //Gym exists
                var gymSub = subscription.Gyms.FirstOrDefault(x => string.Compare(x.Name, gymName, true) == 0);
                if (gymSub == null)
                {
                    //Create new gym subscription object.
                    gymSub = new GymSubscription
                    {
                        GuildId = guildId,
                        UserId = userId,
                        Name = gymName
                    };
                    subscription.Gyms.Add(gymSub);
                }
                else
                {
                    //Already exists.
                    return true;
                }

                try
                {
                    var result = conn.Save(subscription, true);
                    if (result)
                    {
                        _logger.Debug($"Gym Added!");
                    }
                    else
                    {
                        _logger.Debug("Gym Updated!");
                    }

                    return true;
                }
                catch (MySql.Data.MySqlClient.MySqlException)
                {
                    return AddGym(guildId, userId, gymName);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                    return false;
                }
            }
        }

        public bool AddRaid(ulong guildId, ulong userId, Dictionary<int, string> pokemonIds, List<string> cities)
        {
            _logger.Trace($"SubscriptionManager::AddRaid [Guild={guildId}, UserId={userId}, PokemonIds={string.Join(", ", pokemonIds)}, Cities={cities}]");

            if (!IsDbConnectionOpen())
            {
                throw new Exception("Not connected to database.");
            }

            var errors = 0;
            var keys = pokemonIds.Keys.ToList();
            for (var i = 0; i < keys.Count; i++)
            {
                var pokemonId = keys[i];
                var form = pokemonIds[pokemonId];
                if (!AddRaid(guildId, userId, pokemonId, form, cities))
                {
                    errors++;
                }
            }

            return errors == 0;
        }

        #endregion

        #region Remove

        public bool RemovePokemon(ulong guildId, ulong userId, Dictionary<int, string> pokemonIds)
        {
            _logger.Trace($"SubscriptionManager::RemovePokemon [GuildId={guildId}, UserId={userId}, PokemonIds={pokemonIds}]");

            using (var conn = DataAccessLayer.CreateFactory().Open())
            {
                var keys = pokemonIds.Keys.ToList();
                for (var i = 0; i < keys.Count; i++)
                {
                    var pokemonId = keys[i];
                    var form = pokemonIds[pokemonId];
                    var expression = conn?.From<PokemonSubscription>();
                    var where = expression?.Where(x => x.GuildId == guildId
                                                       && x.UserId == userId
                                                       && x.PokemonId == pokemonId
                                                       && (string.IsNullOrEmpty(form) && (x.Form == null || x.Form == string.Empty))
                                                       || string.Compare(x.Form, form, true) == 0);
                    var result = conn.Delete(where);
                    if (result == 0)
                    {
                        _logger.Warn($"Could not delete Pokemon {pokemonId}-{form} from {userId} subscription.");
                    }
                }
            }

            return true;
        }

        public bool RemoveRaid(ulong guildId, ulong userId, Dictionary<int, string> pokemonIds, List<string> cities)
        {
            _logger.Trace($"SubscriptionManager::RemoveRaid [GuildId={guildId}, UserId={userId}, PokemonId={pokemonIds}, Cities={cities}]");

            using (var conn = DataAccessLayer.CreateFactory().Open())
            {
                var keys = pokemonIds.Keys.ToList();
                for (var i = 0; i < keys.Count; i++)
                {
                    var pokemonId = keys[i];
                    var form = pokemonIds[pokemonId];
                    if (conn.Delete<RaidSubscription>(x => x.GuildId == guildId &&
                                                           x.UserId == userId &&
                                                           x.PokemonId == pokemonId &&
                                                           //string.Compare(x.Form, form, true) == 0 &&
                                                           x.Form == form &&
                                                           cities.Select(y => y.ToLower()).Contains(x.City.ToLower())
                                                       ) == 0)
                    {
                        _logger.Warn($"Could not delete raid subscription for user {userId} raid {pokemonId} city {cities}");
                    }
                }
            }

            return true;
        }

        public bool RemoveGym(ulong guildId, ulong userId, string gymName)
        {
            _logger.Trace($"SubscriptionManager::RemoveGym [GuildId={guildId}, UserId={userId}, GymName={gymName}]");

            using (var conn = DataAccessLayer.CreateFactory().Open())
            {
                var result = conn.Delete<GymSubscription>(x =>
                    x.GuildId == guildId &&
                    x.UserId == userId &&
                    string.Compare(gymName, x.Name, true) == 0);
                return result > 0;
            }
        }

        public bool RemoveQuest(ulong guildId, ulong userId, string rewardKeyword, List<string> cities)
        {
            _logger.Trace($"SubscriptionManager::RemoveQuest [GuildId={guildId}, UserId={userId}, RewardKeyword={rewardKeyword}, Cities={cities}]");

            using (var conn = DataAccessLayer.CreateFactory().Open())
            {
                for (var i = 0; i < cities.Count; i++)
                {
                    if (conn.Delete<QuestSubscription>(x => 
                        x.GuildId == guildId &&
                        x.UserId == userId && 
                        string.Compare(rewardKeyword.ToLower(), x.RewardKeyword.ToLower(), true) == 0 &&
                        string.Compare(cities[i], x.City, true) == 0) == 0)
                    {
                        _logger.Warn($"Could not delete quest subscription for user {userId} quest {rewardKeyword} city {cities[i]}");
                    }
                }
            }

            return true;
        }

        public bool RemoveInvasion(ulong guildId, ulong userId, InvasionGruntType gruntType, List<string> cities)
        {
            _logger.Trace($"SubscriptionManager::RemoveInvasion [GuildId={guildId}, UserId={userId}, GruntType={gruntType}, Cities={cities}]");

            using (var conn = DataAccessLayer.CreateFactory().Open())
            {
                for (var i = 0; i < cities.Count; i++)
                {
                    if (conn.Delete<InvasionSubscription>(x =>
                        x.GuildId == guildId &&
                        x.UserId == userId &&
                        x.GruntType == gruntType &&
                        string.Compare(cities[i], x.City, true) == 0) == 0)
                    {
                        _logger.Warn($"Could not delete invasion subscription for user {userId} invasion {gruntType} city {cities[i]}");
                    }
                }
            }

            return true;
        }

        #endregion

        #region Remove All

        public bool RemoveAllUserSubscriptions(ulong guildId, ulong userId)
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

        public bool RemoveAllPokemon(ulong guildId, ulong userId)
        {
            _logger.Trace($"SubscriptionManager::RemoveAllPokemon [GuildId={guildId}, UserId={userId}]");

            using (var conn = DataAccessLayer.CreateFactory().Open())
            {
                var subscription = GetUserSubscriptions(guildId, userId);
                if (subscription == null)
                {
                    //Not subscribed.
                    return true;
                }

                var result = conn.DeleteAll(subscription.Pokemon);
                return result > 0;
            }
        }

        public bool RemoveAllRaids(ulong guildId, ulong userId)
        {
            _logger.Trace($"SubscriptionManager::RemoveAllRaids [GuildId={guildId}, UserId={userId}]");

            using (var conn = DataAccessLayer.CreateFactory().Open())
            {
                var subscription = GetUserSubscriptions(guildId, userId);
                if (subscription == null)
                {
                    //Not subscribed.
                    return true;
                }

                var result = conn.DeleteAll(subscription.Raids);
                return result > 0;
            }
        }

        public bool RemoveAllQuests(ulong guildId, ulong userId)
        {
            _logger.Info($"SubscriptionManager::RemoveAllQuests [GuildId={guildId}, UserId={userId}]");

            using (var conn = DataAccessLayer.CreateFactory().Open())
            {
                var subscription = GetUserSubscriptions(guildId, userId);
                if (subscription == null)
                {
                    //Not subscribed.
                    return true;
                }

                var result = conn.DeleteAll(subscription.Quests);
                return result > 0;
            }
        }

        public bool RemoveAllInvasions(ulong guildId, ulong userId)
        {
            _logger.Info($"SubscriptionManager::RemoveAllInvasions [GuildId={guildId}, UserId={userId}]");

            using (var conn = DataAccessLayer.CreateFactory().Open())
            {
                var subscription = GetUserSubscriptions(guildId, userId);
                if (subscription == null)
                {
                    //Not subscribed.
                    return true;
                }

                var result = conn.DeleteAll(subscription.Invasions);
                return result > 0;
            }
        }

        public bool RemoveAllGyms(ulong guildId, ulong userId)
        {
            _logger.Info($"SubscriptionManager::RemoveAllGyms [GuildId={guildId}, UserId={userId}]");

            using (var conn = DataAccessLayer.CreateFactory().Open())
            {
                var subscription = GetUserSubscriptions(guildId, userId);
                if (subscription == null)
                {
                    //Not subscribed.
                    return true;
                }

                var result = conn.DeleteAll(subscription.Gyms);
                return result > 0;
            }
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
                //conn.CreateTable<SnoozedQuest>();

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
            return _connFactory != null;// && _conn.State == System.Data.ConnectionState.Open;
        }

        #endregion
    }
}