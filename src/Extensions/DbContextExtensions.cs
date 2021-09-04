namespace WhMgr.Extensions
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;

    public static class DbContextExtensions
    {
        public static void AddDatabase<T>(this IServiceCollection services, string connectionString)
            where T : DbContext
        {
            services.AddDbContextFactory<T>(options =>
                options.UseMySql(
                    connectionString,
                    ServerVersion.AutoDetect(connectionString)
                ), ServiceLifetime.Singleton
            );

            services.AddDbContext<T>(options =>
                options.UseMySql(
                    connectionString,
                    ServerVersion.AutoDetect(connectionString)
                ), ServiceLifetime.Scoped
            );
        }
    }
}