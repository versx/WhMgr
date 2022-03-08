namespace WhMgr.Services.Subscriptions.Models
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Base Pokemon subscription class
    /// </summary>
    public abstract class BasePokemonSubscription : BaseSubscription
    {
        [
            JsonPropertyName("pokemon_id"),
            Column("pokemon_id"),
            Required,
        ]
        public List<uint> PokemonId { get; set; } = new();

        [
            JsonPropertyName("forms"),
            Column("forms"),
        ]
        public List<string> Forms { get; set; } = new();

        /*
        [
            JsonPropertyName("costumes"),
            Column("costumes"),
        ]
        public List<string> Costumes { get; set; } = new();
        */

        public BasePokemonSubscription()
        {
            PokemonId = new List<uint>();
            Forms = new List<string>();
            //Costumes = new List<string>();
        }
    }
}
