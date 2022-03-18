namespace WhMgr.Services.Subscriptions
{
    using System.Collections.Generic;

    using WhMgr.Common;

    // TODO: Cache in memory and save to database every x minutes/hours

    public interface ISubscriptionTrackerService
    {
        Dictionary<(ulong, ulong), Dictionary<SubscriptionTrackerType, ISubscriptionTracker>> Trackers { get; }
    }

    public class SubscriptionTrackerService : ISubscriptionTrackerService
    {
        public Dictionary<(ulong, ulong), Dictionary<SubscriptionTrackerType, ISubscriptionTracker>> Trackers { get; }

        // TODO: Singleton

        public SubscriptionTrackerService()
        {
            Trackers = new();
        }

        public void Increment((ulong, ulong) userKey, SubscriptionTrackerType trackerType, uint amount)
        {
            //var userKey = (guildId, userId);
            if (!Trackers.ContainsKey(userKey))
            {
                var trackerManifest = BuildDefaultTrackerManifest();
                Trackers.Add(userKey, trackerManifest);
            }
            else
            {
                ((SubscriptionTracker)Trackers[userKey][trackerType]).Count += amount;
            }
        }

        private static Dictionary<SubscriptionTrackerType, ISubscriptionTracker> BuildDefaultTrackerManifest()
        {
            return new Dictionary<SubscriptionTrackerType, ISubscriptionTracker>
            {
                { SubscriptionTrackerType.Pokemon, new SubscriptionTracker() },
                { SubscriptionTrackerType.PvP, new SubscriptionTracker() },
                { SubscriptionTrackerType.Raids, new SubscriptionTracker() },
                { SubscriptionTrackerType.Quests, new SubscriptionTracker() },
                { SubscriptionTrackerType.Invasions, new SubscriptionTracker() },
                { SubscriptionTrackerType.Lures, new SubscriptionTracker() },
                { SubscriptionTrackerType.Gyms, new SubscriptionTracker() },
            };
        }
    }


    public interface ISubscriptionTracker
    {
        ulong GuildId { get; }

        ulong UserId { get; }

        ulong Count { get; }
    }

    public interface IPokemonSubscriptionTracker : ISubscriptionTracker
    {
        uint PokemonId { get; }
    }

    public interface IPvpSubscriptionTracker : IPokemonSubscriptionTracker
    {
        bool IsGreatLeague { get; }

        bool IsUltraLeague { get; }

        uint Rank { get; }
    }

    public interface IRaidSubscriptionTracker : IPokemonSubscriptionTracker
    {
        ushort Level { get; }
    }


    public interface IPokestopSubscriptionTracker : ISubscriptionTracker
    {
        string PokestopName { get; }
    }


    public interface IQuestSubscriptionTracker : IPokestopSubscriptionTracker
    {
        string RewardKeyword { get; }
    }

    public interface IInvasionSubscriptionTracker : IPokestopSubscriptionTracker
    {
        ushort GruntType { get; }

        uint RewardPokemonId { get; }
    }

    public interface ILureSubscriptionTracker : IPokestopSubscriptionTracker
    {
        PokestopLureType LureType { get; }
    }

    public interface IGymSubscriptionTracker : ISubscriptionTracker
    {
        string GymName { get; }

        ushort Level { get; }

        uint PokemonId { get; }
    }


    public class SubscriptionTracker : ISubscriptionTracker
    {
        public ulong GuildId { get; set; }

        public ulong UserId { get; set; }

        public ulong Count { get; set; }
    }

    public enum SubscriptionTrackerType
    {
        Pokemon,
        PvP,
        Raids,
        Quests,
        Invasions,
        Lures,
        Gyms,
    }
}