using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Ikea
{
    public class IkeaProductUrlFetcher : MonoBehaviour
    {
        private static readonly HttpClient HttpClient = new();
        private readonly ConcurrentDictionary<string, bool> _seenProducts = new();

        public async Task FetchProductUrlsWith3DModels(int maxProducts, string baseUrl, Action<string> onProductFound)
        {
            var productUrls = new HashSet<string>();
            var currentPage = 1;

            try
            {
                while (productUrls.Count < maxProducts)
                {
                    var searchPageUrl = AppendPageParameter(baseUrl, currentPage);
                    var searchPageHtml = await GetHtmlAsync(searchPageUrl);
                    var productPageUrls = ExtractProductPageUrls(searchPageHtml);

                    if (productPageUrls.Count == 0) break;

                    var tasks = productPageUrls.Select(async productPageUrl =>
                    {
                        if (productUrls.Count >= maxProducts) return;

                        var productPageHtml = await GetHtmlAsync(productPageUrl);
                        var productName = GetName(productPageHtml);

                        if (!string.IsNullOrEmpty(productName) && Contains3DModel(productPageHtml))
                            if (_seenProducts.TryAdd(productName, true))
                            {
                                productUrls.Add(productPageUrl);
                                onProductFound?.Invoke(productPageUrl);
                            }
                    }).ToList();

                    await Task.WhenAll(tasks);
                    currentPage++;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error fetching product URLs: {ex.Message}");
            }
        }

        private string AppendPageParameter(string url, int page)
        {
            var uri = new Uri(url);
            var query = uri.Query;
            return string.IsNullOrEmpty(query) ? $"{url}?page={page}" : $"{url}&page={page}";
        }

        private async Task<string> GetHtmlAsync(string url)
        {
            try
            {
                return await HttpClient.GetStringAsync(url);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error fetching {url}: {ex.Message}");
                return string.Empty;
            }
        }

        private List<string> ExtractProductPageUrls(string html)
        {
            var productUrls = new List<string>();
            var regex = new Regex(@"\""url\"":\s*\""(?<url>https:\/\/www\.ikea\.com\/pl\/pl\/p\/[^\""]+)\""",
                RegexOptions.IgnoreCase);
            var matches = regex.Matches(html);

            foreach (Match match in matches) productUrls.Add(match.Groups["url"].Value);

            return productUrls;
        }

        private bool Contains3DModel(string html)
        {
            var jsonLdStrings = ExtractJsonLd(html);
            return jsonLdStrings.Any(jsonString =>
            {
                try
                {
                    var jsonData = JObject.Parse(jsonString);
                    return jsonData["@type"]?.ToString() == "3DModel";
                }
                catch
                {
                    return false;
                }
            });
        }

        private string GetName(string html)
        {
            var jsonLdStrings = ExtractJsonLd(html);
            foreach (var jsonString in jsonLdStrings)
                try
                {
                    var jsonData = JObject.Parse(jsonString);
                    var productName = jsonData["name"]?.ToString();
                    if (!string.IsNullOrEmpty(productName)) return productName;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error parsing JSON-LD: {ex.Message}");
                }

            return ExtractNameFromHtml(html);
        }

        private string ExtractNameFromHtml(string html)
        {
            var regex = new Regex(@"<h1.*?class=""product-title"".*?>(.*?)</h1>", RegexOptions.IgnoreCase);
            var match = regex.Match(html);
            return match.Success ? match.Groups[1].Value.Trim() : "Unknown_Product";
        }

        private List<string> ExtractJsonLd(string html)
        {
            var jsonLdList = new List<string>();
            var regex = new Regex(@"<script type=""application/ld\+json"">(.*?)</script>", RegexOptions.Singleline);
            var matches = regex.Matches(html);

            foreach (Match match in matches) jsonLdList.Add(match.Groups[1].Value);

            return jsonLdList;
        }
    }
}