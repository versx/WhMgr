namespace WhMgr.Services.Icons.Models
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    public class BaseIndexRaidManifest
    {
        [JsonPropertyName("egg")]
        public HashSet<string> Eggs { get; set; }
    }
}