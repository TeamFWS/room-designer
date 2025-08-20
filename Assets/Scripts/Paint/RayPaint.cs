using System.Collections.Generic;
using UI.FlexibleColorPicker.Scripts;
using UnityEngine;
using UnityEngine.XR;

namespace Paint
{
    public class RayPaint : MonoBehaviour
    {
        [SerializeField] private FlexibleColorPicker colorPicker;
        private readonly Dictionary<int, Color> _objectColors = new();
        private Color _currentColor;
        private bool _isEnabled;
        private bool _isPainting;

        private LineRenderer _lineRenderer;
        private Transform _origin;

        private void Start()
        {
            _origin = GameObject.FindWithTag("RightHandPointer").GetComponent<Transform>();
            _currentColor = colorPicker.GetColor();

            _lineRenderer = gameObject.AddComponent<LineRenderer>();
            _lineRenderer.positionCount = 2;
            _lineRenderer.startWidth = 0.02f;
            _lineRenderer.endWidth = 0.02f;
            _lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            _lineRenderer.startColor = _currentColor;
            _lineRenderer.endColor = Color.clear;
            _lineRenderer.enabled = false;
        }

        private void Update()
        {
            if (_isEnabled)
            {
                _currentColor = colorPicker.GetColor();
                _lineRenderer.startColor = _currentColor;

                if (InputDevices.GetDeviceAtXRNode(XRNode.RightHand)
                    .TryGetFeatureValue(CommonUsages.trigger, out var trigger))
                {
                    if (trigger >= 0.5f && !_isPainting)
                        TryToPaint();
                    else if (trigger < 0.5f && _isPainting)
                        _isPainting = false;
                }
            }

            UpdateRayVisual();
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
            _isEnabled = newState == AppState.Painting;
        }

        private void TryToPaint()
        {
            if (Physics.Raycast(_origin.position, _origin.forward, out var hit))
                if (hit.collider.CompareTag("Floor") || hit.collider.CompareTag("Wall") ||
                    hit.collider.CompareTag("Ceiling"))
                    Paint(hit.collider.gameObject);
        }

        private void Paint(GameObject objectToPaint)
        {
            _isPainting = true;
            var color = colorPicker.GetColor();
            _currentColor = color;

            var objectRenderer = objectToPaint.GetComponent<Renderer>();
            if (objectRenderer)
                objectRenderer.material.color = color;

            _objectColors[objectToPaint.GetInstanceID()] = color;
        }

        private void UpdateRayVisual()
        {
            if (!_isEnabled)
            {
                _lineRenderer.enabled = false;
                return;
            }

            if (Physics.Raycast(_origin.position, _origin.forward, out var hit))
            {
                _lineRenderer.enabled = true;
                _lineRenderer.SetPosition(0, _origin.position);
                _lineRenderer.SetPosition(1, hit.point);
            }
            else
            {
                _lineRenderer.enabled = true;
                _lineRenderer.SetPosition(0, _origin.position);
                _lineRenderer.SetPosition(1, _origin.position + _origin.forward * 10f);
            }
        }
    }
}