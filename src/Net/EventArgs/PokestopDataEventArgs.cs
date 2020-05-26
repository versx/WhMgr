namespace WhMgr.Net
{
    using System;

    using WhMgr.Net.Models;

    public sealed class PokestopDataEventArgs : EventArgs
    {
        public PokestopData Pokestop { get; }

        public PokestopDataEventArgs(PokestopData pokestop)
        {
            Pokestop = pokestop;
        }
    }
}