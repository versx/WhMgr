namespace WhMgr.Alarms
{
    using System.Collections.Generic;

    using Newtonsoft.Json;

    using WhMgr.Alarms.Models;

    public class AlarmList
    {
        [JsonProperty("enablePokemon")]
        public bool EnablePokemon { get; set; }

        [JsonProperty("enableRaids")]
        public bool EnableRaids { get; set; }

        [JsonProperty("enableQuests")]
        public bool EnableQuests { get; set; }

        [JsonProperty("enablePokestops")]
        public bool EnablePokestops { get; set; }

        [JsonProperty("enableGyms")]
        public bool EnableGyms { get; set; }

        [JsonProperty("alarms")]
        public List<AlarmObject> Alarms { get; set; }

        public AlarmList()
        {
            Alarms = new List<AlarmObject>();
        }
    }
}