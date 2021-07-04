namespace WhMgr.Data.Models
{
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("gym")]
    public class Gym
    {
        [Column("id")]
        public string Id { get; set; }

        [Column("lat")]
        public double Latitude { get; set; }

        [Column("lon")]
        public double Longitude { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("url")]
        public string Url { get; set; }
    }
}