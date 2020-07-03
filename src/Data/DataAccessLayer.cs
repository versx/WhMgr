namespace WhMgr.Data
{
    using System.Collections.Generic;

    using ServiceStack.OrmLite;
    using ServiceStack.Text;

    /// <summary>
    /// Database connection class
    /// </summary>
    public static class DataAccessLayer
    {
        /// <summary>
        /// Gets the connection factories available
        /// </summary>
        public static Dictionary<string, OrmLiteConnectionFactory> Factories { get; } = new Dictionary<string, OrmLiteConnectionFactory>();

        /// <summary>
        /// Gets or sets the database connection string to use
        /// </summary>
        public static string ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the scanner database connection string to use
        /// </summary>
        public static string ScannerConnectionString { get; set; }

        /// <summary>
        /// Creates a new connection factory
        /// </summary>
        /// <returns>Returns a new <see cref="OrmLiteConnectionFactory"/> class</returns>
        public static OrmLiteConnectionFactory CreateFactory()
        {
            return CreateFactory(ConnectionString);
        }

        /// <summary>
        /// Creates a new connection factory with the specified connection string
        /// </summary>
        /// <param name="connectionString">Database connection string</param>
        /// <returns>Returns a new <see cref="OrmLiteConnectionFactory"/> class</returns>
        public static OrmLiteConnectionFactory CreateFactory(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
                return null;

            if (!Factories.ContainsKey(connectionString))
            {
                var provider = MySqlDialect.Provider;
                provider.StringSerializer = new JsonStringSerializer();
                Factories.Add(connectionString, new OrmLiteConnectionFactory(connectionString, provider));
            }

            //var factory = new OrmLiteConnectionFactory(connectionString, MySqlDialect.Provider);
            //return factory.Open();
            return Factories[connectionString];
        }
    }
}