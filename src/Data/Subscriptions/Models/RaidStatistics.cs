namespace WhMgr.Data.Subscriptions.Models
{
    using System;

    using ServiceStack.DataAnnotations;

    [Alias("raid_stats")]
    public class RaidStatistics
    {
        [Alias("id"), PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Alias("userId"), ForeignKey(typeof(SubscriptionObject))]
        public ulong UserId { get; set; }

        [Alias("date")]
        public DateTime Date { get; set; }

        [Alias("pokemon_id")]
        public uint PokemonId { get; set; }

        [Alias("lat")]
        public double Latitude { get; set; }

        [Alias("lon")]
        public double Longitude { get; set; }
    }
}