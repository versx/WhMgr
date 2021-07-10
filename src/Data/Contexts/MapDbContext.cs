namespace WhMgr.Data.Contexts
{
    using Microsoft.EntityFrameworkCore;
    using System.Collections.Generic;
    using WhMgr.Data.Factories;
    using WhMgr.Data.Models;
    using WhMgr.Services.Webhook.Models;

    public class MapDbContext : DbContext
    {
        public MapDbContext(DbContextOptions<MapDbContext> options)
            : base(options)
        {
        }

        public DbSet<PokemonData> Pokemon { get; set; }

        public DbSet<PokestopData> Pokestops { get; set; }

        public DbSet<GymDetailsData> Gyms { get; set; }

        public DbSet<WeatherData> Weather { get; set; }

        public DbSet<PokemonStatsIV> PokemonStatsIV { get; set; }

        public DbSet<PokemonStatsShiny> PokemonStatsShiny { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PvpRankData>()
                        .HasNoKey();

            modelBuilder.Entity<PokemonData>()
                        .Property(p => p.GreatLeague)
                        .HasConversion(DbContextFactory.CreateJsonValueConverter<List<PvpRankData>>());
            modelBuilder.Entity<PokemonData>()
                        .Property(p => p.UltraLeague)
                        .HasConversion(DbContextFactory.CreateJsonValueConverter<List<PvpRankData>>());

            modelBuilder.Entity<PokemonStatsIV>()
                        .HasKey(p => new { p.Date, p.PokemonId });

            modelBuilder.Entity<PokemonStatsShiny>()
                        .HasKey(p => new { p.Date, p.PokemonId });

            base.OnModelCreating(modelBuilder);
        }
    }
}