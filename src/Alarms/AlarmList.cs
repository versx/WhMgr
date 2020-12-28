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
        /// Gets or sets the global toggle for Pokemon alarms
        /// </summary>
        [JsonProperty("enablePokemon")]
        public bool EnablePokemon { get; set; }

        /// <summary>
        /// Gets or sets the global toggle for Raid alarms
        /// </summary>
        [JsonProperty("enableRaids")]
        public bool EnableRaids { get; set; }

        /// <summary>
        /// Gets or sets the global toggle for Quest alarms
        /// </summary>
        [JsonProperty("enableQuests")]
        public bool EnableQuests { get; set; }

        /// <summary>
        /// Gets or sets the global toggle for Pokestops
        /// </summary>
        [JsonProperty("enablePokestops")]
        public bool EnablePokestops { get; set; }

        /// <summary>
        /// Gets or sets the global toggle for Gyms
        /// </summary>
        [JsonProperty("enableGyms")]
        public bool EnableGyms { get; set; }

        /// <summary>
        /// Gets or sets the global toggle for weather
        /// </summary>
        [JsonProperty("enableWeather")]
        public bool EnableWeather { get; set; }

        /// <summary>
        /// Gets or sets the Alarms list
        /// </summary>
        [JsonProperty("alarms")]
        public List<AlarmObject> Alarms { get; set; }

        /// <summary>
        /// Instantiates a new <see cref="AlarmList"/> class
        /// </summary>
        public AlarmList()
        {
            Alarms = new List<AlarmObject>();
        }
    }
}