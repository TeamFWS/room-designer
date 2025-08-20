using System;
using System.Collections.Generic;
using Ikea;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class FurnitureMenu : MonoBehaviour
    {
        public GameObject buttonPrefab;
        public Button backButton;
        public TMP_Text categoryName;
        public GameObject categoryList;
        public GameObject furnitureListGameObject;
        public IkeaModelLoader ikeaModelLoader;
        public FurnitureList furnitureList;

        [SerializeField] private List<Category> categories = new();

        private void Start()
        {
            backButton.gameObject.SetActive(false);
            backButton.onClick.AddListener(OnBackButtonClicked);

            foreach (var category in categories)
            {
                var buttonObj = Instantiate(buttonPrefab, gameObject.transform);
                buttonObj.name = category.Name;
                var button = buttonObj.GetComponent<Button>();

                var textComponent = buttonObj.GetComponentInChildren<TMP_Text>();
                if (textComponent) textComponent.text = category.Name;

                button.onClick.AddListener(() => OnCategoryButtonClicked(category));
            }
        }

        private void OnCategoryButtonClicked(Category category)
        {
            backButton.gameObject.SetActive(true);
            furnitureListGameObject.SetActive(true);
            categoryList.SetActive(false);
            categoryName.SetText(category.Name);

            if (ikeaModelLoader != null)
                ikeaModelLoader.LoadModelsFromUrl(category.URL);
            else
                Debug.LogError("IkeaModelLoader is not assigned!");
        }

        private void OnBackButtonClicked()
        {
            backButton.gameObject.SetActive(false);
            furnitureListGameObject.SetActive(false);
            categoryList.SetActive(true);
            categoryName.SetText("");
            furnitureList.Clear();
        }

        [Serializable]
        public class Category
        {
            public string Name;
            public string URL;
        }
    }
}