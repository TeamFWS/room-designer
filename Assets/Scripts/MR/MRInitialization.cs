using System.Collections.Generic;
using System.Linq;
using Meta.XR.EnvironmentDepth;
using Meta.XR.MRUtilityKit;
using UnityEngine;

namespace MR
{
    public class MRInitialization : MonoBehaviour
    {
        [SerializeField] private GameObject passthrough;
        [SerializeField] private Camera centerEyeCamera;
        [SerializeField] private Material defaultMaterial;
        [SerializeField] private Material occlusionMaterial;
        private readonly List<GameObject> _gameObjects = new();
        private readonly List<MRUKRoom> _loadedRooms = new();
        private EnvironmentDepthManager _environmentDepthManager;
        private MRUK _mruk;

        private void Awake()
        {
            _environmentDepthManager = FindFirstObjectByType<EnvironmentDepthManager>();
            _mruk = FindFirstObjectByType<MRUK>();
            _mruk.SceneLoadedEvent.AddListener(OnSceneLoaded);
        }

        private void OnEnable()
        {
            StateManager.Instance.StateChanged += OnStateChanged;
        }

        private void OnDisable()
        {
            StateManager.Instance.StateChanged -= OnStateChanged;
        }

        private void OnDestroy()
        {
            if (_mruk != null) _mruk.SceneLoadedEvent.RemoveListener(OnSceneLoaded);
        }

        private void OnStateChanged(AppState newState)
        {
            if (newState == AppState.MRInitialization)
            {
                while (_loadedRooms.Count == 0)
                {
                }

                InitializeMR();
            }
        }

        private void InitializeMR()
        {
            foreach (var room in _mruk.Rooms) ProcessRoom(room);
            SetDepthMaskMeshFilters();
            StateManager.Instance.ChangeState(AppState.Main);
            passthrough.SetActive(true);
            centerEyeCamera.clearFlags = CameraClearFlags.SolidColor;
        }

        private void OnSceneLoaded()
        {
            foreach (var room in _mruk.Rooms) _loadedRooms.Add(room);
        }

        private void ProcessRoom(MRUKRoom room)
        {
            foreach (var anchor in room.Anchors)
                if (anchor.HasLabel(MRUKAnchor.SceneLabels.FLOOR.ToString()))
                    CreateFloorMesh(anchor);
                else if (anchor.HasLabel(MRUKAnchor.SceneLabels.CEILING.ToString()))
                    CreateCeilingMesh(anchor);
                else if (anchor.HasLabel(MRUKAnchor.SceneLabels.WALL_FACE.ToString()))
                    CreateWallCube(anchor);
                else if (!anchor.HasLabel(MRUKAnchor.SceneLabels.GLOBAL_MESH.ToString()))
                    CreateOcclusionCube(anchor);
        }

        private void CreateWallCube(MRUKAnchor anchor)
        {
            if (!anchor.PlaneRect.HasValue) return;
            var planeSize = anchor.PlaneRect.Value.size;
            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.transform.position = anchor.transform.position;
            wall.transform.rotation = anchor.transform.rotation * Quaternion.Euler(90, 0, 0);
            wall.transform.localScale = new Vector3(planeSize.x, 0.1f, planeSize.y);
            wall.transform.SetParent(anchor.transform);

            wall.tag = "Wall";
            var meshFilter = wall.GetComponent<MeshFilter>();
            AddCollider(wall, meshFilter.mesh);
            AddMaterial(wall, defaultMaterial);
            _gameObjects.Add(wall);
        }

        private void CreateFloorMesh(MRUKAnchor anchor)
        {
            if (!anchor.PlaneRect.HasValue) return;
            var floorObject = new GameObject("FloorMesh")
            {
                transform =
                {
                    position = anchor.transform.position,
                    rotation = anchor.transform.rotation,
                    localRotation = Quaternion.Euler(0f, 0f, 0f)
                },
                tag = "Floor"
            };

            var meshFilter = floorObject.AddComponent<MeshFilter>();

            var mesh = new Mesh();
            var size = anchor.PlaneRect.Value.size;

            Vector3[] vertices =
            {
                new(-size.x / 2, 0, -size.y / 2),
                new(size.x / 2, 0, -size.y / 2),
                new(-size.x / 2, 0, size.y / 2),
                new(size.x / 2, 0, size.y / 2)
            };

            int[] triangles = { 0, 2, 1, 1, 2, 3 };

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();

            meshFilter.mesh = mesh;
            AddCollider(floorObject, mesh);
            AddMaterial(floorObject, defaultMaterial);
            _gameObjects.Add(floorObject);
        }

        private void CreateCeilingMesh(MRUKAnchor anchor)
        {
            if (!anchor.PlaneRect.HasValue) return;
            var ceilingObject = new GameObject("CeilingMesh")
            {
                transform =
                {
                    position = anchor.transform.position,
                    rotation = anchor.transform.rotation,
                    localRotation = Quaternion.Euler(180f, 0f, 0f)
                },
                tag = "Ceiling"
            };

            var meshFilter = ceilingObject.AddComponent<MeshFilter>();

            var mesh = new Mesh();
            var size = anchor.PlaneRect.Value.size;

            Vector3[] vertices =
            {
                new(-size.x / 2, 0, -size.y / 2),
                new(size.x / 2, 0, -size.y / 2),
                new(-size.x / 2, 0, size.y / 2),
                new(size.x / 2, 0, size.y / 2)
            };

            int[] triangles = { 0, 2, 1, 1, 2, 3 };

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();

            meshFilter.mesh = mesh;
            AddCollider(ceilingObject, mesh);
            AddMaterial(ceilingObject, defaultMaterial);
            _gameObjects.Add(ceilingObject);
        }

        private void CreateOcclusionCube(MRUKAnchor anchor)
        {
            if (!anchor.VolumeBounds.HasValue) return;
            var bounds = anchor.VolumeBounds.Value;
            var occlusionCube = GameObject.CreatePrimitive(PrimitiveType.Cube);

            occlusionCube.transform.localScale = bounds.size;
            occlusionCube.transform.position = anchor.transform.position;
            occlusionCube.transform.rotation = anchor.transform.rotation;
            var worldUp = Vector3.up;
            var localUp = occlusionCube.transform.up;
            var verticalSize = Mathf.Abs(Vector3.Dot(bounds.size, localUp));
            occlusionCube.transform.position -= worldUp * (verticalSize / 2f);

            DestroyImmediate(occlusionCube.GetComponent<BoxCollider>());
            AddMaterial(occlusionCube, occlusionMaterial);
            _gameObjects.Add(occlusionCube);
        }

        private static void AddCollider(GameObject gameObject, Mesh mesh)
        {
            var meshCollider = gameObject.AddComponent<MeshCollider>();
            meshCollider.sharedMesh = mesh;
        }

        private void AddMaterial(GameObject mesh, Material material)
        {
            var meshRenderer = mesh.GetComponent<MeshRenderer>();
            if (!meshRenderer) meshRenderer = mesh.AddComponent<MeshRenderer>();
            meshRenderer.material = material;
        }

        private void SetDepthMaskMeshFilters()
        {
            var myMeshFilters = _gameObjects.Select(o => o.GetComponent<MeshFilter>()).ToList();
            _environmentDepthManager.MaskMeshFilters = myMeshFilters;
            _environmentDepthManager.MaskBias = 0.06f;
        }
    }
}