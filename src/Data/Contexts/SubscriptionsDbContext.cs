namespace WhMgr.Data.Contexts
{
    using System;

    using Microsoft.EntityFrameworkCore;

    using WhMgr.Data.Factories;
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
                      .HasConversion(x => x.ToString(), x => (PvPLeague)Enum.Parse(typeof(PvPLeague), x));
            });
            modelBuilder.Entity<PokemonSubscription>()
                        .Property(nameof(PokemonSubscription.IVList))
                        .HasConversion(DbContextFactory.CreateJsonValueConverter<string>());

            /*
            modelBuilder.Entity<PokemonSubscription>()
                        .HasOne(e => e.Subscription)
                        .WithMany(e => e.Pokemon)
                        .HasForeignKey(e => e.SubscriptionId);

            modelBuilder.Entity<PvPSubscription>()
                        .HasOne(e => e.Subscription)
                        .WithMany(e => e.PvP)
                        .HasForeignKey(e => e.SubscriptionId);

            modelBuilder.Entity<RaidSubscription>()
                        .HasOne(e => e.Subscription)
                        .WithMany(e => e.Raids)
                        .HasForeignKey(e => e.SubscriptionId);

            modelBuilder.Entity<QuestSubscription>()
                        .HasOne(e => e.Subscription)
                        .WithMany(e => e.Quests)
                        .HasForeignKey(e => e.SubscriptionId);

            modelBuilder.Entity<GymSubscription>()
                        .HasOne(e => e.Subscription)
                        .WithMany(e => e.Gyms)
                        .HasForeignKey(e => e.SubscriptionId);

            modelBuilder.Entity<InvasionSubscription>()
                        .HasOne(e => e.Subscription)
                        .WithMany(e => e.Invasions)
                        .HasForeignKey(e => e.SubscriptionId);

            modelBuilder.Entity<SubscriptionObject>()
                        .HasMany(e => e.Pokemon)
                        .WithOne(e => e.Subscription)
                        .HasForeignKey(e => e.SubscriptionId);

            modelBuilder.Entity<SubscriptionObject>()
                        .HasMany(e => e.PvP)
                        .WithOne(e => e.Subscription)
                        .HasForeignKey(e => e.SubscriptionId);

            modelBuilder.Entity<SubscriptionObject>()
                        .HasMany(e => e.Raids)
                        .WithOne(e => e.Subscription)
                        .HasForeignKey(e => e.SubscriptionId);

            modelBuilder.Entity<SubscriptionObject>()
                        .HasMany(e => e.Quests)
                        .WithOne(e => e.Subscription)
                        .HasForeignKey(e => e.SubscriptionId);

            modelBuilder.Entity<SubscriptionObject>()
                        .HasMany(e => e.Gyms)
                        .WithOne(e => e.Subscription)
                        .HasForeignKey(e => e.SubscriptionId);

            modelBuilder.Entity<SubscriptionObject>()
                        .HasMany(e => e.Invasions)
                        .WithOne(e => e.Subscription)
                        .HasForeignKey(e => e.SubscriptionId);
            */

            base.OnModelCreating(modelBuilder);
        }
    }
}