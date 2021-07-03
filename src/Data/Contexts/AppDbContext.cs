namespace WhMgr.Data.Contexts
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.ChangeTracking;
    using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

    using WhMgr.Extensions;
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
                        .HasConversion(CreateJsonValueConverter<List<uint>>(), CreateValueComparer<uint>());
            modelBuilder.Entity<PokemonSubscription>()
                        .Property(p => p.IVList)
                        .HasConversion(CreateJsonValueConverter<List<string>>(), CreateValueComparer<string>());
            modelBuilder.Entity<PokemonSubscription>()
                        .Property(p => p.Areas)
                        .HasConversion(CreateJsonValueConverter<List<string>>(), CreateValueComparer<string>());

            modelBuilder.Entity<PvpSubscription>()
                        .Property(p => p.Areas)
                        .HasConversion(CreateJsonValueConverter<List<string>>(), CreateValueComparer<string>());

            modelBuilder.Entity<RaidSubscription>()
                        .Property(p => p.Areas)
                        .HasConversion(CreateJsonValueConverter<List<string>>(), CreateValueComparer<string>());

            modelBuilder.Entity<QuestSubscription>()
                        .Property(p => p.Areas)
                        .HasConversion(CreateJsonValueConverter<List<string>>(), CreateValueComparer<string>());

            modelBuilder.Entity<InvasionSubscription>()
                        .Property(p => p.Areas)
                        .HasConversion(CreateJsonValueConverter<List<string>>(), CreateValueComparer<string>());
            modelBuilder.Entity<InvasionSubscription>()
                        .Property(p => p.RewardPokemonId)
                        .HasConversion(CreateJsonValueConverter<List<uint>>(), CreateValueComparer<uint>());

            modelBuilder.Entity<LureSubscription>()
                        .Property(p => p.Areas)
                        .HasConversion(CreateJsonValueConverter<List<string>>(), CreateValueComparer<string>());

            modelBuilder.Entity<GymSubscription>()
                        .Property(p => p.PokemonIDs)
                        .HasConversion(CreateJsonValueConverter<List<uint>>(), CreateValueComparer<uint>());

            base.OnModelCreating(modelBuilder);
        }

        public static ValueConverter<T, string> CreateJsonValueConverter<T>()
        {
            return new ValueConverter<T, string>
            (
                v => v.ToJson(),
                v => v.FromJson<T>()
            );
        }

        public static ValueComparer<List<T>> CreateValueComparer<T>()
        {
            return new ValueComparer<List<T>>
            (
                (c1, c2) => c1.SequenceEqual(c2),
                c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                c => c.ToList()
            );
        }
    }
}