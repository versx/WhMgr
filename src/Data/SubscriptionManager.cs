namespace WhMgr.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using ServiceStack.OrmLite;

    using WhMgr.Data.Models;
    using WhMgr.Diagnostics;

    public class SubscriptionManager
    {
        private static readonly IEventLogger _logger = EventLogger.GetLogger();

        public SubscriptionManager()
        {
            _logger.Trace($"SubscriptionManager::SubscriptionManager");

            CreateDefaultTables();
        }

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
                       subscription.Latitude == latitude &&
                       subscription.Longitude == longitude;
            }
        }

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
            //_logger.Trace($"SubscriptionManager::GetUserSubscriptions [UserId={userId}]");

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
            //_logger.Trace($"SubscriptionManager::GetUserSubscriptionsByPokemonId [PokemonId={pokeId}]");

            var subscriptions = GetUserSubscriptions();
            if (subscriptions != null)
            {
                return subscriptions.Where(x => x.Pokemon.Exists(y => y.PokemonId == pokeId)).ToList();
            }

            return null;
        }

        public List<SubscriptionObject> GetUserSubscriptionsByRaidBossId(int pokeId)
        {
            //_logger.Trace($"SubscriptionManager::GetUserSubscriptionsByRaidBossId [PokemonId={pokeId}]");

            var subscriptions = GetUserSubscriptions();
            if (subscriptions != null)
            {
                return subscriptions.Where(x => x.Raids.Exists(y => y.PokemonId == pokeId)).ToList();
            }

            return null;
        }

        public List<SubscriptionObject> GetUserSubscriptions()
        {
            //_logger.Trace($"SubscriptionManager::GetUserSubscriptions");

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

        #region Add

        public bool AddPokemon(ulong userId, int pokemonId, int iv = 0, int lvl = 0, string gender = "*")
        {
            _logger.Trace($"SubscriptionManager::AddPokemon [UserId={userId}, PokemonId={pokemonId}, IV={iv}, Level={lvl}, Gender={gender}]");

            using (var db = DataAccessLayer.CreateFactory())
            {
                var subscription = GetUserSubscriptions(userId);

                //Subscription exists.
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
                    var result = db.Save(subscription, true);
                    if (result)
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
                    return false;
                }
            }
        }

        public bool AddRaid(ulong userId, int pokemonId, string city)
        {
            _logger.Trace($"SubscriptionManager::AddRaid [UserId={userId}, PokemonId={pokemonId}, City={city}]");

            using (var db = DataAccessLayer.CreateFactory())
            {
                var subscription = GetUserSubscriptions(userId);

                //Subscription exists.
                var raidSub = subscription.Raids.FirstOrDefault(x => x.PokemonId == pokemonId && x.City == city);
                if (raidSub == null)
                {
                    //Create new raid subscription object.
                    raidSub = new RaidSubscription
                    {
                        PokemonId = pokemonId,
                        UserId = userId,
                        City = city
                    };
                    subscription.Raids.Add(raidSub);
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
                        _logger.Debug($"Raid Added!");
                    }
                    else
                    {
                        _logger.Debug("Raid Updated!");
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

        public bool AddQuest(ulong userId, string rewardKeyword, string city)
        {
            _logger.Trace($"SubscriptionManager::AddQuest [UserId={userId}, RewardKeyword={rewardKeyword}, City={city}]");

            using (var db = DataAccessLayer.CreateFactory())
            {
                var subscription = GetUserSubscriptions(userId);

                //Subscription exists.
                var questSub = subscription.Quests.FirstOrDefault(x => rewardKeyword.ToLower().Contains(x.RewardKeyword.ToLower()) && x.City == city);
                if (questSub == null)
                {
                    //Create new raid subscription object.
                    questSub = new QuestSubscription
                    {
                        UserId = userId,
                        RewardKeyword = rewardKeyword,
                        City = city
                    };
                    subscription.Quests.Add(questSub);
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
                        _logger.Debug($"Quest Added!");
                    }
                    else
                    {
                        _logger.Debug("Quest Updated!");
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

        public bool AddPokemon(ulong userId, List<int> pokemonIds, int iv = 0, int lvl = 0, string gender = "*")
        {
            _logger.Trace($"SubscriptionManager::AddPokemon [UserId={userId}, PokemonIds={string.Join(", ", pokemonIds)}, IV={iv}, Level={lvl}, Gender={gender}]");

            var errors = 0;
            for (var i = 0; i < pokemonIds.Count; i++)
            {
                if (!AddPokemon(userId, pokemonIds[i], iv, lvl, gender))
                {
                    errors++;
                }
            }

            return errors == 0;
        }

        public bool AddRaid(ulong userId, List<int> pokemonIds, string city)
        {
            _logger.Trace($"SubscriptionManager::AddRaid [UserId={userId}, PokemonIds={string.Join(", ", pokemonIds)}, City={city}]");

            var errors = 0;
            for (var i = 0; i < pokemonIds.Count; i++)
            {
                if (!AddRaid(userId, pokemonIds[i], city))
                {
                    errors++;
                }
            }

            return errors == 0;
        }

        #endregion

        #region Remove

        public bool RemovePokemon(ulong userId, int pokemonId)
        {
            _logger.Trace($"SubscriptionManager::RemovePokemon [UserId={userId}, PokemonId={pokemonId}]");

            using (var db = DataAccessLayer.CreateFactory())
            {
                var result = db.Delete<PokemonSubscription>(x => x.PokemonId == pokemonId);
                return result > 0;
            }
        }

        public bool RemoveRaid(ulong userId, List<int> pokemonIds, List<string> cities)
        {
            _logger.Trace($"SubscriptionManager::RemoveRaid [UserId={userId}, PokemonId={pokemonIds}, Cities={cities}]");

            using (var db = DataAccessLayer.CreateFactory())
            {
                //TODO: Foreach raid subscription instead of Forloop
                for (var i = 0; i < pokemonIds.Count; i++)
                {
                    if (db.Delete<RaidSubscription>(x=>x.PokemonId == pokemonIds[i] && cities.Contains(x.City)) == 0)
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
                var result = db.Delete<GymSubscription>(x => string.Compare(gymName, x.Name, true) == 0);
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
                    if (db.Delete<QuestSubscription>(x => string.Compare(rewardKeyword.ToLower(), x.RewardKeyword.ToLower(), true) == 0 && string.Compare(cities[i], x.City, true) == 0) == 0)
                    {
                        _logger.Warn($"Could not delete quest subscription for user {userId} quest {rewardKeyword} city {cities[i]}");
                    }
                }

                return true;
            }
        }

        public bool RemovePokemon(ulong userId, List<int> pokemonIds)
        {
            _logger.Trace($"SubscriptionManager::RemovePokemon [UserId={userId}, PokemonIds={string.Join(", ", pokemonIds)}]");

            var errors = 0;
            for (var i = 0; i < pokemonIds.Count; i++)
            {
                if (!RemovePokemon(userId, pokemonIds[i]))
                {
                    errors++;
                }
            }

            return errors == 0;
        }

        #endregion

        #region Remove All

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

                _logger.Info($"Database tables created.");
            }
        }

        #endregion
    }
}