using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class MenuManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject menuPanel;      
    [SerializeField] private GameObject controlsPanel; 

    [Header("Buttons")]
    [SerializeField] private Button playButton;        
    [SerializeField] private Button resumeButton;      
    [SerializeField] private Button controlsButton;
    [SerializeField] private Button exitButton;
    [SerializeField] private Button controlsBackButton;

    [Header("Optional")]
    [SerializeField] private Text titleText;          
    [SerializeField] private bool showMenuOnStart = true;

    private enum State { StartMenu, Playing, Paused }
    private State state;

    private void Awake()
    {
        playButton.onClick.AddListener(Play);
        resumeButton.onClick.AddListener(Resume);
        controlsButton.onClick.AddListener(OpenControls);
        controlsBackButton.onClick.AddListener(CloseControls);
        exitButton.onClick.AddListener(ExitGame);
    }

    private void Start()
    {
        if (showMenuOnStart) ShowStartMenu();
        else StartPlaying();
    }

    private void Update()
    {
        if (!PausePressed()) return;

        if (controlsPanel.activeSelf)         
            CloseControls();
        else if (state == State.Playing)
            Pause();
        else if (state == State.Paused)
            Resume();
    }

    private void ShowStartMenu()
    {
        state = State.StartMenu;
        SetGameFrozen(true);
        ShowMenu("MY GAME", showPlay: true);
    }

    private void Play() => StartPlaying();

    private void StartPlaying()
    {
        state = State.Playing;
        menuPanel.SetActive(false);
        controlsPanel.SetActive(false);
        SetGameFrozen(false);
    }

    public void Pause()
    {
        state = State.Paused;
        SetGameFrozen(true);
        ShowMenu("PAUSED", showPlay: false);
    }

    public void Resume() => StartPlaying();

    private void OpenControls()
    {
        menuPanel.SetActive(false);
        controlsPanel.SetActive(true);
        Select(controlsBackButton.gameObject);
    }

    private void CloseControls()
    {
        controlsPanel.SetActive(false);
        menuPanel.SetActive(true);
        Select(state == State.StartMenu ? playButton.gameObject : resumeButton.gameObject);
    }

    private void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void ShowMenu(string title, bool showPlay)
    {
        controlsPanel.SetActive(false);
        menuPanel.SetActive(true);
        playButton.gameObject.SetActive(showPlay);
        resumeButton.gameObject.SetActive(!showPlay);
        if (titleText != null) titleText.text = title;
        Select(showPlay ? playButton.gameObject : resumeButton.gameObject);
    }

    private void SetGameFrozen(bool frozen)
    {
        Time.timeScale = frozen ? 0f : 1f;
        AudioListener.pause = frozen;
        Cursor.visible = frozen;
        Cursor.lockState = frozen ? CursorLockMode.None : CursorLockMode.Locked;
    }

    private void Select(GameObject go)
    {
        if (EventSystem.current == null) return;
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(go);
    }
    private bool PausePressed()
    {
#if ENABLE_INPUT_SYSTEM
        bool key = Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
        bool pad = Gamepad.current != null && Gamepad.current.startButton.wasPressedThisFrame;
        return key || pad;
#else
        return Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.JoystickButton7);
#endif
    }
}
