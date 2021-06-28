namespace WhMgr.Services.Subscriptions.Models
{
    using System;

    [Flags]
    public enum NotificationStatusType : byte
    {
        None = 0x0,
        Pokemon = 0x1,
        PvP = 0x2,
        Raids = 0x4,
        Quests = 0x8,
        Invasions = 0x10,
        Lures = 0x20,
        Gyms = 0x40,
        All = Pokemon | PvP | Raids | Quests | Invasions | Lures | Gyms,
    }
}