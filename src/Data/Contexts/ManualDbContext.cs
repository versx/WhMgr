namespace WhMgr.Data.Contexts
{
    using Microsoft.EntityFrameworkCore;

    using WhMgr.Data.Models;

    class ManualDbContext : DbContext
    {
        public DbSet<Nest> Nests { get; set; }

        public ManualDbContext(DbContextOptions<ManualDbContext> options)
            : base(options)
        {
        }
    }
}