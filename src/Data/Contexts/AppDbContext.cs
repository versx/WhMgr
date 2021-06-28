namespace WhMgr.Data.Contexts
{
    using Microsoft.EntityFrameworkCore;

    using WhMgr.Services.Subscriptions.Models;

    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Subscription> Subscriptions { get; set; }

        public DbSet<PokemonSubscription> Pokemon { get; set; }

        public DbSet<PvPSubscription> Pvp { get; set; }

        public DbSet<RaidSubscription> Raids { get; set; }

        public DbSet<QuestSubscription> Quests { get; set; }

        public DbSet<GymSubscription> Gyms { get; set; }

        public DbSet<InvasionSubscription> Invasions { get; set; }

        public DbSet<LureSubscription> Lures { get; set; }

        public DbSet<LocationSubscription> Locations { get; set; }

        public DbSet<Metadata> Metadata { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // TODO: Handle json columns
            base.OnModelCreating(modelBuilder);
        }
    }
}