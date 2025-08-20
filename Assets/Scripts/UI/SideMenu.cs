using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class SideMenu : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI mainText;
        [SerializeField] private Toggle[] toggles;
        [SerializeField] private string[] texts;
        [SerializeField] private GameObject[] contents;
        [SerializeField] private AppState[] appStates;
        private Color _normalColor;
        private Color _selectedColor;

        private int _selectedIndex;

        private void Awake()
        {
            for (var i = 0; i < toggles.Length; i++)
            {
                var index = i;
                toggles[i].onValueChanged.AddListener(isOn => OnToggleChanged(isOn, index));
            }

            DeactivateAllContents();
            if (toggles.Length > 0)
            {
                toggles[0].isOn = true;
                _normalColor = toggles[0].colors.normalColor;
                _selectedColor = toggles[0].colors.selectedColor;
                SetSelectedToggle(0);
            }
        }

        private void Start()
        {
            if (toggles.Length != texts.Length || contents.Length != texts.Length)
                Debug.LogError("The number of toggles, texts, or contents do not match!");
        }

        private void OnToggleChanged(bool isOn, int index)
        {
            if (isOn && index != _selectedIndex) SetSelectedToggle(index);
        }

        private void SetSelectedToggle(int index)
        {
            _selectedIndex = index;
            SetContent(index);
            HighlightSelected(index);
        }

        private void HighlightSelected(int index)
        {
            foreach (var toggle in toggles)
            {
                var colors = toggle.colors;
                colors.normalColor = _normalColor;
                toggle.colors = colors;
            }

            var toggleColors = toggles[index].colors;
            toggleColors.normalColor = _selectedColor;
            toggles[index].colors = toggleColors;
        }

        private void SetContent(int index)
        {
            mainText.text = texts[index];
            StateManager.Instance.ChangeState(appStates[index]);
            DeactivateAllContents();
            if (contents[index] != null) contents[index].SetActive(true);
        }

        private void DeactivateAllContents()
        {
            foreach (var content in contents)
                if (content != null)
                    content.SetActive(false);
        }
    }
}