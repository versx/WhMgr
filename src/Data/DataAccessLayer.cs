namespace WhMgr.Data
{
    using System.Collections.Generic;

    using ServiceStack.OrmLite;

    public static class DataAccessLayer
    {
        public static Dictionary<string, OrmLiteConnectionFactory> Factories { get; } = new Dictionary<string, OrmLiteConnectionFactory>();

        public static string ConnectionString { get; set; }

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
                Factories.Add(connectionString, new OrmLiteConnectionFactory(connectionString, MySqlDialect.Provider));
            }

            //var factory = new OrmLiteConnectionFactory(connectionString, MySqlDialect.Provider);
            //return factory.Open();
            return Factories[connectionString];
        }
    }
}