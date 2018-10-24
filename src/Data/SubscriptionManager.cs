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
                var subscription = db.LoadSelect<SubscriptionObject>(x => x.UserId == userId).FirstOrDefault();
                if (subscription == null)
                {
                    //Create new subscription object.
                    subscription = new SubscriptionObject
                    {
                        UserId = userId,
                        Enabled = enabled,
                        Pokemon = new List<PokemonSubscription>(),
                        Raids = new List<RaidSubscription>()
                    };
                }

                subscription.Enabled = enabled;
                db.Save(subscription, true);

                return subscription.Enabled == enabled;
            }
        }

        public bool UserExists(ulong userId)
        {
            using (var db = DataAccessLayer.CreateFactory())
            {
                var subscription = db.LoadSelect<SubscriptionObject>(x => x.UserId == userId).FirstOrDefault();
                return subscription != null;
            }
        }

        public SubscriptionObject GetUserSubscriptions(ulong userId)
        {
            _logger.Trace($"SubscriptionManager::GetUserSubscriptions [UserId={userId}]");

            using (var db = DataAccessLayer.CreateFactory())
            {
                var subscription = db.LoadSelect<SubscriptionObject>(x => x.UserId == userId).FirstOrDefault();
                return subscription;
            }
        }

        #region Add

        public bool AddPokemon(ulong userId, int pokemonId, int iv = 0, int lvl = 0, string gender = "*")
        {
            _logger.Trace($"SubscriptionManager::AddPokemon [UserId={userId}, PokemonId={pokemonId}, IV={iv}, Level={lvl}, Gender={gender}]");

            using (var db = DataAccessLayer.CreateFactory())
            {
                var subscription = db.LoadSelect<SubscriptionObject>(x => x.UserId == userId).FirstOrDefault();
                if (subscription == null)
                {
                    //Create new subscription object.
                    subscription = new SubscriptionObject
                    {
                        UserId = userId,
                        Enabled = true,
                        Pokemon = new List<PokemonSubscription>(),
                        Raids = new List<RaidSubscription>()
                    };
                }

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
                        Console.WriteLine($"Pokemon Added!");
                    }
                    else
                    {
                        Console.WriteLine("Pokemon Updated!");
                    }

                    Console.WriteLine(db.LastInsertId());

                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERROR: {ex}");
                    return false;
                }
            }
        }

        public bool AddRaid(ulong userId, int pokemonId, string city)
        {
            _logger.Trace($"SubscriptionManager::AddRaid [UserId={userId}, PokemonId={pokemonId}, City={city}]");

            using (var db = DataAccessLayer.CreateFactory())
            {
                var subscription = db.LoadSelect<SubscriptionObject>(x => x.UserId == userId).FirstOrDefault();
                if (subscription == null)
                {
                    //Create new subscription object.
                    subscription = new SubscriptionObject
                    {
                        UserId = userId,
                        Enabled = true,
                        Pokemon = new List<PokemonSubscription>(),
                        Raids = new List<RaidSubscription>()
                    };
                }

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
                        Console.WriteLine($"Raid Added!");
                    }
                    else
                    {
                        Console.WriteLine("Raid Updated!");
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERROR: {ex}");
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

        #region Add All

        public bool AddAllPokemon(ulong userId, int iv = 0, int lvl = 0, string gender = "*")
        {
            _logger.Trace($"SubscriptionManager::AddPokemonAll [UserId={userId}, IV={iv}, Level={lvl}, Gender={gender}]");

            return true;
        }

        public bool AddAllRaids(ulong userId)
        {
            _logger.Trace($"SubscriptionManager::AddRaidsAll [UserId={userId}]");

            return true;
        }

        #endregion

        #region Remove

        public bool RemovePokemon(ulong userId, int pokemonId)
        {
            _logger.Trace($"SubscriptionManager::RemovePokemon [UserId={userId}, PokemonId={pokemonId}]");

            using (var db = DataAccessLayer.CreateFactory())
            {
                var subscription = db.LoadSelect<SubscriptionObject>(x => x.UserId == userId).FirstOrDefault();
                if (subscription == null)
                {
                    //Not subscribed.
                    return false;
                }

                var pkmnSub = subscription.Pokemon.FirstOrDefault(x => x.PokemonId == pokemonId);
                if (pkmnSub == null)
                {
                    //Not subscribed.
                    return true;
                }
                else
                {
                    //Subscription exists.
                    //var result = subscription.Pokemon.Remove(pkmnSub);
                    //db.Save(subscription, true);
                    //return true;
                    var result = db.Delete(pkmnSub);
                    return result > 0;
                }
            }
        }

        public bool RemoveRaid(ulong userId, int pokemonId, string city)
        {
            _logger.Trace($"SubscriptionManager::RemoveRaid [UserId={userId}, PokemonId={pokemonId}, City={city}]");

            using (var db = DataAccessLayer.CreateFactory())
            {
                var subscription = db.LoadSelect<SubscriptionObject>(x => x.UserId == userId).FirstOrDefault();
                if (subscription == null)
                {
                    //Not subscribed.
                    return false;
                }

                var raidSub = subscription.Raids.FirstOrDefault(x => x.PokemonId == pokemonId && string.Compare(x.City, city, true) == 0);
                if (raidSub == null)
                {
                    //Not subscribed.
                    return true;
                }
                else
                {
                    //Subscription exists.
                    //var result = subscription.Raids.Remove(raidSub);//All(x => x.PokemonId == pokemonId && string.Compare(x.City, city, true) == 0);
                    //db.Save(subscription, true);
                    var result = db.Delete(raidSub);
                    return result > 0;
                }
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

        public bool RemoveRaid(ulong userId, List<int> pokemonIds, string city)
        {
            _logger.Trace($"SubscriptionManager::RemoveRaid [UserId={userId}, PokemonIds={string.Join(", ", pokemonIds)}, City={city}]");

            var errors = 0;
            for (var i = 0; i < pokemonIds.Count; i++)
            {
                if (!RemoveRaid(userId, pokemonIds[i], city))
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
                var subscription = db.LoadSelect<SubscriptionObject>(x => x.UserId == userId).FirstOrDefault();
                if (subscription == null)
                {
                    //Not subscribed.
                    return false;
                }

                return db.DeleteAll(subscription.Pokemon) > 0;
            }
        }

        public bool RemoveAllRaids(ulong userId)
        {
            _logger.Trace($"SubscriptionManager::RemoveAllRaids [UserId={userId}]");

            using (var db = DataAccessLayer.CreateFactory())
            {
                var subscription = db.LoadSelect<SubscriptionObject>(x => x.UserId == userId).FirstOrDefault();
                if (subscription == null)
                {
                    //Not subscribed.
                    return false;
                }

                return db.DeleteAll(subscription.Raids) > 0;
            }
        }

        #endregion

        #region Private Methods

        private void CreateDefaultTables()
        {
            _logger.Trace($"SubscriptionManager::CreateDefaultTables");

            using (var db = DataAccessLayer.CreateFactory())
            {
                db.CreateTable<SubscriptionObject>();
                db.CreateTable<PokemonSubscription>();
                db.CreateTable<RaidSubscription>();
            }
        }

        #endregion
    }
}