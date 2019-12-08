namespace WhMgr.Extensions
{
    using ServiceStack.OrmLite;

    using WhMgr.Data;

    public static class SqlExtensions
    {
        public static bool Remove<T>(this int id)
        {
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

        public static bool Update<T>(this T obj)
        {
            using (var conn = DataAccessLayer.CreateFactory().Open())
            {
                var result = conn.Update(obj);
                return result == 1;
            }
        }
    }
}