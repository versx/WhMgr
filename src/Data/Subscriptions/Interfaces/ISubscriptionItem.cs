namespace WhMgr.Data.Subscriptions.Interfaces
{
    public interface ISubscriptionItem
    {
        int Id { get; }
        ulong GuildId { get; }
        ulong UserId { get; }

        bool Save();
        bool Update();
        bool Remove();
    }
}