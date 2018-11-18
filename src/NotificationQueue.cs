namespace WhMgr
{
    using System;
    using System.Collections.Generic;

    using DSharpPlus.Entities;

    public sealed class NotificationQueue : Queue<Tuple<DiscordUser, string, DiscordEmbed>>
    {
    }
}