namespace WhMgr.Data.Factories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.ChangeTracking;
    using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

    using WhMgr.Data.Contexts;
    using WhMgr.Extensions;

    /*
     * Reference: https://docs.microsoft.com/en-us/ef/core/cli/dbcontext-creation?tabs=dotnet-core-cli
    public class DbContextFactory2<T> : Microsoft.EntityFrameworkCore.Design.IDesignTimeDbContextFactory<T> where T : DbContext
    {
        public T CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<T>();
            optionsBuilder.UseMySql(args[0], ServerVersion.AutoDetect(args[0]));

            return new T(optionsBuilder.Options);
        }
    }
    */

    public class DbContextFactory
    {
        public static ManualDbContext CreateManualContext(string connectionString)// where T : DbContext
        {
            try
            {
                var optionsBuilder = new DbContextOptionsBuilder<ManualDbContext>();
                optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));

                var ctx = new ManualDbContext(optionsBuilder.Options);
                //ctx.ChangeTracker.AutoDetectChangesEnabled = false;
                return ctx;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex}");
                Environment.Exit(0);
            }
            return null;
        }

        public static MapDbContext CreateMapContext(string connectionString)// where T : DbContext
        {
            try
            {
                var optionsBuilder = new DbContextOptionsBuilder<MapDbContext>();
                optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));

                var ctx = new MapDbContext(optionsBuilder.Options);
                //ctx.ChangeTracker.AutoDetectChangesEnabled = false;
                return ctx;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex}");
                Environment.Exit(0);
            }
            return null;
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