namespace WhMgr.Data.Models
{
    using System.ComponentModel.DataAnnotations.Schema;

    using WeatherCondition = POGOProtos.Rpc.GameplayWeatherProto.Types.WeatherCondition;

    [Table("weather")]
    public class Weather
    {
        [Column("id")]
        public long Id { get; set; }

        [Column("level")]
        public ushort Level { get; set; }

        [Column("latitude")]
        public double Latitude { get; set; }

        [Column("longitude")]
        public double Longitude { get; set; }

        [Column("gameplay_condition")]
        public WeatherCondition GameplayCondition { get; set; }

        [Column("updated")]
        public ulong Updated { get; set; }

        //wind_direction
        //cloud_level
        //raid_level
        //wind_level
        //snow_level
        //fog_level
        //special_effect_level
        //severity
        //warn_weather
        //updated
    }
}