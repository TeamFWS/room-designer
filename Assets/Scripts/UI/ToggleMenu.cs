using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;

namespace UI
{
    public class ToggleMenu : MonoBehaviour
    {
        private const float DistanceFromCamera = 0.8f;
        private const float YOffset = -0.2f;
        [SerializeField] private GameObject menuUI;
        [SerializeField] private Button closeButton;
        private bool _isDisabled = true;
        private bool _isMenuVisible;

        private Transform _playerCamera;
        private bool _wasMenuButtonPressed;

        private void Start()
        {
            _playerCamera = Camera.main.GetComponent<Transform>();
            menuUI.SetActive(false);
            closeButton.onClick.AddListener(() => Toggle(false));
        }

        private void Update()
        {
            if (_isDisabled || !InputDevices.GetDeviceAtXRNode(XRNode.LeftHand)
                    .TryGetFeatureValue(CommonUsages.menuButton, out var isMenuButtonPressed)) return;
            if (isMenuButtonPressed && !_wasMenuButtonPressed) Toggle();
            _wasMenuButtonPressed = isMenuButtonPressed;
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
            if (newState is AppState.Main or AppState.PlacingFurniture or AppState.Painting)
            {
                _isDisabled = false;
            }
            else
            {
                _isDisabled = true;
                Toggle(false);
            }
        }

        private void Toggle()
        {
            _isMenuVisible = !_isMenuVisible;
            menuUI.SetActive(_isMenuVisible);

            if (_isMenuVisible) PositionMenuInFrontOfPlayer();
            else if (StateManager.Instance.CurrentState is AppState.PlacingFurniture or AppState.Painting)
                StateManager.Instance.ChangeState(AppState.Main);
        }

        private void Toggle(bool active)
        {
            _isMenuVisible = !active;
            Toggle();
        }

        private void PositionMenuInFrontOfPlayer()
        {
            var menuPosition = _playerCamera.position + _playerCamera.forward * DistanceFromCamera;
            menuPosition.y = _playerCamera.position.y + YOffset;

            menuUI.transform.position = menuPosition;

            var directionToMenu = menuUI.transform.position - _playerCamera.position;
            directionToMenu.y = 0;

            if (directionToMenu.sqrMagnitude > 0.01f)
            {
                var fixedXRotation = Quaternion.Euler(15f, 0f, 0f);
                var menuRotation = Quaternion.LookRotation(directionToMenu) * fixedXRotation;

                menuUI.transform.rotation = menuRotation;
            }
        }
    }
}