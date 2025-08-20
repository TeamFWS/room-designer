using UnityEngine;

namespace ManualRoom
{
    public class RoomCreation : MonoBehaviour
    {
        private const float DistanceFromCamera = 0.5f;
        private const float YOffset = -0.2f;
        private const float UISideOffset = 0.8f;
        [SerializeField] private GameObject uiPrefab;
        [SerializeField] private RoomGrid roomGrid;
        [SerializeField] private Material defaultMaterial;

        private Transform _playerCamera;
        private RoomGenerator _roomGenerator;

        private void Start()
        {
            _playerCamera = Camera.main.transform;
        }

        private void OnEnable()
        {
            StateManager.Instance.StateChanged += OnStateChanged;
        }

        private void OnDisable()
        {
            StateManager.Instance.StateChanged -= OnStateChanged;
        }

        private void OnStateChanged(AppState newState)
        {
            if (newState == AppState.RoomCreation)
                Activate();
            else
                Deactivate();
        }

        private void Activate()
        {
            roomGrid.SpawnGrid();
            uiPrefab.SetActive(true);
            PositionUIInFrontOfPlayer();
        }

        private void Deactivate()
        {
            roomGrid.RemoveGrid();
            uiPrefab.SetActive(false);
        }

        public void GenerateRoom()
        {
            _roomGenerator = new RoomGenerator(roomGrid, defaultMaterial);
            _roomGenerator.GenerateRoomModel();
            StateManager.Instance.ChangeState(AppState.Main);
        }

        private void PositionUIInFrontOfPlayer()
        {
            var rightOffset = _playerCamera.right * UISideOffset;
            var uiPosition = _playerCamera.position + _playerCamera.forward * DistanceFromCamera + rightOffset;
            uiPosition.y = _playerCamera.position.y + YOffset;

            uiPrefab.transform.position = uiPosition;

            var directionToUI = uiPrefab.transform.position - _playerCamera.position;
            directionToUI.y = 0;

            if (directionToUI.sqrMagnitude > 0.01f)
            {
                var uiRotation = Quaternion.LookRotation(directionToUI);
                uiPrefab.transform.rotation = uiRotation;
            }
        }
    }
}