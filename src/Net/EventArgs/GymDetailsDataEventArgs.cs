namespace WhMgr.Net
{
    using System;

    using WhMgr.Net.Models;

    public sealed class GymDetailsDataEventArgs : EventArgs
    {
        public GymDetailsData GymDetails { get; }

        public GymDetailsDataEventArgs(GymDetailsData gymDetails)
        {
            GymDetails = gymDetails;
        }
    }
}