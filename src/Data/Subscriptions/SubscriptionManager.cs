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
        private static readonly IEventLogger _logger = EventLogger.GetLogger();
        private readonly WhConfig _whConfig;
        private List<SubscriptionObject> _subscriptions;

        public IReadOnlyList<SubscriptionObject> Subscriptions => _subscriptions;
        private System.Data.IDbConnection _conn;
        //private readonly System.Data.IDbConnection _scanConn;

        public SubscriptionManager(WhConfig whConfig)
        {
            _logger.Trace($"SubscriptionManager::SubscriptionManager");

            _whConfig = whConfig;
            _conn = DataAccessLayer.CreateFactory().Open();
            if (_conn == null)
                throw new Exception("Failed to connect to the database.");

            CreateDefaultTables();
            ReloadSubscriptions();
        }

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

        public bool Set(ulong userId, bool enabled)
        {
            _logger.Trace($"SubscriptionManager::Set [UserId={userId}, Enabled={enabled}]");

            if (!IsDbConnectionOpen())
            {
                throw new Exception("Not connected to database.");
            }

            var subscription = GetUserSubscriptions(userId);
            subscription.Enabled = enabled;
            _conn.Save(subscription, true);

            return subscription.Enabled == enabled;
        }

        public bool SetDistance(ulong userId, int distance, double latitude, double longitude)
        {
            _logger.Trace($"SubscriptionManager::SetDistance [UserId={userId}, Distance={distance}, Latitude={latitude}, Longitude={longitude}]");

            if (!IsDbConnectionOpen())
            {
                throw new Exception("Not connected to database.");
            }

            var subscription = GetUserSubscriptions(userId);
            subscription.DistanceM = distance;
            subscription.Latitude = latitude;
            subscription.Longitude = longitude;
            _conn.Save(subscription, true);

            return subscription.DistanceM == distance &&
                Math.Abs(subscription.Latitude - latitude) < double.Epsilon &&
                Math.Abs(subscription.Longitude - longitude) < double.Epsilon;
        }

        #region User

        public bool UserExists(ulong userId)
        {
            if (!IsDbConnectionOpen())
            {
                throw new Exception("Not connected to database.");
            }

            try
            {
                var conn = GetConnection();
                return conn.LoadSingleById<SubscriptionObject>(userId) != null;
            }
            catch (MySql.Data.MySqlClient.MySqlException)
            {
                _conn = DataAccessLayer.CreateFactory().Open();
                return UserExists(userId);
            }
        }

        public SubscriptionObject GetUserSubscriptions(ulong userId)
        {
            if (!IsDbConnectionOpen())
            {
                throw new Exception("Not connected to database.");
            }

            try
            {
                var conn = GetConnection();
                var sub = conn.LoadSingleById<SubscriptionObject>(userId);
                return sub ?? new SubscriptionObject { UserId = userId };
            }
            catch (MySql.Data.MySqlClient.MySqlException)
            {
                _conn = DataAccessLayer.CreateFactory().Open();
                return GetUserSubscriptions(userId);
            }
        }

        public List<SubscriptionObject> GetUserSubscriptionsByPokemonId(int pokeId)
        {
            //var subscriptions = GetUserSubscriptions();
            //if (subscriptions != null)
            //{
            //    return subscriptions.Where(x => x.Pokemon.Exists(y => y.PokemonId == pokeId)).ToList();
            //}
            //List<SubscriptionObject> subs;
            //using (var db = DataAccessLayer.CreateFactory())
            //{
            //    subs = db.LoadSelect<SubscriptionObject>().Where(x => x.Enabled && x.Pokemon.Exists(y => y.PokemonId == pokeId)).ToList();
            //}

            //return subs;
            return _subscriptions?.Where(x => x.Enabled && x.Pokemon != null && x.Pokemon.Exists(y => y.PokemonId == pokeId))?.ToList();
        }

        public List<SubscriptionObject> GetUserSubscriptionsByRaidBossId(int pokeId)
        {
            //var subscriptions = GetUserSubscriptions();
            //if (subscriptions != null)
            //{
            //    return subscriptions.Where(x => x.Raids.Exists(y => y.PokemonId == pokeId)).ToList();
            //}

            //return null;
            return _subscriptions?.Where(x => x.Enabled && x.Raids != null && x.Raids.Exists(y => y.PokemonId == pokeId))?.ToList();
        }

        public List<SubscriptionObject> GetUserSubscriptionsByGruntType(InvasionGruntType gruntType)
        {
            return _subscriptions?.Where(x => x.Enabled && x.Invasions != null && x.Invasions.Exists(y => y.GruntType == gruntType))?.ToList();
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
                return conn.LoadSelect<SubscriptionObject>();//.Where(x => x.Enabled).ToList();
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
            _subscriptions = GetUserSubscriptions();
        }

        #endregion

        public List<Pokestop> GetQuests()
        {
            try
            {
                List<Pokestop> pokestops;
                using (var db = DataAccessLayer.CreateFactory(_whConfig.ScannerConnectionString).Open())
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

        public bool AddPokemon(ulong userId, int pokemonId, int iv = 0, int lvl = 0, string gender = "*", int attack = 0, int defense = 0, int stamina = 0)
        {
            _logger.Trace($"SubscriptionManager::AddPokemon [UserId={userId}, PokemonId={pokemonId}, IV={iv}, Level={lvl}, Gender={gender}, Attack={attack}, Defense={defense}, Stamina={stamina}]");

            if (!IsDbConnectionOpen())
            {
                throw new Exception("Not connected to database.");
            }

            var subscription = GetUserSubscriptions(userId);
            var pkmnSub = subscription.Pokemon.FirstOrDefault(x => x.PokemonId == pokemonId);
            if (pkmnSub == null)
            {
                //Create new pkmn subscription object.
                pkmnSub = new PokemonSubscription
                {
                    PokemonId = pokemonId,
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
                if (iv != pkmnSub.MinimumIV ||
                    lvl != pkmnSub.MinimumLevel ||
                    gender != pkmnSub.Gender ||
                    attack != pkmnSub.Attack ||
                    defense != pkmnSub.Defense ||
                    stamina != pkmnSub.Stamina)
                {
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
            catch (MySql.Data.MySqlClient.MySqlException)
            {
                return AddPokemon(userId, pokemonId, iv, lvl, gender);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }

            return false;
        }

        public bool AddRaid(ulong userId, int pokemonId, List<string> cities)
        {
            _logger.Trace($"SubscriptionManager::AddRaid [UserId={userId}, PokemonId={pokemonId}, Cities={cities}]");

            using (var conn = DataAccessLayer.CreateFactory().Open())
            {
                var subscription = GetUserSubscriptions(userId);
                for (var i = 0; i < cities.Count; i++)
                {
                    var city = cities[i];
                    var raidSub = subscription.Raids.FirstOrDefault(x => x.PokemonId == pokemonId && string.Compare(x.City, city, true) == 0);
                    if (raidSub != null)
                        //return true;
                        continue;

                    //Create new raid subscription object.
                    raidSub = new RaidSubscription
                    {
                        PokemonId = pokemonId,
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
                    //return true;
                }
                catch (MySql.Data.MySqlClient.MySqlException)
                {
                    return AddRaid(userId, pokemonId, cities);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                }
            }

            //return false;
            return true;
        }

        public bool AddQuest(ulong userId, string rewardKeyword, List<string> cities)
        {
            _logger.Trace($"SubscriptionManager::AddQuest [UserId={userId}, RewardKeyword={rewardKeyword}, Cities={cities}]");

            using (var conn = DataAccessLayer.CreateFactory().Open())
            {
                var subscription = GetUserSubscriptions(userId);
                for (var i = 0; i < cities.Count; i++)
                {
                    var city = cities[i];
                    var questSub = subscription.Quests.FirstOrDefault(x => rewardKeyword.ToLower().Contains(x.RewardKeyword.ToLower()) && string.Compare(x.City, city, true) == 0);
                    if (questSub != null)
                        //return true;
                        continue;

                    //Create new raid subscription object.
                    questSub = new QuestSubscription
                    {
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
                    return AddQuest(userId, rewardKeyword, cities);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                }
            }

            return true;
        }

        public bool AddInvasion(ulong userId, InvasionGruntType gruntType, List<string> cities)
        {
            _logger.Trace($"SubscriptionManager::AddInvasion [UserId={userId}, GruntType={gruntType}, Cities={cities}]");

            using (var conn = DataAccessLayer.CreateFactory().Open())
            {
                var subscription = GetUserSubscriptions(userId);
                for (var i = 0; i < cities.Count; i++)
                {
                    var city = cities[i];
                    var invasionSub = subscription.Invasions.FirstOrDefault(x => gruntType == x.GruntType && string.Compare(x.City, city, true) == 0);
                    if (invasionSub != null)
                        //return true;
                        continue;

                    //Create new Team Rocket invasion subscription object.
                    invasionSub = new InvasionSubscription
                    {
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
                    return AddInvasion(userId, gruntType, cities);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                }
            }

            return true;
        }

        //public bool AddSnoozedQuest(ulong userId, SnoozedQuest quest)
        //{
        //    _logger.Trace($"SubscriptionManager::AddSnoozedQuest [UserId={userId}, SnoozedQuest={quest.PokestopName}]");

        //    if (!IsDbConnectionOpen())
        //    {
        //        throw new Exception("Not connected to database.");
        //    }

        //    var subscription = GetUserSubscriptions(userId);
        //    var questSub = subscription.SnoozedQuests.FirstOrDefault(x =>
        //        string.Compare(quest.PokestopName, x.PokestopName, true) == 0 &&
        //        string.Compare(quest.Quest, x.Quest, true) == 0 &&
        //        string.Compare(quest.Reward, x.Reward, true) == 0 &&
        //        string.Compare(quest.City, x.City, true) == 0);

        //    //Already added.
        //    if (questSub != null)
        //        return true;

        //    subscription.SnoozedQuests.Add(quest);

        //    try
        //    {
        //        var conn = GetConnection();
        //        if (conn.Save(subscription, true))
        //        {
        //            _logger.Debug($"Snoozed Quest Added!");
        //        }
        //        else
        //        {
        //            _logger.Debug("Snoozed Quest Updated!");
        //        }

        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.Error(ex);
        //    }

        //    return false;
        //}

        public bool AddGym(ulong userId, string gymName)
        {
            _logger.Trace($"SubscriptionManager::AddGym [UserId={userId}, GymName={gymName}]");

            using (var conn = DataAccessLayer.CreateFactory().Open())
            {
                var subscription = GetUserSubscriptions(userId);

                //Gym exists
                var gymSub = subscription.Gyms.FirstOrDefault(x => string.Compare(x.Name, gymName, true) == 0);
                if (gymSub == null)
                {
                    //Create new gym subscription object.
                    gymSub = new GymSubscription
                    {
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
                    return AddGym(userId, gymName);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                    return false;
                }
            }
        }

        public bool AddRaid(ulong userId, List<int> pokemonIds, List<string> cities)
        {
            _logger.Trace($"SubscriptionManager::AddRaid [UserId={userId}, PokemonIds={string.Join(", ", pokemonIds)}, Cities={cities}]");

            if (!IsDbConnectionOpen())
            {
                throw new Exception("Not connected to database.");
            }

            var errors = 0;
            for (var i = 0; i < pokemonIds.Count; i++)
            {
                if (!AddRaid(userId, pokemonIds[i], cities))
                {
                    errors++;
                }
            }

            return errors == 0;
        }

        #endregion

        #region Remove

        public bool RemovePokemon(ulong userId, List<int> pokemonIds)
        {
            _logger.Trace($"SubscriptionManager::RemovePokemon [UserId={userId}, PokemonIds={pokemonIds}]");

            using (var conn = DataAccessLayer.CreateFactory().Open())
            {
                for (var i = 0; i < pokemonIds.Count; i++)
                {
                    var pokemonId = pokemonIds[i];
                    if (conn.Delete<PokemonSubscription>(x => x.UserId == userId && x.PokemonId == pokemonId) == 0)
                    {
                        _logger.Warn($"Could not delete Pokemon {pokemonId} from {userId} subscription.");
                    }
                }
            }

            return true;
        }

        public bool RemoveRaid(ulong userId, List<int> pokemonIds, List<string> cities)
        {
            _logger.Trace($"SubscriptionManager::RemoveRaid [UserId={userId}, PokemonId={pokemonIds}, Cities={cities}]");

            using (var conn = DataAccessLayer.CreateFactory().Open())
            {
                for (var i = 0; i < pokemonIds.Count; i++)
                {
                    if (conn.Delete<RaidSubscription>(x => x.UserId == userId && x.PokemonId == pokemonIds[i] && cities.Select(y => y.ToLower()).Contains(x.City.ToLower())) == 0)
                    {
                        _logger.Warn($"Could not delete raid subscription for user {userId} raid {pokemonIds[i]} city {cities}");
                    }
                }
            }

            return true;
        }

        public bool RemoveGym(ulong userId, string gymName)
        {
            _logger.Trace($"SubscriptionManager::RemoveGym [UserId={userId}, GymName={gymName}]");

            using (var conn = DataAccessLayer.CreateFactory().Open())
            {
                var result = conn.Delete<GymSubscription>(x => 
                    x.UserId == userId && 
                    string.Compare(gymName, x.Name, true) == 0);
                return result > 0;
            }
        }

        public bool RemoveQuest(ulong userId, string rewardKeyword, List<string> cities)
        {
            _logger.Trace($"SubscriptionManager::RemoveQuest [UserId={userId}, RewardKeyword={rewardKeyword}, Cities={cities}]");

            using (var conn = DataAccessLayer.CreateFactory().Open())
            {
                for (var i = 0; i < cities.Count; i++)
                {
                    if (conn.Delete<QuestSubscription>(x => 
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

        public bool RemoveInvasion(ulong userId, InvasionGruntType gruntType, List<string> cities)
        {
            _logger.Trace($"SubscriptionManager::RemoveInvasion [UserId={userId}, GruntType={gruntType}, Cities={cities}]");

            using (var conn = DataAccessLayer.CreateFactory().Open())
            {
                for (var i = 0; i < cities.Count; i++)
                {
                    if (conn.Delete<InvasionSubscription>(x =>
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

        public bool RemoveAllUserSubscriptions(ulong userId)
        {
            _logger.Trace($"SubscriptionManager::RemoveAllUserSubscription [UserId={userId}]");

            try
            {
                using (var conn = DataAccessLayer.CreateFactory().Open())
                {
                    conn.Delete<PokemonSubscription>(x => x.UserId == userId);
                    conn.Delete<RaidSubscription>(x => x.UserId == userId);
                    conn.Delete<QuestSubscription>(x => x.UserId == userId);
                    conn.Delete<GymSubscription>(x => x.UserId == userId);
                    conn.Delete<InvasionSubscription>(x => x.UserId == userId);
                    conn.Delete<SubscriptionObject>(x => x.UserId == userId);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }

            return false;
        }

        public bool RemoveAllPokemon(ulong userId)
        {
            _logger.Trace($"SubscriptionManager::RemoveAllPokemon [UserId={userId}]");

            using (var conn = DataAccessLayer.CreateFactory().Open())
            {
                var subscription = conn.LoadSingleById<SubscriptionObject>(userId);
                if (subscription == null)
                {
                    //Not subscribed.
                    return true;
                }

                var result = conn.DeleteAll(subscription.Pokemon);
                return result > 0;
            }
        }

        public bool RemoveAllRaids(ulong userId)
        {
            _logger.Trace($"SubscriptionManager::RemoveAllRaids [UserId={userId}]");

            using (var conn = DataAccessLayer.CreateFactory().Open())
            {
                var subscription = conn.LoadSingleById<SubscriptionObject>(userId);
                if (subscription == null)
                {
                    //Not subscribed.
                    return true;
                }

                var result = conn.DeleteAll(subscription.Raids);
                return result > 0;
            }
        }

        public bool RemoveAllQuests(ulong userId)
        {
            _logger.Info($"SubscriptionManager::RemoveAllQuests [UserId={userId}]");

            using (var conn = DataAccessLayer.CreateFactory().Open())
            {
                var subscription = conn.LoadSingleById<SubscriptionObject>(userId);
                if (subscription == null)
                {
                    //Not subscribed.
                    return true;
                }

                var result = conn.DeleteAll(subscription.Quests);
                return result > 0;
            }
        }

        public bool RemoveAllInvasions(ulong userId)
        {
            _logger.Info($"SubscriptionManager::RemoveAllInvasions [UserId={userId}]");

            using (var conn = DataAccessLayer.CreateFactory().Open())
            {
                var subscription = conn.LoadSingleById<SubscriptionObject>(userId);
                if (subscription == null)
                {
                    //Not subscribed.
                    return true;
                }

                var result = conn.DeleteAll(subscription.Invasions);
                return result > 0;
            }
        }

        public bool RemoveAllGyms(ulong userId)
        {
            _logger.Info($"SubscriptionManager::RemoveAllGyms [UserId={userId}]");

            using (var conn = DataAccessLayer.CreateFactory().Open())
            {
                var subscription = conn.LoadSingleById<SubscriptionObject>(userId);
                if (subscription == null)
                {
                    //Not subscribed.
                    return true;
                }

                var result = conn.DeleteAll(subscription.Gyms);
                return result > 0;
            }
        }

        //public bool RemoveAllSnoozedQuests()
        //{
        //    _logger.Info($"SubscriptionManager::RemoveAllSnoozedQuests");

        //    using (var conn = DataAccessLayer.CreateFactory().Open())
        //    {
        //        var result = conn.DeleteAll<SnoozedQuest>();
        //        return result > 0;
        //    }
        //}

        #endregion

        #region Add Statistics

        //public bool AddPokemonStatistic(ulong userId, PokemonData pokemon)
        //{
        //    var subscription = GetUserSubscriptions(userId);
        //    if (subscription == null)
        //        return false;

        //    subscription.PokemonStatistics.Add(new PokemonStatistics
        //    {
        //        UserId = userId,
        //        PokemonId = (uint)pokemon.Id,
        //        IV = pokemon.IV,
        //        CP = int.Parse(pokemon.CP ?? "0"),
        //        Date = DateTime.Now,
        //        Latitude = pokemon.Latitude,
        //        Longitude = pokemon.Longitude
        //    });

        //    if (!IsDbConnectionOpen())
        //    {
        //        _conn = DataAccessLayer.CreateFactory().Open();
        //    }

        //    return _conn.Save(subscription, true);
        //}

        //public bool AddRaidStatistic(ulong userId, RaidData raid)
        //{
        //    var subscription = GetUserSubscriptions(userId);
        //    if (subscription == null)
        //        return false;

        //    subscription.RaidStatistics.Add(new RaidStatistics
        //    {
        //        UserId = userId,
        //        PokemonId = (uint)raid.PokemonId,
        //        Date = DateTime.Now,
        //        Latitude = raid.Latitude,
        //        Longitude = raid.Longitude
        //    });

        //    if (!IsDbConnectionOpen())
        //    {
        //        _conn = DataAccessLayer.CreateFactory().Open();
        //    }

        //    return _conn.Save(subscription, true);
        //}

        //public bool AddQuestStatistic(ulong userId, QuestData quest)
        //{
        //    var subscription = GetUserSubscriptions(userId);
        //    if (subscription == null)
        //        return false;

        //    subscription.QuestStatistics.Add(new QuestStatistics
        //    {
        //        UserId = userId,
        //        Reward = quest.GetReward(),
        //        Date = DateTime.Now,
        //        Latitude = quest.Latitude,
        //        Longitude = quest.Longitude
        //    });

        //    if (!IsDbConnectionOpen())
        //    {
        //        _conn = DataAccessLayer.CreateFactory().Open();
        //    }

        //    return _conn.Save(subscription, true);
        //}

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
                conn.CreateTable<SubscriptionObject>();
                conn.CreateTable<PokemonSubscription>();
                conn.CreateTable<RaidSubscription>();
                conn.CreateTable<GymSubscription>();
                conn.CreateTable<QuestSubscription>();
                conn.CreateTable<InvasionSubscription>();
                //conn.CreateTable<SnoozedQuest>();
                //conn.CreateTable<PokemonStatistics>();
                //conn.CreateTable<RaidStatistics>();
                //conn.CreateTable<QuestStatistics>();

                _logger.Info($"Database tables created.");
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }

        private System.Data.IDbConnection GetConnection()
        {
            if (!IsDbConnectionOpen())
            {
                _conn = DataAccessLayer.CreateFactory().Open();
            }

            try
            {
                if (_conn == null)
                {
                    _conn = DataAccessLayer.CreateFactory().Open();
                }
                var results = _conn.Select<SubscriptionObject>();
                var list = results.ToList();
                return _conn;
            }
            catch (MySql.Data.MySqlClient.MySqlException)
            {
                try
                {
                    return _conn = DataAccessLayer.CreateFactory().Open();
                }
                catch (MySql.Data.MySqlClient.MySqlException)
                {
                    //TODO: Better solution
                    return _conn;
                }
                //return GetConnection();
            }

            //return _conn ?? DataAccessLayer.CreateFactory().Open();
        }

        private bool IsDbConnectionOpen()
        {
            if (_conn == null || _conn?.State != System.Data.ConnectionState.Open)
                _conn = DataAccessLayer.CreateFactory().Open();

            return _conn != null && _conn.State == System.Data.ConnectionState.Open;
        }

        #endregion
    }
}