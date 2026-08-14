using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;

namespace DataAccessLayer.UpdateCheck
{
    public class GitHubReleaseClient : IUpdateSource
    {
        private readonly HttpClient httpClient;
        private readonly string repositoryApiUrl;

        public GitHubReleaseClient(HttpClient httpClient, string owner, string repository)
        {
            this.httpClient = httpClient;
            repositoryApiUrl = $"https://api.github.com/repos/{owner}/{repository}/releases/latest";

            if(!this.httpClient.DefaultRequestHeaders.UserAgent.Any())
            {
                this.httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("PassNest", null));
            }
        }
        public async Task<GitHubReleaseInfo?> GetLatestReleaseAsync()
        {
            try
            {
                using var response = await httpClient.GetAsync(repositoryApiUrl);
                if (!response.IsSuccessStatusCode) return null;
                
                var json = await response.Content.ReadAsStringAsync();

                return JsonSerializer.Deserialize<GitHubReleaseInfo>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch
            {
                return null;
            }
        }
    }
}
