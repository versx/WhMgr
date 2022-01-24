namespace WhMgr.Services
{
    public abstract class IStaticsticsService
    {
        public virtual ulong TotalPokemonReceived { get; set; }
        public virtual ulong TotalPokemonMissingStatsReceived { get; set; }
        public virtual ulong TotalPokemonWithStatsReceived { get; set; }
        public virtual ulong TotalRaidsReceived { get; set; }
        public virtual ulong TotalEggsReceived { get; set; }
        public virtual ulong TotalPokestopsReceived { get; set; }
        public virtual ulong TotalQuestsReceived { get; set; }
        public virtual ulong TotalInvasionsReceived { get; set; }
        public virtual ulong TotalLuresReceived { get; set; }
        public virtual ulong TotalGymsReceived { get; set; }
        public virtual ulong TotalWeatherReceived { get; set; }


        public virtual ulong TotalPokemonSubscriptionsSent { get; set; }
        public virtual ulong TotalPvpSubscriptionsSent { get; set; }
        public virtual ulong TotalRaidSubscriptionsSent { get; set; }
        public virtual ulong TotalQuestSubscriptionsSent { get; set; }
        public virtual ulong TotalInvasionSubscriptionsSent { get; set; }
        public virtual ulong TotalLureSubscriptionsSent { get; set; }
        public virtual ulong TotalGymSubscriptionsSent { get; set; }

        // TODO: 100%, 0%, and maybe PvP rank 1 stats
    }

    public class StatisticsService : IStaticsticsService
    {
    }
}