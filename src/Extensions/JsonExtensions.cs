namespace WhMgr.Extensions
{
    using System;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    public static class JsonExtensions
    {
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            AllowTrailingCommas = true,
            WriteIndented = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
        };

        public static T FromJson<T>(this string json) =>
            JsonSerializer.Deserialize<T>(json, _jsonOptions);

        public static string ToJson<T>(this T obj) =>
            JsonSerializer.Serialize(obj, _jsonOptions);
    }
}