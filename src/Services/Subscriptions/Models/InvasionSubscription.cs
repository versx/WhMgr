
namespace WhMgr.Services.Subscriptions.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Text.Json.Serialization;

    using InvasionCharacter = POGOProtos.Rpc.EnumWrapper.Types.InvasionCharacter;

    [Table("invasions")]
    public class InvasionSubscription : BaseSubscription
    {
        [
            JsonPropertyName("subscription_id"),
            Column("subscription_id"),
            //ForeignKey(typeof(Subscription)),
            ForeignKey("subscription_id"),
        ]
        public int SubscriptionId { get; set; }

        [JsonIgnore]
        public Subscription Subscription { get; set; }

        [
            JsonPropertyName("pokestop_name"),
            Column("pokestop_name"),
        ]
        public string PokestopName { get; set; }

        [
            JsonPropertyName("grunt_type"),
            Column("grunt_type"),
        ]
        public List<InvasionCharacter> InvasionType { get; set; } = new();

        [
            JsonPropertyName("reward_pokemon_id"),
            Column("reward_pokemon_id"),
        ]
        public List<uint> RewardPokemonId { get; set; } = new();

        [
            JsonPropertyName("areas"),
            Column("areas"),
        ]
        public List<string> Areas { get; set; } = new();

        [
            JsonPropertyName("location"),
            Column("location"),
        ]
        public string Location { get; set; }
    }
}