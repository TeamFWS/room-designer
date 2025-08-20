using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ikea;
using SaveSystem;
using UnityEngine;
using System.Linq;
using TMPro;
using Oculus.VoiceSDK.UX;

public class RoomLayoutManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI debugPrints;

    private readonly Dictionary<string, Queue<PendingFurniture>> _pendingSpawns = new();
    private MRAnchoredLayoutData _currentLayout = new();
    private IkeaModelLoader _modelLoader;

    private void Awake()
    {
        _modelLoader = FindFirstObjectByType<IkeaModelLoader>();
        IkeaModelLoader.OnModelLoaded += HandleModelLoaded;
    }

    private void Destroy()
    {
        if (_modelLoader != null) IkeaModelLoader.OnModelLoaded -= HandleModelLoaded;
    }

    private void HandleModelLoaded(string modelName, GameObject modelPrefab)
    {
        if (_pendingSpawns.TryGetValue(modelName, out var pendingQueue))
        {
            while (pendingQueue.Count > 0)
            {
                var pending = pendingQueue.Dequeue();
                SpawnLoadedFurniture(modelPrefab, pending.Data, pending.Anchor);
            }

            _pendingSpawns.Remove(modelName);
        }
    }

    private void SpawnLoadedFurniture(GameObject modelPrefab, AnchoredFurnitureData data, OVRSpatialAnchor anchor)
    {
        var instance = Instantiate(modelPrefab, anchor.transform);
        instance.transform.localRotation = data.relativeRotation;
        instance.transform.localScale = data.scale;
        instance.SetActive(true);
    }

    public void RegisterFurniture(AnchoredFurnitureData furnitureData)
    {
        _currentLayout.furniture.Add(furnitureData);
    }

    public void QueueFurnitureSpawn(string modelName, AnchoredFurnitureData data, OVRSpatialAnchor anchor)
    {
        if (!_pendingSpawns.ContainsKey(modelName)) _pendingSpawns[modelName] = new Queue<PendingFurniture>();

        _pendingSpawns[modelName].Enqueue(new PendingFurniture
        {
            Data = data,
            Anchor = anchor
        });
    }

    public void SaveCurrentLayoutButton(string filename)
    {
        _ = SaveCurrentLayout(filename);
    }

    public void LoadLayoutButton(string filename)
    {
        if (filename == null)
        {
            Debug.Log($"Couldn't find save data: {filename}");
            debugPrints.text = "Couldn't find save data: " + filename;
        }
        else
        {
            _ = LoadLayout(filename);
        }
    }

    public async Task SaveCurrentLayout(string filename = "DefaultFilename")
    {
        try
        {
            await SaveDataSerializer.SaveLayout(_currentLayout, filename);
            Debug.Log($"Layout saved successfully as {filename}");
            debugPrints.text = "Layout saved successfully as: " + filename;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save layout: {e.Message}");
            debugPrints.text = "Failed to save because of: " + e.Message;
        }
    }

    public async Task LoadLayout(string filename)
    {
        try
        {
            var loadedLayout = await SaveDataSerializer.LoadLayout(filename);
            if (loadedLayout == null) return;

            _currentLayout = loadedLayout;

            // Prepare reusable buffer
            List<OVRSpatialAnchor.UnboundAnchor> unboundAnchors = new();

            foreach (var furnitureData in loadedLayout.furniture)
            {
                if (!System.Guid.TryParse(furnitureData.spatialAnchorId, out var anchorUuid))
                {
                    Debug.LogWarning($"Invalid anchor UUID: {furnitureData.spatialAnchorId}");
                    continue;
                }

                // Step One: Load anchor by UUID
                var result = await OVRSpatialAnchor.LoadUnboundAnchorsAsync(new[] { anchorUuid }, unboundAnchors);

                if (!result.Success || unboundAnchors.Count == 0)
                {
                    Debug.LogWarning($"Failed to load anchor {furnitureData.spatialAnchorId}: Status -> {result.Status}");
                    continue;
                }

                var unbound = unboundAnchors.FirstOrDefault(a => a.Uuid == anchorUuid);
                if (unbound.Equals(default(OVRSpatialAnchor.UnboundAnchor)))
                {
                    Debug.LogWarning($"Anchor {anchorUuid} was loaded but missing from collection.");
                    continue;
                }

                // Step Two: Localize (async)
                bool localized = await unbound.LocalizeAsync();
                if (!localized)
                {
                    Debug.LogWarning($"Localization failed for anchor {anchorUuid}");
                    continue;
                }

                // Step Three: Create GameObject & bind
                var anchorGO = new GameObject($"Anchor_{furnitureData.modelId}");
                var spatialAnchor = anchorGO.AddComponent<OVRSpatialAnchor>();

                unbound.BindTo(spatialAnchor);

                // Queue the persistent furniture for spawn
                QueueFurnitureSpawn(furnitureData.modelId, furnitureData, spatialAnchor);
                _modelLoader.LoadModelsFromUrl(furnitureData.modelId);
            }

            Debug.Log($"Layout loaded successfully from {filename}.");
            debugPrints.text = "Loaded layout from " + filename;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load layout: {e.Message}");
            debugPrints.text = "Failed to load: " + filename;
        }
    }

    public void ClearCurrentLayout()
    {
        _currentLayout = new MRAnchoredLayoutData();
        _pendingSpawns.Clear();
        var existingAnchors = FindObjectsOfType<OVRSpatialAnchor>();
        foreach (var anchor in existingAnchors) Destroy(anchor.gameObject);
    }

    public string[] GetSavedLayouts()
    {
        return SaveDataSerializer.GetSavedLayouts();
    }

    private class PendingFurniture
    {
        public OVRSpatialAnchor Anchor;
        public AnchoredFurnitureData Data;
    }
}