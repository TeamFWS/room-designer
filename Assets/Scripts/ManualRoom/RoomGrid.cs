using UnityEngine;

namespace ManualRoom
{
    public class RoomGrid : MonoBehaviour
    {
        private const float DistanceFromCamera = 0.5f;
        private const float YOffset = -0.2f;
        [SerializeField] private GameObject cellPrefab;
        [SerializeField] private int gridSizeX = 15;
        [SerializeField] private int gridSizeY = 20;
        [SerializeField] private float spacing = 2.0f;
        private GameObject _gridContainer;
        private Transform _playerCamera;

        public GridCell[,] GridCells { get; private set; }

        public int GridSizeX => gridSizeX;
        public int GridSizeY => gridSizeY;


        private void Start()
        {
            _playerCamera = Camera.main.GetComponent<Transform>();
        }

        public void RemoveGrid()
        {
            Destroy(_gridContainer);
        }

        public void SpawnGrid()
        {
            _gridContainer = new GameObject("GridContainer");
            GridCells = new GridCell[gridSizeX, gridSizeY];

            var offsetX = (gridSizeX - 1) * spacing / 2.0f;
            for (var x = 0; x < gridSizeX; x++)
            for (var y = 0; y < gridSizeY; y++)
            {
                var spawnPosition = new Vector3(x * spacing - offsetX, 0, y * spacing);
                var cellObject = Instantiate(cellPrefab, spawnPosition, Quaternion.identity);

                cellObject.transform.parent = _gridContainer.transform;
                GridCells[x, y] = cellObject.GetComponent<GridCell>();
                GridCells[x, y].x = x;
                GridCells[x, y].y = y;
            }

            PositionGridInFrontOfPlayer();
        }

        private void PositionGridInFrontOfPlayer()
        {
            var gridPosition = _playerCamera.position + _playerCamera.forward * DistanceFromCamera;
            gridPosition.y = _playerCamera.position.y + YOffset;

            _gridContainer.transform.position = gridPosition + new Vector3(0f, -0.5f, 0f);
            _gridContainer.transform.localScale = new Vector3(0.03f, 0.03f, 0.03f);

            var directionToGrid = _gridContainer.transform.position - _playerCamera.position;
            directionToGrid.y = 0;

            if (directionToGrid.sqrMagnitude > 0.01f)
            {
                var fixedXRotation = Quaternion.Euler(-60f, 0f, 0f);
                var gridRotation = Quaternion.LookRotation(directionToGrid) * fixedXRotation;
                _gridContainer.transform.rotation = gridRotation;
            }
        }

        public GridCell GetGridCellFromHit(GameObject hitObject)
        {
            for (var x = 0; x < gridSizeX; x++)
            for (var y = 0; y < gridSizeY; y++)
            {
                var cell = GetCellAt(x, y);
                if (cell && cell.trigger == hitObject)
                    return cell;
            }

            return null;
        }

        public void CreateWallBetweenCells(GridCell start, GridCell end)
        {
            if (!start || !end)
                return;

            if (start.x == end.x)
                CreateWallInLine(start, end, true);
            else if (start.y == end.y)
                CreateWallInLine(start, end, false);
        }

        private void CreateWallInLine(GridCell start, GridCell end, bool isVertical)
        {
            var minCoord = isVertical ? Mathf.Min(start.y, end.y) : Mathf.Min(start.x, end.x);
            var maxCoord = isVertical ? Mathf.Max(start.y, end.y) : Mathf.Max(start.x, end.x);

            for (var coord = minCoord; coord <= maxCoord; coord++)
            {
                var cell = isVertical ? GetCellAt(start.x, coord) : GetCellAt(coord, start.y);
                if (cell)
                {
                    cell.MarkWall(start);
                    cell.MarkWall(end);
                }
            }
        }

        private GridCell GetCellAt(int x, int y)
        {
            if (x >= 0 && x < gridSizeX && y >= 0 && y < gridSizeY)
                return GridCells[x, y];
            return null;
        }
    }
}