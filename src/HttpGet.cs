using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Resources;
using System.Threading;
using System.Threading.Tasks;

namespace DSPi
{
    public class HttpGet
    {
        private static HttpClient client = new HttpClient();

        public static bool IsNetworkConnected()
        {
            try
            {
                Ping ping = new Ping();
                PingReply reply = ping.Send("git.lnvpe.com");

                if (reply.Status == IPStatus.Success)
                {
                    return true;
                }
            }
            
            return false;
        }

        private const string AccessToken = token.accesstoken;

        public static async Task<List<Repository>> GetRepositoriesAsync(string apiUrl)
        {
            var apiUrlWithToken = $"{apiUrl}?access_token={AccessToken}";

            var response = await client.GetAsync(apiUrlWithToken);
            response.EnsureSuccessStatusCode();
            var RepositoriesJson = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<Repository>>(RepositoriesJson);
        }

        public static async Task<List<Release>> GetReleasesAsync(string releasesurl)
        {
            var releasesurlWithToken = $"{releasesurl}?access_token={AccessToken}";

            var response = await client.GetAsync(releasesurlWithToken);
            response.EnsureSuccessStatusCode();
            var releasesJson = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<Release>>(releasesJson);
        }

        public class Repository
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string full_name { get; set; }
            public string description { get; set; }
            // Add other properties as needed
        }
        public class Release
        {
            public string name { get; set; }
            public string tag_name { get; set; }
            public string body { get; set; }
            public List<Asset> assets { get; set; }
            public AuthorInfo author { get; set; }
        }

        public class Asset
        {
            public string name { get; set; }
            public int size { get; set; }
            public string browser_download_url { get; set; }
        }

        public class AuthorInfo
        {
            public string username { get; set; }
            public string email { get; set; }
        }

    }
}
