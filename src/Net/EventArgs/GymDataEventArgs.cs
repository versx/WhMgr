namespace WhMgr.Net
{
    using System;

    using WhMgr.Net.Models;

    public sealed class GymDataEventArgs : EventArgs
    {
        public GymData Gym { get; }

        public GymDataEventArgs(GymData gym)
        {
            Gym = gym;
        }
    }
}