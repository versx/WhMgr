namespace WhMgr.Data.Contexts
{
    using System;
    using System.Collections.Generic;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
    using Newtonsoft.Json;

    using WhMgr.Data.Models;
    using WhMgr.Data.Subscriptions.Models;

    public class SubscriptionsDbContext : DbContext
    {
        public DbSet<SubscriptionObject> Subscriptions { get; set; }

        public DbSet<PokemonSubscription> Pokemon { get; set; }

        public DbSet<PvPSubscription> PvP { get; set; }

        public DbSet<RaidSubscription> Raids { get; set; }

        public DbSet<QuestSubscription> Quests { get; set; }

        public DbSet<InvasionSubscription> Invasions { get; set; }

        public DbSet<GymSubscription> Gyms { get; set; }

        public DbSet<Metadata> Metadata { get; set; }

        public SubscriptionsDbContext(DbContextOptions<SubscriptionsDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PvPSubscription>(entity =>
            {
                entity.Property(e => e.League)
                    .HasConversion(x => x.ToString(),
                        x => (PvPLeague)Enum.Parse(typeof(PvPLeague), x));
            });
            var jsonListConverter = new ValueConverter<List<string>, string>(v => JsonConvert.SerializeObject(v), v => JsonConvert.DeserializeObject<List<string>>(v));
            modelBuilder.Entity<PokemonSubscription>()
                .Property(nameof(PokemonSubscription.IVList))
                .HasConversion(jsonListConverter);
            base.OnModelCreating(modelBuilder);
        }
    }
}