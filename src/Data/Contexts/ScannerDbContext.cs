namespace WhMgr.Data.Contexts
{
    using System;

    using Microsoft.EntityFrameworkCore;

    using WhMgr.Data.Factories;
    using WhMgr.Data.Models;
    using WhMgr.Net.Models;

    class ScannerDbContext : DbContext
    {
        public DbSet<PokemonData> Pokemon { get; set; }

        //public DbSet<RaidData> Raids { get; set; }

        public DbSet<GymDetailsData> Gyms { get; set; }

        public DbSet<Pokestop> Pokestops { get; set; }

        //public DbSet<QuestData> Quests { get; set; }

        public DbSet<WeatherData> Weather { get; set; }

        public DbSet<PokemonStatsShiny> PokemonStatsShiny { get; set; }

        public DbSet<PokemonStatsIV> PokemonStatsIV { get; set; }

        public ScannerDbContext(DbContextOptions<ScannerDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PokemonStatsShiny>(entity => entity.HasNoKey());
            modelBuilder.Entity<PokemonStatsIV>(entity => entity.HasNoKey());
            modelBuilder.Entity<PokemonData>()
                        .Property(e => e.Gender)
                        .HasConversion(x => x.ToString(), x => (PokemonGender)Enum.Parse(typeof(PokemonGender), x));
            modelBuilder.Entity<RaidData>()
                        .Property(e => e.Gender)
                        .HasConversion(x => x.ToString(), x => (PokemonGender)Enum.Parse(typeof(PokemonGender), x));
            /*
            modelBuilder.Entity<Pokestop>(entity =>
            {
                entity.Property(e => e.)
                    .HasConversion(x => x.ToString(),
                        x => (PokestopLureType)Enum.Parse(typeof(PokestopLureType), x));
                entity.Property(e => e.GruntType)
                    .HasConversion(x => x.ToString(),
                        x => (InvasionGruntType)Enum.Parse(typeof(InvasionGruntType), x));
            });
            */
            modelBuilder.Entity<WeatherData>()
                        .Property(e => e.GameplayCondition)
                        .HasConversion(x => x.ToString(), x => (WeatherType)Enum.Parse(typeof(WeatherType), x));
            modelBuilder.Entity<QuestData>()
                        .Property(nameof(QuestData.Rewards))
                        .HasConversion(DbContextFactory.CreateJsonValueConverter<QuestRewardMessage>());
            modelBuilder.Entity<QuestData>()
                        .Property(nameof(QuestData.Conditions))
                        .HasConversion(DbContextFactory.CreateJsonValueConverter<QuestConditionMessage>());
            base.OnModelCreating(modelBuilder);
        }
    }
}
