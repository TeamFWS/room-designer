using ManualRoom;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class ManualCreateRoom : MonoBehaviour
    {
        [SerializeField] private Button cancelButton;
        [SerializeField] private Button confirmButton;

        private void Start()
        {
            var roomCreation = GameObject.FindWithTag("Scripts").GetComponent<RoomCreation>();
            cancelButton.onClick.AddListener(() => StateManager.Instance.RevertPreviousState());
            confirmButton.onClick.AddListener(roomCreation.GenerateRoom);
        }
    }
}