namespace WhMgr.Utilities
{
    using System;
    using System.Net;

    public static class NetUtils
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string Get(string url)
        {
            using (var wc = new WebClient())
            {
                wc.Proxy = null;
                try
                {
                    return wc.DownloadString(url);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to download data from {url}: {ex}");
                }
            }

            return null;
        }
    }
}