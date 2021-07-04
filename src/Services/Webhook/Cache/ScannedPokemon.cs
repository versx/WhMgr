namespace WhMgr.Services.Webhook.Cache
{
    using System;

    using WhMgr.Extensions;
    using WhMgr.Services.Webhook.Models;

    internal class ScannedPokemon : IScannedItem
    {
        public double Latitude { get; }

        public double Longitude { get; }

        public bool IsMissingStats { get; }

        public DateTime DespawnTime { get; }

        public bool IsExpired
        {
            get
            {
                var now = DateTime.UtcNow.ConvertTimeFromCoordinates(Latitude, Longitude);
                return now > DespawnTime;
            }
        }

        public ScannedPokemon(PokemonData pokemon)
        {
            Latitude = pokemon.Latitude;
            Longitude = pokemon.Longitude;
            IsMissingStats = pokemon.IsMissingStats;
            DespawnTime = pokemon.DespawnTime;
        }
    }
}