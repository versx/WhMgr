namespace T.Net
{
    using System;

    using T.Net.Models;

    public class PokemonDataEventArgs : EventArgs
    {
        public PokemonData Pokemon { get; }

        public PokemonDataEventArgs(PokemonData pokemon)
        {
            Pokemon = pokemon;
        }
    }
}