namespace WhMgr.Extensions
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;

    using WhMgr.Configuration;

    public static class DbContextExtensions
    {
        public static void AddDatabase<T>(this IServiceCollection services, DatabaseConfig dbConfig)
            where T : DbContext
        {
            AddDatabase<T>(services, dbConfig.ToString());
        }

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