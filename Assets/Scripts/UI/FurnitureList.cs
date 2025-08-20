using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class FurnitureList : MonoBehaviour
    {
        private readonly List<GameObject> _furniture = new();
        private readonly List<Toggle> _toggles = new();
        private Color _normalColor;
        private RaySpawner _raySpawner;
        private Color _selectedColor;

        private void Start()
        {
            _raySpawner = FindFirstObjectByType<RaySpawner>();
        }

        private void OnDisable()
        {
            if (_raySpawner) _raySpawner.SetObjectToSpawn(null);
        }

        private void OnDestroy()
        {
            if (_raySpawner) _raySpawner.SetObjectToSpawn(null);
        }

        public void RegisterFurniture(GameObject model, Toggle toggle)
        {
            if (!model || !toggle) return;

            _furniture.Add(model);
            _toggles.Add(toggle);

            var index = _furniture.Count - 1;

            if (_toggles.Count == 1)
            {
                _normalColor = toggle.colors.normalColor;
                _selectedColor = toggle.colors.selectedColor;
            }

            toggle.isOn = false;
            toggle.onValueChanged.AddListener(isOn => OnToggleChanged(isOn, index));
        }

        private void OnToggleChanged(bool isOn, int index)
        {
            if (isOn) SetSelectedToggle(index);
        }

        private void SetSelectedToggle(int index)
        {
            if (index < 0 || index >= _toggles.Count || index >= _furniture.Count) return;

            for (var i = 0; i < _toggles.Count; i++)
            {
                var colors = _toggles[i].colors;
                colors.normalColor = i == index ? _selectedColor : _normalColor;
                _toggles[i].colors = colors;
            }

            _raySpawner.SetObjectToSpawn(_furniture[index]);
        }

        public void Clear()
        {
            foreach (var furniture in _furniture.Where(furniture => furniture != null)) Destroy(furniture);
            foreach (var toggle in _toggles.Where(toggle => toggle != null)) toggle.onValueChanged.RemoveAllListeners();
            foreach (Transform child in transform) Destroy(child.gameObject);

            _furniture.Clear();
            _toggles.Clear();

            if (_raySpawner) _raySpawner.SetObjectToSpawn(null);
        }
    }
}