namespace WhMgr.Data
{
    using System.Data;

    using ServiceStack.OrmLite;

    public static class DataAccessLayer
    {
        public static string ConnectionString { get; set; }

        public static IDbConnection CreateFactory()
        {
            if (string.IsNullOrEmpty(ConnectionString))
                return null;

            var factory = new OrmLiteConnectionFactory(ConnectionString, MySqlDialect.Provider);
            return factory.Open();
        }
    }
}