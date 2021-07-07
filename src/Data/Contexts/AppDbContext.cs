namespace WhMgr.Data.Contexts
{
    using System.Collections.Generic;

    using Microsoft.EntityFrameworkCore;

    using WhMgr.Common;
    using WhMgr.Extensions;
    using WhMgr.Data.Factories;
    using WhMgr.Services.Subscriptions.Models;

    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
            // Migrate to latest
            //var createSql = Database.GenerateCreateScript();
            //System.Console.WriteLine($"CreateSql: {createSql}");
            base.Database.Migrate();
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
            // Handle json columns
            modelBuilder.Entity<PokemonSubscription>()
                        .Property(p => p.PokemonId)
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
                        .Property(p => p.PokemonId)
                        .HasConversion(
                            DbContextFactory.CreateJsonValueConverter<List<uint>>(),
                            DbContextFactory.CreateValueComparer<uint>());
            modelBuilder.Entity<PvpSubscription>()
                        .Property(p => p.League)
                        .HasConversion(x => x.ObjectToString(), x => x.StringToObject<PvpLeague>());
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
                        .Property(p => p.LureType)
                        .HasConversion(x => x.ObjectToString(), x => x.StringToObject<PokestopLureType>());
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