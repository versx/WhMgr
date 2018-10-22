namespace WhMgr.Net
{
    using System;

    using WhMgr.Net.Models;

    public class PokemonDataEventArgs : EventArgs
    {
        public PokemonData Pokemon { get; }

        public PokemonDataEventArgs(PokemonData pokemon)
        {
            Pokemon = pokemon;
        }
    }
}