using UnityEngine;
using UnityEngine.XR;

namespace ManualRoom
{
    public class RaySelectGrid : MonoBehaviour
    {
        private GridCell _endCell;
        private GridCell _hoveredCell;
        private bool _isSelecting;
        private Transform _origin;
        private RoomGrid _roomGrid;
        private GridCell _startCell;

        private void Start()
        {
            _roomGrid = GameObject.FindWithTag("Scripts").GetComponent<RoomGrid>();
            _origin = GameObject.FindWithTag("RightHandPointer").GetComponent<Transform>();
        }

        private void Update()
        {
            HandleRaycast();

            if (InputDevices.GetDeviceAtXRNode(XRNode.RightHand)
                .TryGetFeatureValue(CommonUsages.trigger, out var triggerValue))
            {
                if (triggerValue >= 0.5f && !_isSelecting)
                {
                    _isSelecting = true;
                    if (_hoveredCell)
                        HandleCellSelection(_hoveredCell);
                    else
                        DeselectStartEnd();
                }
                else if (triggerValue < 0.5f && _isSelecting)
                {
                    _isSelecting = false;
                }
            }
        }

        private void HandleRaycast()
        {
            if (Physics.Raycast(_origin.position, _origin.forward, out var hit))
            {
                var hoveredCell = _roomGrid.GetGridCellFromHit(hit.collider.gameObject);

                if (hoveredCell != _hoveredCell)
                {
                    if (_hoveredCell) _hoveredCell.SetHovered(false);
                    _hoveredCell = hoveredCell;
                    if (_hoveredCell) _hoveredCell.SetHovered();
                }
            }
            else if (_hoveredCell)
            {
                _hoveredCell.SetHovered(false);
                _hoveredCell = null;
            }
        }

        private void HandleCellSelection(GridCell cell)
        {
            if (cell.IsSelected())
            {
                cell.SetSelected(false);
                DeselectStartEnd();
            }
            else if (!_startCell)
            {
                _startCell = cell;
                cell.SetSelected();
            }
            else if (!_endCell)
            {
                _endCell = cell;
                cell.SetSelected();

                _roomGrid.CreateWallBetweenCells(_startCell, _endCell);
                DeselectStartEnd();
            }
        }

        private void DeselectStartEnd()
        {
            if (_startCell) _startCell.SetSelected(false);
            if (_endCell) _endCell.SetSelected(false);
            _startCell = null;
            _endCell = null;
        }
    }
}