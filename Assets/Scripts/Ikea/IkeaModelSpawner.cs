using System.Collections.Generic;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace Ikea
{
    public class IkeaModelSpawner : MonoBehaviour
    {
        public GameObject togglePrefab;
        public GameObject fetchingPrefab;
        public Transform toggleParent;
        private readonly Queue<GameObject> _fetchingQueue = new();

        private FurnitureList _furnitureList;

        private void Start()
        {
            _furnitureList = FindFirstObjectByType<FurnitureList>();
            IkeaModelLoader.OnModelLoaded += ReplaceFetchingWithModel;
        }

        private void OnDestroy()
        {
            IkeaModelLoader.OnModelLoaded -= ReplaceFetchingWithModel;
        }

        public void AddFetchingPlaceholder(string modelName)
        {
            var fetchingObj = Instantiate(fetchingPrefab, toggleParent);
            fetchingObj.name = "Fetching_" + modelName;

            _fetchingQueue.Enqueue(fetchingObj);
        }

        private void ReplaceFetchingWithModel(string modelName, GameObject modelPrefab)
        {
            if (_fetchingQueue.Count == 0) return;

            var fetchingObj = _fetchingQueue.Dequeue();
            if (fetchingObj == null) return;

            var toggleObj = Instantiate(togglePrefab, toggleParent);
            toggleObj.name = modelName;
            Destroy(fetchingObj);

            var toggle = toggleObj.GetComponent<Toggle>();
            var textComponent = toggleObj.GetComponentInChildren<TMP_Text>();
            if (textComponent) textComponent.text = modelName;

            var furnitureParent = new GameObject(modelName + "_Container");
            var modelInstance = Instantiate(modelPrefab, furnitureParent.transform);
            modelInstance.SetActive(true);

            modelInstance.transform.SetParent(toggleObj.transform, false);
            modelInstance.transform.localPosition = new Vector3(0f, 0f, -0.2f);
            modelInstance.transform.localScale = new Vector3(30f, 30f, 0.3f);
            modelInstance.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);

            _furnitureList.RegisterFurniture(modelInstance, toggle);
        }
    }
}