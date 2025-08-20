using UnityEngine;

namespace ManualRoom
{
    public class RoomGenerator
    {
        private const float Spacing = 1.0f;
        private readonly GridCell[,] _gridCells;
        private readonly int _gridSizeX;
        private readonly int _gridSizeY;
        private readonly Material _material;
        private readonly GameObject _roomContainer;
        private readonly float _wallHeight = 2f;

        public RoomGenerator(RoomGrid roomGrid, Material material)
        {
            _roomContainer = new GameObject("RoomContainer");
            _gridCells = roomGrid.GridCells;
            _gridSizeX = roomGrid.GridSizeX;
            _gridSizeY = roomGrid.GridSizeY;
            _material = material;
        }

        public void GenerateRoomModel()
        {
            CreateWallPlanes();
            CreateFloorPlane();
            CreateCeilingPlane();
            AdjustRoomPositionToContainPlayer();
            _roomContainer.transform.localScale = new Vector3(1f, _wallHeight, 1f);
        }

        private void CreateWallPlanes()
        {
            for (var x = 0; x < _gridSizeX; x++)
            for (var y = 0; y < _gridSizeY; y++)
            {
                var cell = _gridCells[x, y];
                if (cell.HasTopWall()) CreateWallPlane(new Vector3(x * Spacing, 0.5f, (y + 0.5f) * Spacing), true);
                if (cell.HasLeftWall()) CreateWallPlane(new Vector3((x - 0.5f) * Spacing, 0.5f, y * Spacing), false);
            }
        }

        private void CreateWallPlane(Vector3 position, bool isVertical)
        {
            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.gameObject.SetActive(true);
            wall.transform.position = position;
            wall.transform.localScale =
                isVertical ? new Vector3(Spacing, 1f, 0.001f) : new Vector3(0.001f, 1f, Spacing);
            wall.transform.rotation = Quaternion.Euler(0, 90, 0);
            wall.transform.parent = _roomContainer.transform;
            wall.tag = "Wall";
            AddMaterial(wall);
        }

        private void CreateFloorPlane()
        {
            var centerPosition = new Vector3((_gridSizeX - 1) * Spacing / 2, 0, (_gridSizeY - 1) * Spacing / 2);
            var scale = new Vector3(_gridSizeX * Spacing / 10, 1, _gridSizeY * Spacing / 10);

            var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.gameObject.SetActive(true);
            floor.transform.position = centerPosition;
            floor.transform.localScale = scale;
            floor.transform.parent = _roomContainer.transform;
            floor.tag = "Floor";
            AddMaterial(floor);
        }

        private void CreateCeilingPlane()
        {
            var centerPosition = new Vector3((_gridSizeX - 1) * Spacing / 2, 1f, (_gridSizeY - 1) * Spacing / 2);
            var scale = new Vector3(_gridSizeX * Spacing / 10, 1, _gridSizeY * Spacing / 10);

            var ceiling = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ceiling.gameObject.SetActive(true);
            ceiling.transform.position = centerPosition;
            ceiling.transform.rotation = Quaternion.Euler(180, 0, 0);
            ceiling.transform.localScale = scale;
            ceiling.transform.parent = _roomContainer.transform;
            ceiling.tag = "Ceiling";
            AddMaterial(ceiling);
        }

        private void AddMaterial(GameObject mesh)
        {
            var meshRenderer = mesh.GetComponent<MeshRenderer>();
            if (!meshRenderer) meshRenderer = mesh.AddComponent<MeshRenderer>();
            meshRenderer.material = _material;
        }

        private void AdjustRoomPositionToContainPlayer()
        {
            Vector3? cornerPosition = null;

            for (var x = 0; x < _gridSizeX; x++)
            {
                for (var y = 0; y < _gridSizeY; y++)
                {
                    var cell = _gridCells[x, y];
                    if (cell.HasTopWall() && cell.HasLeftWall())
                    {
                        cornerPosition = new Vector3((x - 0.5f) * Spacing, 0, (y + 0.5f) * Spacing);
                        break;
                    }
                }

                if (cornerPosition.HasValue)
                {
                    var playerPosition = Camera.main.transform.position;
                    playerPosition.y = 0f;
                    _roomContainer.transform.position += playerPosition - cornerPosition.Value;
                    break;
                }
            }
        }
    }
}