namespace WhMgr.Data.Contexts
{
    using Microsoft.EntityFrameworkCore;

    using WhMgr.Services.Webhook.Models;

    public class MapDbContext : DbContext
    {
        public MapDbContext(DbContextOptions<MapDbContext> options)
            : base(options)
        {
        }

        public DbSet<PokestopData> Pokestops { get; set; }

        public DbSet<GymDetailsData> Gyms { get; set; }

        public DbSet<WeatherData> Weather { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}