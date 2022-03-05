namespace WhMgr.Extensions
{
    using System;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Http;

    public static class HttpRequestExtensions
    {
        /// <summary>
        /// Retrieve the raw body as a string and deserialize as type from the Request.Body stream
        /// </summary>
        /// <typeparam name="T">Serialized type</typeparam>
        /// <param name="request">Request instance to apply to</param>
        /// <param name="encoding">Optional - Encoding, defaults to UTF8</param>
        /// <returns></returns>
        public static async Task<T> GetRawBodyAsync<T>(this HttpRequest request, Encoding encoding = null)
        {
            var json = await GetRawBodyStringAsync(request, encoding);
            if (string.IsNullOrEmpty(json))
            {
                return default;
            }
            var obj = json.FromJson<T>();
            return obj;
        }

        /// <summary>
        /// Retrieve the raw body as a string from the Request.Body stream
        /// </summary>
        /// <param name="request">Request instance to apply to</param>
        /// <param name="encoding">Optional - Encoding, defaults to UTF8</param>
        /// <returns></returns>
        public static async Task<string> GetRawBodyStringAsync(this HttpRequest request, Encoding encoding = null)
        {
            if (encoding == null)
                encoding = Encoding.UTF8;

            using var reader = new StreamReader(request.Body, encoding);
            return await reader.ReadToEndAsync();
        }

        /// <summary>
        /// Retrieves the raw body as a byte array from the Request.Body stream
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static async Task<byte[]> GetRawBodyBytesAsync(this HttpRequest request)
        {
            using var ms = new MemoryStream(2048);
            await request.Body.CopyToAsync(ms);
            return ms.ToArray();
        }
    }
}