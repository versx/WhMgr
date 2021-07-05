namespace WhMgr.Data.Contexts
{
    using System.Collections.Generic;

    using Microsoft.EntityFrameworkCore;

    using WhMgr.Data.Factories;
    using WhMgr.Services.Subscriptions.Models;

    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Subscription> Subscriptions { get; set; }

        public DbSet<PokemonSubscription> Pokemon { get; set; }

        public DbSet<PvpSubscription> Pvp { get; set; }

        public DbSet<RaidSubscription> Raids { get; set; }

        public DbSet<QuestSubscription> Quests { get; set; }

        public DbSet<GymSubscription> Gyms { get; set; }

        public DbSet<InvasionSubscription> Invasions { get; set; }

        public DbSet<LureSubscription> Lures { get; set; }

        public DbSet<LocationSubscription> Locations { get; set; }

        public DbSet<Metadata> Metadata { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            /*
            modelBuilder.Entity<Subscription>()
                .HasMany(s => s.Pokemon)
                .WithOne()
                .HasForeignKey(s => s.SubscriptionId);
            */
            //.HasConstraintName("FK_pokemon_subscriptions_subscription_id");
            //modelBuilder.Entity<Subscription>()

            modelBuilder.Entity<PokemonSubscription>()
                .HasOne(s => s.Subscription);

            modelBuilder.Entity<Subscription>()
                .HasMany(s => s.PvP)
                .WithOne();
            modelBuilder.Entity<Subscription>()
                .HasMany(s => s.Raids)
                .WithOne();
            modelBuilder.Entity<Subscription>()
                .HasMany(s => s.Quests)
                .WithOne();
            modelBuilder.Entity<Subscription>()
                .HasMany(s => s.Invasions)
                .WithOne();
            modelBuilder.Entity<Subscription>()
                .HasMany(s => s.Lures)
                .WithOne();
            modelBuilder.Entity<Subscription>()
                .HasMany(s => s.Gyms)
                .WithOne();
            modelBuilder.Entity<Subscription>()
                .HasMany(s => s.Locations)
                .WithOne();

            // Handle json columns
            modelBuilder.Entity<PokemonSubscription>()
                        .Property(nameof(PokemonSubscription.PokemonId))
                        .HasConversion(
                            DbContextFactory.CreateJsonValueConverter<List<uint>>(),
                            DbContextFactory.CreateValueComparer<uint>());
            modelBuilder.Entity<PokemonSubscription>()
                        .Property(p => p.IVList)
                        .HasConversion(
                            DbContextFactory.CreateJsonValueConverter<List<string>>(),
                            DbContextFactory.CreateValueComparer<string>());
            modelBuilder.Entity<PokemonSubscription>()
                        .Property(p => p.Areas)
                        .HasConversion(
                            DbContextFactory.CreateJsonValueConverter<List<string>>(),
                            DbContextFactory.CreateValueComparer<string>());

            modelBuilder.Entity<PvpSubscription>()
                        .Property(p => p.Areas)
                        .HasConversion(
                            DbContextFactory.CreateJsonValueConverter<List<string>>(),
                            DbContextFactory.CreateValueComparer<string>());

            modelBuilder.Entity<RaidSubscription>()
                        .Property(p => p.Areas)
                        .HasConversion(
                            DbContextFactory.CreateJsonValueConverter<List<string>>(),
                            DbContextFactory.CreateValueComparer<string>());

            modelBuilder.Entity<QuestSubscription>()
                        .Property(p => p.Areas)
                        .HasConversion(
                            DbContextFactory.CreateJsonValueConverter<List<string>>(),
                            DbContextFactory.CreateValueComparer<string>());

            modelBuilder.Entity<InvasionSubscription>()
                        .Property(p => p.Areas)
                        .HasConversion(
                            DbContextFactory.CreateJsonValueConverter<List<string>>(),
                            DbContextFactory.CreateValueComparer<string>());
            modelBuilder.Entity<InvasionSubscription>()
                        .Property(p => p.RewardPokemonId)
                        .HasConversion(
                            DbContextFactory.CreateJsonValueConverter<List<uint>>(),
                            DbContextFactory.CreateValueComparer<uint>());

            modelBuilder.Entity<LureSubscription>()
                        .Property(p => p.Areas)
                        .HasConversion(
                            DbContextFactory.CreateJsonValueConverter<List<string>>(),
                            DbContextFactory.CreateValueComparer<string>());

            modelBuilder.Entity<GymSubscription>()
                        .Property(p => p.PokemonIDs)
                        .HasConversion(
                            DbContextFactory.CreateJsonValueConverter<List<uint>>(),
                            DbContextFactory.CreateValueComparer<uint>());

            base.OnModelCreating(modelBuilder);
        }
    }
}