namespace WhMgr.Alarms
{
    using System.Collections.Generic;

    using Newtonsoft.Json;

    using WhMgr.Alarms.Models;

    /// <summary>
    /// Alarm list class
    /// </summary>
    public class AlarmList
    {
        /// <summary>
        /// Global toggle for Pokemon alarms
        /// </summary>
        [JsonProperty("enablePokemon")]
        public bool EnablePokemon { get; set; }

        /// <summary>
        /// Global toggle for Raid alarms
        /// </summary>
        [JsonProperty("enableRaids")]
        public bool EnableRaids { get; set; }

        /// <summary>
        /// Global toggle for Quest alarms
        /// </summary>
        [JsonProperty("enableQuests")]
        public bool EnableQuests { get; set; }

        /// <summary>
        /// Global toggle for Pokestops
        /// </summary>
        [JsonProperty("enablePokestops")]
        public bool EnablePokestops { get; set; }

        /// <summary>
        /// Global toggle for Gyms
        /// </summary>
        [JsonProperty("enableGyms")]
        public bool EnableGyms { get; set; }

        /// <summary>
        /// Global toggle for weather
        /// </summary>
        [JsonProperty("enableWeather")]
        public bool EnableWeather { get; set; }

        /// <summary>
        /// Alarms list
        /// </summary>
        [JsonProperty("alarms")]
        public List<AlarmObject> Alarms { get; set; }

        /// <summary>
        /// Instantiate a new <see cref="AlarmList"/> class
        /// </summary>
        public AlarmList()
        {
            Alarms = new List<AlarmObject>();
        }
    }
}