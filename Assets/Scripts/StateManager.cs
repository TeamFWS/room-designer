using UnityEngine;

public class StateManager : MonoBehaviour
{
    public delegate void OnStateChange(AppState newState);

    private static StateManager _instance;

    private AppState _previousState;

    public static StateManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<StateManager>();

                if (_instance == null)
                {
                    var singletonObject = new GameObject("StateManager");
                    _instance = singletonObject.AddComponent<StateManager>();
                    DontDestroyOnLoad(singletonObject);
                }
            }

            return _instance;
        }
    }

    public AppState CurrentState { get; private set; }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    public event OnStateChange StateChanged;

    public void ChangeState(AppState newState)
    {
        if (CurrentState != newState)
        {
            _previousState = CurrentState;
            CurrentState = newState;
            StateChanged?.Invoke(newState);
        }
    }

    public void RevertPreviousState()
    {
        ChangeState(_previousState);
    }
}