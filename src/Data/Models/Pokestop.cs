namespace WhMgr.Data.Models
{
    using ServiceStack.DataAnnotations;

    public class Pokestop
    {
        [Alias("id")]
        public string Id { get; set; }

        [Alias("name")]
        public string Name { get; set; }

        [Alias("url")]
        public string Url { get; set; }

        [Alias("lat")]
        public double Latitude { get; set; }

        [Alias("lon")]
        public double Longitude { get; set; }

        [Alias("lure_expire_timestamp")]
        public long LureExpireTimestamp { get; set; }

        [Alias("enabled")]
        public bool Enabled { get; set; }
    }
}