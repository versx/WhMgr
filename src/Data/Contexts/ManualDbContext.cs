namespace WhMgr.Data.Contexts
{
    using Microsoft.EntityFrameworkCore;

    using WhMgr.Data.Models;

    public class ManualDbContext : DbContext
    {
        public ManualDbContext(DbContextOptions<ManualDbContext> options)
            : base(options)
        {
        }

        public DbSet<Nest> Nests { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}