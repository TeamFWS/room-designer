using ManualRoom;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class RoomTab : MonoBehaviour
    {
        [SerializeField] private Button manualCreate;

        private void Start()
        {
            var roomCreation = GameObject.FindWithTag("Scripts").GetComponent<RoomCreation>();
            manualCreate.onClick.AddListener(() => StateManager.Instance.ChangeState(AppState.RoomCreation));
        }
    }
}