namespace WhMgr.Data.Subscriptions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using ServiceStack.OrmLite;

    using WhMgr.Data.Subscriptions.Models;
    using WhMgr.Diagnostics;

    public class SubscriptionManager
    {
        private static readonly IEventLogger _logger = EventLogger.GetLogger();

        public SubscriptionManager()
        {
            _logger.Trace($"SubscriptionManager::SubscriptionManager");

            CreateDefaultTables();
        }

        /// <summary>
        /// Saves the subscription to the database.
        /// </summary>
        /// <param name="subscription">Subscription to save.</param>
        public void Save(SubscriptionObject subscription)
        {
            using (var db = DataAccessLayer.CreateFactory())
            {
                db.Save(subscription, true);
            }
        }

        public bool Set(ulong userId, bool enabled)
        {
            _logger.Trace($"SubscriptionManager::Set [UserId={userId}, Enabled={enabled}]");

            using (var db = DataAccessLayer.CreateFactory())
            {
                var subscription = GetUserSubscriptions(userId);
                subscription.Enabled = enabled;
                db.Save(subscription, true);

                return subscription.Enabled == enabled;
            }
        }

        public bool SetDistance(ulong userId, int distance, double latitude, double longitude)
        {
            _logger.Trace($"SubscriptionManager::SetDistance [UserId={userId}, Distance={distance}, Latitude={latitude}, Longitude={longitude}]");

            using (var db = DataAccessLayer.CreateFactory())
            {
                var subscription = GetUserSubscriptions(userId);
                subscription.DistanceM = distance;
                subscription.Latitude = latitude;
                subscription.Longitude = longitude;
                db.Save(subscription, true);

                return subscription.DistanceM == distance &&
                    Math.Abs(subscription.Latitude - latitude) < double.Epsilon &&
                    Math.Abs(subscription.Longitude - longitude) < double.Epsilon;
            }
        }

        public bool SetAlertTime(ulong userId, DateTime? alertTime)
        {
            _logger.Trace($"SubscriptionManager::SetAlertTime [UserId={userId}, AlertTime={alertTime}]");

            var value = alertTime.HasValue && alertTime.Value != DateTime.MinValue ? alertTime : null;
            using (var db = DataAccessLayer.CreateFactory())
            {
                var subscription = GetUserSubscriptions(userId);
                subscription.AlertTime = value;
                db.Save(subscription, true);

                return subscription.AlertTime == value;
            }
        }

        #region User

        public bool UserExists(ulong userId)
        {
            using (var db = DataAccessLayer.CreateFactory())
            {
                var subscription = db.LoadSingleById<SubscriptionObject>(userId);
                return subscription != null;
            }
        }

        public SubscriptionObject GetUserSubscriptions(ulong userId)
        {
            using (var db = DataAccessLayer.CreateFactory())
            {
                var subscription = db.LoadSingleById<SubscriptionObject>(userId);
                if (subscription != null)
                {
                    return subscription;
                }
            }

            return new SubscriptionObject { UserId = userId };
        }

        public List<SubscriptionObject> GetUserSubscriptionsByPokemonId(int pokeId)
        {
            var subscriptions = GetUserSubscriptions();
            if (subscriptions != null)
            {
                return subscriptions.Where(x => x.Pokemon.Exists(y => y.PokemonId == pokeId)).ToList();
            }

            return null;
        }

        public List<SubscriptionObject> GetUserSubscriptionsByRaidBossId(int pokeId)
        {
            var subscriptions = GetUserSubscriptions();
            if (subscriptions != null)
            {
                return subscriptions.Where(x => x.Raids.Exists(y => y.PokemonId == pokeId)).ToList();
            }

            return null;
        }

        public List<SubscriptionObject> GetUserSubscriptions()
        {
            try
            {
                using (var db = DataAccessLayer.CreateFactory())
                {
                    return db.LoadSelect<SubscriptionObject>();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }

            return null;
        }

        #endregion

        #region Add

        public bool AddPokemon(ulong userId, int pokemonId, int iv = 0, int lvl = 0, string gender = "*")
        {
            _logger.Trace($"SubscriptionManager::AddPokemon [UserId={userId}, PokemonId={pokemonId}, IV={iv}, Level={lvl}, Gender={gender}]");

            using (var db = DataAccessLayer.CreateFactory())
            {
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
                        Gender = gender
                    };
                    subscription.Pokemon.Add(pkmnSub);
                }
                else
                {
                    //Pokemon subscription exists, check if values are the same.
                    if (iv != pkmnSub.MinimumIV ||
                        lvl != pkmnSub.MinimumLevel ||
                        gender != pkmnSub.Gender)
                    {
                        pkmnSub.MinimumIV = (pkmnSub.PokemonId == 201 ? 0 : iv);
                        pkmnSub.MinimumLevel = lvl;
                        pkmnSub.Gender = gender;
                    }
                }

                try
                {
                    if (db.Save(subscription, true))
                    {
                        _logger.Debug($"Pokemon Added!");
                    }
                    else
                    {
                        _logger.Debug("Pokemon Updated!");
                    }

                    _logger.Debug($"LastInsertId: {db.LastInsertId()}");

                    return true;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                }

                return false;
            }
        }

        public bool AddRaid(ulong userId, int pokemonId, List<string> cities)
        {
            _logger.Trace($"SubscriptionManager::AddRaid [UserId={userId}, PokemonId={pokemonId}, Cities={cities}]");

            using (var db = DataAccessLayer.CreateFactory())
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

                    try
                    {
                        if (db.Save(subscription, true))
                        {
                            _logger.Debug($"Raid Added!");
                        }
                        else
                        {
                            _logger.Debug("Raid Updated!");
                        }
                        //return true;
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex);
                    }
                }

                //return false;
                return true;
            }
        }

        public bool AddQuest(ulong userId, string rewardKeyword, List<string> cities)
        {
            _logger.Trace($"SubscriptionManager::AddQuest [UserId={userId}, RewardKeyword={rewardKeyword}, Cities={cities}]");

            using (var db = DataAccessLayer.CreateFactory())
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

                    try
                    {
                        if (db.Save(subscription, true))
                        {
                            _logger.Debug($"Quest Added!");
                        }
                        else
                        {
                            _logger.Debug("Quest Updated!");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex);
                    }
                }

                return true;
            }
        }

        public bool AddSnoozedQuest(ulong userId, SnoozedQuest quest)
        {
            _logger.Trace($"SubscriptionManager::AddSnoozedQuest [UserId={userId}, SnoozedQuest={quest.PokestopName}]");

            using (var db = DataAccessLayer.CreateFactory())
            {
                var subscription = GetUserSubscriptions(userId);
                var questSub = subscription.SnoozedQuests.FirstOrDefault(x =>
                    string.Compare(quest.PokestopName, x.PokestopName, true) == 0 &&
                    string.Compare(quest.Quest, x.Quest, true) == 0 &&
                    string.Compare(quest.Reward, x.Reward, true) == 0 &&
                    string.Compare(quest.City, x.City, true) == 0);

                //Already added.
                if (questSub != null)
                    return true;

                subscription.SnoozedQuests.Add(quest);

                try
                {
                    if (db.Save(subscription, true))
                    {
                        _logger.Debug($"Snoozed Quest Added!");
                    }
                    else
                    {
                        _logger.Debug("Snoozed Quest Updated!");
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                }

                return false;
            }
        }

        public bool AddGym(ulong userId, string gymName)
        {
            _logger.Trace($"SubscriptionManager::AddGym [UserId={userId}, GymName={gymName}]");

            using (var db = DataAccessLayer.CreateFactory())
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
                    var result = db.Save(subscription, true);
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

            using (var db = DataAccessLayer.CreateFactory())
            {
                for (var i = 0; i < pokemonIds.Count; i++)
                {
                    var pokemonId = pokemonIds[i];
                    if (db.Delete<PokemonSubscription>(x => x.UserId == userId && x.PokemonId == pokemonId) == 0)
                    {
                        _logger.Warn($"Could not delete Pokemon {pokemonId} from {userId} subscription.");
                    }
                }

                return true;
            }
        }

        public bool RemoveRaid(ulong userId, List<int> pokemonIds, List<string> cities)
        {
            _logger.Trace($"SubscriptionManager::RemoveRaid [UserId={userId}, PokemonId={pokemonIds}, Cities={cities}]");

            using (var db = DataAccessLayer.CreateFactory())
            {
                for (var i = 0; i < pokemonIds.Count; i++)
                {
                    if (db.Delete<RaidSubscription>(x => x.UserId == userId && x.PokemonId == pokemonIds[i] && cities.Select(y => y.ToLower()).Contains(x.City.ToLower())) == 0)
                    {
                        _logger.Warn($"Could not delete raid subscription for user {userId} raid {pokemonIds[i]} city {cities}");
                    }
                }

                return true;
            }
        }

        public bool RemoveGym(ulong userId, string gymName)
        {
            _logger.Trace($"SubscriptionManager::RemoveGym [UserId={userId}, GymName={gymName}]");

            using (var db = DataAccessLayer.CreateFactory())
            {
                var result = db.Delete<GymSubscription>(x => x.UserId == userId && string.Compare(gymName, x.Name, true) == 0);
                return result > 0;
            }
        }

        public bool RemoveQuest(ulong userId, string rewardKeyword, List<string> cities)
        {
            _logger.Trace($"SubscriptionManager::RemoveQuest [UserId={userId}, RewardKeyword={rewardKeyword}, Cities={cities}]");

            using (var db = DataAccessLayer.CreateFactory())
            {
                for (var i = 0; i < cities.Count; i++)
                {
                    if (db.Delete<QuestSubscription>(x => x.UserId == userId && string.Compare(rewardKeyword.ToLower(), x.RewardKeyword.ToLower(), true) == 0 && string.Compare(cities[i], x.City, true) == 0) == 0)
                    {
                        _logger.Warn($"Could not delete quest subscription for user {userId} quest {rewardKeyword} city {cities[i]}");
                    }
                }

                return true;
            }
        }

        #endregion

        #region Remove All

        public bool RemoveAllUserSubscriptions(ulong userId)
        {
            _logger.Trace($"SubscriptionManager::RemoveAllUserSubscription [UserId={userId}]");

            using (var db = DataAccessLayer.CreateFactory())
            {
                try
                {
                    db.Delete<PokemonSubscription>(x => x.UserId == userId);
                    db.Delete<RaidSubscription>(x => x.UserId == userId);
                    db.Delete<QuestSubscription>(x => x.UserId == userId);
                    db.Delete<GymSubscription>(x => x.UserId == userId);
                    db.Delete<SubscriptionObject>(x => x.UserId == userId);
                    return true;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                }

                return true;
            }
        }

        public bool RemoveAllPokemon(ulong userId)
        {
            _logger.Trace($"SubscriptionManager::RemoveAllPokemon [UserId={userId}]");

            using (var db = DataAccessLayer.CreateFactory())
            {
                var subscription = db.LoadSingleById<SubscriptionObject>(userId);
                if (subscription == null)
                {
                    //Not subscribed.
                    return true;
                }

                return db.DeleteAll(subscription.Pokemon) > 0;
            }
        }

        public bool RemoveAllRaids(ulong userId)
        {
            _logger.Trace($"SubscriptionManager::RemoveAllRaids [UserId={userId}]");

            using (var db = DataAccessLayer.CreateFactory())
            {
                var subscription = db.LoadSingleById<SubscriptionObject>(userId);
                if (subscription == null)
                {
                    //Not subscribed.
                    return true;
                }

                return db.DeleteAll(subscription.Raids) > 0;
            }
        }

        public bool RemoveAllQuests(ulong userId)
        {
            _logger.Info($"SubscriptionManager::RemoveAllQuests [UserId={userId}]");

            using (var db = DataAccessLayer.CreateFactory())
            {
                var subscription = db.LoadSingleById<SubscriptionObject>(userId);
                if (subscription == null)
                {
                    //Not subscribed.
                    return true;
                }

                return db.DeleteAll(subscription.Quests) > 0;
            }
        }

        public bool RemoveAllGyms(ulong userId)
        {
            _logger.Info($"SubscriptionManager::RemoveAllGyms [UserId={userId}]");

            using (var db = DataAccessLayer.CreateFactory())
            {
                var subscription = db.LoadSingleById<SubscriptionObject>(userId);
                if (subscription == null)
                {
                    //Not subscribed.
                    return true;
                }

                return db.DeleteAll(subscription.Gyms) > 0;
            }
        }

        #endregion

        #region Private Methods

        private void CreateDefaultTables()
        {
            _logger.Trace($"SubscriptionManager::CreateDefaultTables");

            using (var db = DataAccessLayer.CreateFactory())
            {
                if (db == null)
                    return;

                db.CreateTable<SubscriptionObject>();
                db.CreateTable<PokemonSubscription>();
                db.CreateTable<RaidSubscription>();
                db.CreateTable<GymSubscription>();
                db.CreateTable<QuestSubscription>();
                db.CreateTable<SnoozedQuest>();

                _logger.Info($"Database tables created.");
            }
        }

        #endregion
    }
}