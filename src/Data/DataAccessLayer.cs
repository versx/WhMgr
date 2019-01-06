namespace WhMgr.Data
{
    using System.Data;

    using ServiceStack.OrmLite;

    public static class DataAccessLayer
    {
        public static string ConnectionString { get; set; }

        public static IDbConnection CreateFactory()
        {
            return CreateFactory(ConnectionString);
        }

        public static IDbConnection CreateFactory(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
                return null;

            var factory = new OrmLiteConnectionFactory(connectionString, MySqlDialect.Provider);
            return factory.Open();
        }
    }
}