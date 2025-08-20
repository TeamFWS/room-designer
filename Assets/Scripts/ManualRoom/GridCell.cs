using UnityEngine;

namespace ManualRoom
{
    public class GridCell : MonoBehaviour
    {
        [SerializeField] private Color defaultColor;
        [SerializeField] private Color selectedColor;
        [SerializeField] private Color hoverColor;
        [SerializeField] private Color wallColor;

        [SerializeField] private SpriteRenderer centerPoint;
        [SerializeField] private SpriteRenderer topWall;
        [SerializeField] private SpriteRenderer rightWall;
        [SerializeField] private SpriteRenderer bottomWall;
        [SerializeField] private SpriteRenderer leftWall;

        public GameObject trigger;
        public int x;
        public int y;

        private bool _hasBottomWall;
        private bool _hasLeftWall;
        private bool _hasRightWall;
        private bool _hasTopWall;
        private bool _isSelected;

        public bool IsSelected()
        {
            return _isSelected;
        }

        public void SetSelected(bool selected = true)
        {
            _isSelected = selected;
            centerPoint.color = selected ? selectedColor : defaultColor;
        }

        public void SetHovered(bool hovered = true)
        {
            if (!_isSelected) centerPoint.color = hovered ? hoverColor : defaultColor;
        }

        public void MarkWall(GridCell other)
        {
            if (x < other.x)
            {
                rightWall.color = wallColor;
                _hasRightWall = true;
            }
            else if (x > other.x)
            {
                leftWall.color = wallColor;
                _hasLeftWall = true;
            }
            else if (y < other.y)
            {
                topWall.color = wallColor;
                _hasTopWall = true;
            }
            else if (y > other.y)
            {
                bottomWall.color = wallColor;
                _hasBottomWall = true;
            }
        }

        public bool HasRightWall()
        {
            return _hasRightWall;
        }

        public bool HasLeftWall()
        {
            return _hasLeftWall;
        }

        public bool HasTopWall()
        {
            return _hasTopWall;
        }

        public bool HasBottomWall()
        {
            return _hasBottomWall;
        }
    }
}