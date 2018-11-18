namespace WhMgr.Net
{
    using System;

    using WhMgr.Net.Models;

    public sealed class RaidDataEventArgs : EventArgs
    {
        public RaidData Raid { get; }

        public RaidDataEventArgs(RaidData raid)
        {
            Raid = raid;
        }
    }
}