using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using GLTFast;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace Ikea
{
    public class IkeaModelLoader : MonoBehaviour
    {
        private const int MaxProducts = 10;
        private const string CacheIndexFileName = "ikeaModelCacheIndex.json";
        private static readonly Dictionary<string, List<string>> UrlToProductsMap = new();
        private static readonly Dictionary<string, GameObject> ModelDictionary = new();
        private static readonly Dictionary<string, bool> CacheStatus = new();
        private IkeaModelSpawner _spawner;
        private IkeaProductUrlFetcher _urlFetcher;

        private void Awake()
        {
            _urlFetcher = gameObject.AddComponent<IkeaProductUrlFetcher>();
            _spawner = FindFirstObjectByType<IkeaModelSpawner>();
            LoadCacheIndex();
        }

        public async void LoadModelsFromUrl(string url)
        {
            if (CacheStatus.TryGetValue(url, out var isCached) && isCached)
            {
                foreach (var product in UrlToProductsMap[url])
                {
                    _spawner.AddFetchingPlaceholder(product);
                    if (ModelDictionary.TryGetValue(product, out var model))
                        OnModelLoaded?.Invoke(product, model);
                    else
                        StartCoroutine(LoadModelFromCache(product));
                }

                return;
            }

            CacheStatus[url] = false;
            UrlToProductsMap[url] = new List<string>();

            await _urlFetcher.FetchProductUrlsWith3DModels(MaxProducts, url,
                productUrl => { StartCoroutine(ProcessProduct(productUrl, url)); });
        }

        public static event Action<string, GameObject> OnModelLoaded;

        private IEnumerator ProcessProduct(string ikeaUrl, string sourceUrl)
        {
            var pageRequest = UnityWebRequest.Get(ikeaUrl);
            yield return pageRequest.SendWebRequest();

            if (pageRequest.result != UnityWebRequest.Result.Success)
                yield break;

            var html = pageRequest.downloadHandler.text;
            var jsonLdStrings = ExtractJsonLd(html);

            string modelUrl = null;
            var productName = "Unknown_Product";

            foreach (var jsonString in jsonLdStrings)
            {
                JObject jsonData;
                try
                {
                    jsonData = JObject.Parse(jsonString);
                }
                catch
                {
                    continue;
                }

                if (jsonData["@type"]?.ToString() == "3DModel")
                {
                    productName = jsonData["name"]?.ToString() ?? "Unknown_Product";
                    productName = Regex.Replace(productName, "[\\/*?:\"<>|]", "_");

                    if (jsonData["encoding"] is JArray encodingArray)
                        foreach (var encoding in encodingArray)
                            if (encoding["@type"]?.ToString() == "MediaObject" &&
                                encoding["encodingFormat"]?.ToString() == "model/gltf-binary")
                            {
                                modelUrl = encoding["contentUrl"]?.ToString();
                                break;
                            }

                    if (!string.IsNullOrEmpty(modelUrl))
                        break;
                }
            }

            if (string.IsNullOrEmpty(modelUrl))
                yield break;

            UrlToProductsMap[sourceUrl].Add(productName);
            _spawner.AddFetchingPlaceholder(productName);
            yield return LoadModel(modelUrl, productName);

            CacheStatus[sourceUrl] = true;
            SaveCacheIndex();
        }

        private IEnumerator LoadModel(string modelUrl, string productName)
        {
            if (ModelDictionary.ContainsKey(productName))
            {
                OnModelLoaded?.Invoke(productName, ModelDictionary[productName]);
                yield break;
            }

            var tempPath = Path.Combine(Application.temporaryCachePath, productName + ".glb");
            if (!File.Exists(tempPath))
            {
                var modelRequest = UnityWebRequest.Get(modelUrl);
                yield return modelRequest.SendWebRequest();
                if (modelRequest.result != UnityWebRequest.Result.Success)
                    yield break;
                File.WriteAllBytes(tempPath, modelRequest.downloadHandler.data);
            }

            var gltf = new GltfImport();
            var loadTask = gltf.Load(tempPath);
            while (!loadTask.IsCompleted) yield return null;

            if (loadTask.Result)
            {
                var model = new GameObject(productName);
                var instTask = gltf.InstantiateMainSceneAsync(model.transform);
                while (!instTask.IsCompleted) yield return null;

                if (instTask.Result)
                {
                    model.transform.localRotation = Quaternion.identity;
                    model.SetActive(false);
                    ModelDictionary[productName] = model;
                    OnModelLoaded?.Invoke(productName, model);
                }
            }
        }

        private IEnumerator LoadModelFromCache(string productName)
        {
            var tempPath = Path.Combine(Application.temporaryCachePath, productName + ".glb");
            if (!File.Exists(tempPath))
                yield break;

            var gltf = new GltfImport();
            var loadTask = gltf.Load(tempPath);
            while (!loadTask.IsCompleted) yield return null;

            if (loadTask.Result)
            {
                var model = new GameObject(productName);
                var instTask = gltf.InstantiateMainSceneAsync(model.transform);
                while (!instTask.IsCompleted) yield return null;

                if (instTask.Result)
                {
                    model.transform.localRotation = Quaternion.identity;
                    model.SetActive(false);
                    ModelDictionary[productName] = model;
                    OnModelLoaded?.Invoke(productName, model);
                }
            }
        }

        public void ClearCache()
        {
            var dir = Application.temporaryCachePath;
            foreach (var file in Directory.GetFiles(dir, "*.glb"))
                File.Delete(file);
            var idx = Path.Combine(dir, CacheIndexFileName);
            if (File.Exists(idx)) File.Delete(idx);
            UrlToProductsMap.Clear();
            CacheStatus.Clear();
            ModelDictionary.Clear();
        }

        private void LoadCacheIndex()
        {
            var file = Path.Combine(Application.temporaryCachePath, CacheIndexFileName);
            if (!File.Exists(file)) return;
            try
            {
                var root = JObject.Parse(File.ReadAllText(file));
                UrlToProductsMap.Clear();
                CacheStatus.Clear();
                foreach (var p in (JObject)root["cacheStatus"])
                    CacheStatus[p.Key] = p.Value.Value<bool>();
                foreach (var p in (JObject)root["urlToProducts"])
                    UrlToProductsMap[p.Key] = p.Value.ToObject<List<string>>();
            }
            catch
            {
            }
        }

        private void SaveCacheIndex()
        {
            var root = new JObject
            {
                ["cacheStatus"] = JObject.FromObject(CacheStatus),
                ["urlToProducts"] = JObject.FromObject(UrlToProductsMap)
            };
            File.WriteAllText(
                Path.Combine(Application.temporaryCachePath, CacheIndexFileName),
                root.ToString()
            );
        }

        private static List<string> ExtractJsonLd(string html)
        {
            var list = new List<string>();
            var matches = Regex.Matches(html, "<script type=\"application/ld\\+json\">(.*?)</script>",
                RegexOptions.Singleline);
            foreach (Match m in matches)
                list.Add(m.Groups[1].Value);
            return list;
        }
    }
}