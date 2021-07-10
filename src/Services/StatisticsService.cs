namespace WhMgr.Services
{
    public interface IStaticsticsService
    {
        ulong TotalPokemonReceived { get; set; }
        ulong TotalPokemonMissingStatsReceived { get; set; }
        ulong TotalPokemonWithStatsReceived { get; set; }
        ulong TotalRaidsReceived { get; set; }
        ulong TotalEggsReceived { get; set; }
        ulong TotalPokestopsReceived { get; set; }
        ulong TotalQuestsReceived { get; set; }
        ulong TotalInvasionsReceived { get; set; }
        ulong TotalLuresReceived { get; set; }
        ulong TotalGymsReceived { get; set; }
        ulong TotalWeatherReceived { get; set; }

        
        ulong TotalPokemonSubscriptionsSent { get; set; }
        ulong TotalPvpSubscriptionsSent { get; set; }
        ulong TotalRaidSubscriptionsSent { get; set; }
        ulong TotalQuestSubscriptionsSent { get; set; }
        ulong TotalInvasionSubscriptionsSent { get; set; }
        ulong TotalLureSubscriptionsSent { get; set; }
        ulong TotalGymSubscriptionsSent { get; set; }

        // TODO: 100%, 0%, and maybe PvP rank 1 stats
    }

    public class StatisticsService : IStaticsticsService
    {
        public ulong TotalPokemonReceived { get; set; }

        public ulong TotalPokemonMissingStatsReceived { get; set; }

        public ulong TotalPokemonWithStatsReceived { get; set; }

        public ulong TotalPvpSubscriptionsSent { get; set; }

        public ulong TotalRaidsReceived { get; set; }

        public ulong TotalEggsReceived { get; set; }

        public ulong TotalPokestopsReceived { get; set; }

        public ulong TotalQuestsReceived { get; set; }

        public ulong TotalInvasionsReceived { get; set; }

        public ulong TotalLuresReceived { get; set; }

        public ulong TotalGymsReceived { get; set; }

        public ulong TotalWeatherReceived { get; set; }

        public ulong TotalPokemonSubscriptionsSent { get; set; }

        public ulong TotalRaidSubscriptionsSent { get; set; }

        public ulong TotalQuestSubscriptionsSent { get; set; }

        public ulong TotalInvasionSubscriptionsSent { get; set; }

        public ulong TotalLureSubscriptionsSent { get; set; }

        public ulong TotalGymSubscriptionsSent { get; set; }
    }
}