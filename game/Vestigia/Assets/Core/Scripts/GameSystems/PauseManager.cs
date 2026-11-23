
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance { get; private set; }
    public static bool IsPaused { get; private set; }

    [SerializeField] private GameObject pauseCanvas;

    private bool isPaused;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        IsPaused = false;
        isPaused = false;

        DontDestroyOnLoad(gameObject);

        if (pauseCanvas != null)
            pauseCanvas.SetActive(false);
    }

    private void Update()
    {
        if (Keyboard.current != null &&
            Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        if (isPaused)
            Resume();
        else
            Pause();
    }

    private void Pause()
    {
        isPaused = true;
        IsPaused = true;

        if (pauseCanvas != null)
            pauseCanvas.SetActive(true);

        PauseBlurRendererFeature.BlurEnabled = true;
        Time.timeScale = 0f;
    }

    public void Resume()
    {
        isPaused = false;
        IsPaused = false;

        if (pauseCanvas != null)
            pauseCanvas.SetActive(false);

        PauseBlurRendererFeature.BlurEnabled = false;
        Time.timeScale = 1f;
    }

    public void ExitGame()
    {
        Resume();
        SceneManager.LoadScene("MenuPrincipal");
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            PauseBlurRendererFeature.BlurEnabled = false;
            IsPaused = false;
            Time.timeScale = 1f;
            Instance = null;
        }
    }
}
