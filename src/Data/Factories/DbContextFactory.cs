namespace WhMgr.Data.Factories
{
    using System;
    using System.Collections.Generic;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
    using Newtonsoft.Json;

    using WhMgr.Data.Contexts;

    class DbContextFactory
    {
        /// <summary>
        /// Gets or sets the database connection string to use
        /// </summary>
        public static string ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the scanner database connection string to use
        /// </summary>
        public static string ScannerConnectionString { get; set; }


        public static SubscriptionsDbContext CreateSubscriptionContext(string connectionString)
        {
            var optionsBuilder = new DbContextOptionsBuilder<SubscriptionsDbContext>();
            optionsBuilder.UseMySQL(connectionString);

            var context = new SubscriptionsDbContext(optionsBuilder.Options);
            context.ChangeTracker.AutoDetectChangesEnabled = false;
            return context;
        }

        public static ManualDbContext CreateManualDbContext(string connectionString)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ManualDbContext>();
            optionsBuilder.UseMySQL(connectionString);

            var context = new ManualDbContext(optionsBuilder.Options);
            return context;
        }

        public static ScannerDbContext CreateScannerDbContext(string connectionString)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ScannerDbContext>();
            optionsBuilder.UseMySQL(connectionString);

            var context = new ScannerDbContext(optionsBuilder.Options);
            return context;
        }


        public static ValueConverter<List<T>, string> CreateJsonValueConverter<T>()
        {
            return new ValueConverter<List<T>, string>
            (
                v => JsonConvert.SerializeObject(v),
                v => JsonConvert.DeserializeObject<List<T>>(v)
            );
        }
    }
}