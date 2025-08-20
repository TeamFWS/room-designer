using System;
using System.Threading.Tasks;
using Meta.XR.MRUtilityKit;
using SaveSystem;
using UnityEngine;
using UnityEngine.XR;

public class RaySpawner : MonoBehaviour
{
    public GameObject furnitureParentPrefab;
    private GameObject _currentPreview;
    private bool _isPreviewVisible;
    private bool _isSpawning;
    private RoomLayoutManager _layoutManager;
    private LineRenderer _lineRenderer;
    private MRUK _mruk;
    private GameObject _objectToSpawn;
    private Transform _origin;
    private float _rotationAngle;

    private void Start()
    {
        _origin = GameObject.FindWithTag("RightHandPointer").GetComponent<Transform>();
        _lineRenderer = gameObject.AddComponent<LineRenderer>();
        _lineRenderer.positionCount = 2;
        _lineRenderer.startWidth = 0.02f;
        _lineRenderer.endWidth = 0.02f;
        _lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        _lineRenderer.startColor = Color.white;
        _lineRenderer.endColor = Color.clear;
        _lineRenderer.enabled = false;

        _mruk = FindFirstObjectByType<MRUK>();
        _layoutManager = FindFirstObjectByType<RoomLayoutManager>();
    }

    private void Update()
    {
        UpdateRayVisual();

        if (!_objectToSpawn) return;
        HandleRaycast();

        if (InputDevices.GetDeviceAtXRNode(XRNode.RightHand).TryGetFeatureValue(CommonUsages.trigger, out var trigger))
        {
            if (trigger >= 0.5f && !_isSpawning)
                SpawnObject();
            else if (trigger < 0.5f && _isSpawning) _isSpawning = false;
        }
    }

    public void SetObjectToSpawn(GameObject newObject)
    {
        if (_currentPreview) Destroy(_currentPreview);
        if (!newObject) return;
        _objectToSpawn = newObject;
        _currentPreview = Instantiate(_objectToSpawn);
        _currentPreview.transform.localScale = Vector3.one;
        _isPreviewVisible = true;
    }

    private void HandleRaycast()
    {
        if (Physics.Raycast(_origin.position, _origin.forward, out var hit))
        {
            if (hit.collider.CompareTag("Floor"))
            {
                if (_currentPreview)
                {
                    _currentPreview.transform.position = hit.point;
                    SetPreviewObjectVisual(true);
                }
            }
            else if (hit.collider.CompareTag("Furniture"))
            {
                hit.collider.GetComponent<FurnitureManipulator>().OpenMenu();
            }
            else
            {
                SetPreviewObjectVisual(false);
            }
        }
        else
        {
            SetPreviewObjectVisual(false);
        }
    }

    private async Task SpawnObject()
    {
        if (!_objectToSpawn) return;
        _isSpawning = true;

        if (_currentPreview && _currentPreview.activeSelf)
        {
            var parent = Instantiate(furnitureParentPrefab);
            var spawned = Instantiate(_objectToSpawn, _currentPreview.transform.position, _currentPreview.transform.rotation, parent.transform);
            spawned.transform.localScale = Vector3.one;
            parent.GetComponent<FurnitureManipulator>().furnitureModel = spawned;
            parent.GetComponent<FurnitureManipulator>().Rename(_objectToSpawn.name);

            var anchor = spawned.AddComponent<OVRSpatialAnchor>();

            try
            {
                await anchor.WhenLocalizedAsync();
                _layoutManager.RegisterFurniture(new AnchoredFurnitureData
                {
                    modelId = _objectToSpawn.name,
                    spatialAnchorId = anchor.Uuid.ToString(),
                    relativeRotation = spawned.transform.localRotation,
                    scale = spawned.transform.localScale
                });
                Debug.Log($"Furniture anchor OK -- {spawned.transform.position}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to create anchor: {e.Message}");
                Destroy(spawned);
                _isSpawning = false;
                return;
            }

            _objectToSpawn = null;
        }
    }

    private void SetPreviewObjectVisual(bool isVisible)
    {
        if (_currentPreview && _isPreviewVisible != isVisible)
        {
            _isPreviewVisible = isVisible;
            var renderers = _currentPreview.GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers) renderer.enabled = isVisible;
        }
    }

    private void UpdateRayVisual()
    {
        if (_objectToSpawn)
        {
            _lineRenderer.enabled = true;
            if (Physics.Raycast(_origin.position, _origin.forward, out var hit))
            {
                _lineRenderer.SetPosition(0, _origin.position);
                _lineRenderer.SetPosition(1, hit.point);
            }
            else
            {
                _lineRenderer.SetPosition(0, _origin.position);
                _lineRenderer.SetPosition(1, _origin.position + _origin.forward * 10f);
            }
        }
        else
        {
            _lineRenderer.enabled = false;
        }
    }
}