using System.Collections.Generic;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

public class FurnitureManipulator : MonoBehaviour
{
    public GameObject furnitureModel;
    public GameObject sliderObject;
    public GameObject menu;
    public GameObject nameHolder;
    private float speed = 1.0f;
    private Slider slider;

    private void Start()
    {
        slider = sliderObject.GetComponent<Slider>();
    }

    public void CloseMenu()
    {
        menu.gameObject.SetActive(false);
    }

    public void SetSpeed(int value)
    {
        switch (value)
        {
            case 1:
                speed = 0.1f;
                break;
            case 2:
                speed = 1.0f;
                break;
            case 3:
                speed = 10.0f;
                break;
        }
    }

    public void OpenMenu()
    {
        menu.transform.position = furnitureModel.transform.position;
        menu.transform.Translate(new Vector3(0, 0.5f, 0));
        menu.gameObject.SetActive(true);
    }

    public void MoveFurniture(int directionValue)
    {
        Vector3 direction = Vector3.forward;
        switch (directionValue)
        {
            case 1:
                direction = Vector3.forward;
                break;
            case 2:
                direction = Vector3.left;
                break;
            case 3:
                direction = Vector3.right;
                break;
            case 4:
                direction = Vector3.back;
                break;
        }
        furnitureModel.transform.Translate(direction * speed * Time.deltaTime);
    }

    public void RotateFurniture(float value)
    {
        furnitureModel.transform.localRotation = Quaternion.Euler(0, slider.value * 360, 0);
    }

    public void Rename(string name)
    {
        nameHolder.GetComponent<TMPro.TextMeshProUGUI>().text = name;
    }
}
