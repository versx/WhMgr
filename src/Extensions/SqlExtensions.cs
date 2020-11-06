/*
namespace WhMgr.Extensions
{
    using System.Linq;

    using WhMgr.Data.Factories;

    public static class SqlExtensions
    {
        public static bool Remove<T>(this int id)
        {
            using (var ctx = DbContextFactory.CreateSubscriptionContext(" CONNECTION STRING "))
            {
                ctx.SaveChanges();
            }
            using (var conn = DataAccessLayer.CreateFactory().Open())
            {
                var result = conn.DeleteById<T>(id);
                return result == 1;
            }
        }

        public static bool Save<T>(this T obj, bool references = true)
        {
            using (var conn = DataAccessLayer.CreateFactory().Open())
            {
                var inserted = conn.Save(obj, references);
                return inserted;
            }
        }
    }
}
*/