using UnityEditor;
using UnityEngine;

public class StartState : MonoBehaviour
{
    [SerializeField] private GameObject cameraRig;
    [SerializeField] private GameObject startRoom;
    [SerializeField] private GameObject startMenuUI;

    private void Start()
    {
        OnStateChanged(AppState.StartMenu);
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
        var isStartState = newState == AppState.StartMenu;
        startRoom.gameObject.SetActive(isStartState);
        startMenuUI.gameObject.SetActive(isStartState);
        cameraRig.gameObject.transform.position = new Vector3(0, 0, 0);
    }

    public void StartNewVRProject()
    {
        StateManager.Instance.ChangeState(AppState.RoomCreation);
    }

    public void StartNewMRProject()
    {
        StateManager.Instance.ChangeState(AppState.MRInitialization);
    }

    public void ExitApplication()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }
}