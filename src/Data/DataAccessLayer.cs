namespace WhMgr.Data
{
    using System.Collections.Generic;

    using ServiceStack.OrmLite;
    using ServiceStack.Text;

    public static class DataAccessLayer
    {
        public static Dictionary<string, OrmLiteConnectionFactory> Factories { get; } = new Dictionary<string, OrmLiteConnectionFactory>();

        public static string ConnectionString { get; set; }

        public static string ScannerConnectionString { get; set; }

        public static OrmLiteConnectionFactory CreateFactory()
        {
            return CreateFactory(ConnectionString);
        }

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