namespace WhMgr.Services.Subscriptions.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using System.Text.Json.Serialization;

    using InvasionCharacter = POGOProtos.Rpc.EnumWrapper.Types.InvasionCharacter;

    [Table("invasions")]
    public class InvasionSubscription : SubscriptionItem
    {
        [
            Column("subscription_id"),
            //ForeignKey(typeof(Subscription)),
            ForeignKey(""),
        ]
        public int SubscriptionId { get; set; }

        [
            JsonPropertyName("pokestop_name"),
            Column("pokestop_name"),
        ]
        public string PokestopName { get; set; }

        [
            JsonPropertyName("grunt_type"),
            Column("grunt_type"),
        ]
        public InvasionCharacter InvasionType { get; set; }

        [
            JsonIgnore,
            NotMapped,
        ]
        public List<uint> RewardPokemonId => RewardPokemonIdString?.Split(',')?
                                                                   .Select(x => uint.Parse(x))
                                                                   .ToList();

        [
            JsonPropertyName("reward_pokemon_id"),
            Column("reward_pokemon_id"),
        ]
        public string RewardPokemonIdString { get; set; }

        [
            JsonPropertyName("city"),
            Column("city"),
        ]
        public List<string> Areas { get; set; } = new();

        [
            JsonPropertyName("location"),
            Column("location"),
        ]
        public string Location { get; set; }
    }
}