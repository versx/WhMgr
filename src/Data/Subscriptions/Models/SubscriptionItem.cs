namespace WhMgr.Data.Subscriptions.Models
{
    using Newtonsoft.Json;

    using ServiceStack.DataAnnotations;
    using ServiceStack.OrmLite;

    using WhMgr.Data.Subscriptions.Interfaces;
    using WhMgr.Diagnostics;

    public abstract class SubscriptionItem<T> : ISubscriptionItem
    {
        private static readonly IEventLogger _logger = EventLogger.GetLogger("SUBITEM");

        #region Properties

        [
            JsonIgnore,//JsonProperty("id"),
            Alias("id"),
            PrimaryKey,
            AutoIncrement
        ]
        public int Id { get; set; }

        [
             JsonProperty("guild_id"),
             Alias("guild_id"),
             Required
         ]
        public virtual ulong GuildId { get; set; }

        [
            JsonProperty("user_id"),
            Alias("userId"),
            Required
        ]
        public virtual ulong UserId { get; set; }

        #endregion

        #region Methods

        public virtual bool Remove()
        {
            _logger.Trace($"PokemonSubscription::Remove [GuildId={GuildId}, UserId={UserId}]");

            using (var conn = DataAccessLayer.CreateFactory().Open())
            {
                var result = conn.DeleteById<T>(Id);
                return result == 1;
            }
        }

        public virtual bool Save()
        {
            _logger.Trace($"$PokemonSubscription::Update [GuildId={GuildId}, UserId={UserId}]");

            using (var conn = DataAccessLayer.CreateFactory().Open())
            {
                var inserted = conn.Save(this, true);
                return inserted;
            }
        }

        public virtual bool Update()
        {
            _logger.Trace($"$PokemonSubscription::Update [GuildId={GuildId}, UserId={UserId}]");

            using (var conn = DataAccessLayer.CreateFactory().Open())
            {
                var result = conn.Update<T>(this);
                return result == 1;
            }
        }

        #endregion
    }
}