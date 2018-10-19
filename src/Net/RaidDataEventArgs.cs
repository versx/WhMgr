namespace T.Net
{
    using System;

    using T.Net.Models;

    public class RaidDataEventArgs : EventArgs
    {
        public RaidData Raid { get; }

        public RaidDataEventArgs(RaidData raid)
        {
            Raid = raid;
        }
    }
}