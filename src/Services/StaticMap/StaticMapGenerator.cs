namespace WhMgr.Services.StaticMap
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    
    using WhMgr.Extensions;

    public class StaticMapGenerator : IStaticMapGenerator
    {
        private readonly StaticMapOptions _options;

        public StaticMapGenerator(StaticMapOptions options)
        {
            _options = options;
        }

        public string GenerateLink()
        {
            var payload = new
            {
                url = _options.BaseUrl,
                lat = _options.Latitude,
                lon = _options.Longitude,
                latitude = _options.Latitude,
                longitude = _options.Longitude,
                pokestops = _options.Pokestops,
                gyms = _options.Gyms,
                url2 = _options.SecondaryImageUrl,
                team_id = _options.Team,
                regeneratable = _options.Regeneratable,
                pregenerated = _options.Pregenerate,
                path = _options.PolygonPath,
            };
            var payloadJson = payload.ToJson();
            var url = BuildUrl();
            var result = SendRequest(url, payloadJson);
            var responseUrl = $"{_options.BaseUrl}/{_options.TemplateType.ToString().ToLower()}/pregenerated/{result}";
            return responseUrl;
        }

        private string BuildUrl()
        {
            var sb = new StringBuilder();
            sb.Append(_options.BaseUrl);
            sb.Append('/');
            sb.Append(_options.TemplateType.ToString().ToLower());
            sb.Append('/');
            sb.Append(_options.MapType.ToString().ToLower());
            sb.Append('?');
            sb.Append($"pregenerate={_options.Pregenerate.ToString().ToLower()}");
            sb.Append('&');
            sb.Append($"regeneratable={_options.Regeneratable.ToString().ToLower()}");
            sb.Append('&');
            sb.Append($"lat={_options.Latitude}");
            sb.Append('&');
            sb.Append($"lon={_options.Longitude}");
            var url = sb.ToString();
            return url;
        }

        private static string SendRequest(string url, string payload)
        {
            try
            {
                using var client = new HttpClient();
                var mime = "application/json";
                var requestMessage = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri(url),
                    Headers =
                    {
                        { HttpRequestHeader.Accept.ToString(), mime },
                        { HttpRequestHeader.ContentType.ToString(), mime },
                    },
                    Content = new StringContent(payload, Encoding.UTF8, mime),
                };
                var response = client.SendAsync(requestMessage).Result;
                var responseData = response.Content.ReadAsStringAsync().Result;
                return responseData;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex}");
            }
            return null;
        }
    }
}