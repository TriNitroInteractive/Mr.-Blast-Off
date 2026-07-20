using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

/// <summary>
/// Controls the pause menu state, unscaled timing, resume logic, scene transition to MainMenu,
/// and handles Escape and P keyboard shortcuts via the new Input System.
/// </summary>
public class PauseMenuController : MonoBehaviour
{
    [Header("Manual Assignments (Optional)")]
    [Tooltip("The main Pause Button on the screen HUD.")]
    [SerializeField] private Button pauseButton;

    [Tooltip("The Pause Menu container panel.")]
    [SerializeField] private GameObject pauseMenuPanel;

    [Tooltip("The Resume Button inside the pause menu.")]
    [SerializeField] private Button resumeButton;

    [Tooltip("The Main Menu Button inside the pause menu.")]
    [SerializeField] private Button mainMenuButton;

    private bool _isPaused = false;

    private void Start()
    {
        // 1. Locate components dynamically if not manually assigned
        Canvas canvas = Object.FindAnyObjectByType<Canvas>();
        if (canvas != null)
        {
            // Pause Menu is inactive by default, so we find it via Transform.Find on the parent Canvas
            if (pauseMenuPanel == null)
            {
                Transform menuTransform = canvas.transform.Find("Pause Menu");
                if (menuTransform != null)
                {
                    pauseMenuPanel = menuTransform.gameObject;
                }
            }

            // Pause Button is active by default
            if (pauseButton == null)
            {
                Transform buttonTransform = canvas.transform.Find("Pause Button");
                if (buttonTransform != null)
                {
                    pauseButton = buttonTransform.GetComponent<Button>();
                }
            }
        }

        // Find buttons nested inside the Pause Menu if found
        if (pauseMenuPanel != null)
        {
            if (resumeButton == null)
            {
                Transform resumeTransform = pauseMenuPanel.transform.Find("Resume");
                if (resumeTransform != null)
                {
                    resumeButton = resumeTransform.GetComponent<Button>();
                }
            }

            if (mainMenuButton == null)
            {
                Transform mainMenuTransform = pauseMenuPanel.transform.Find("Main Menu");
                if (mainMenuTransform != null)
                {
                    mainMenuButton = mainMenuTransform.GetComponent<Button>();
                }
            }
        }

        // 2. Add visual juice to all buttons
        ApplyJuiceToButton(pauseButton);
        ApplyJuiceToButton(resumeButton);
        ApplyJuiceToButton(mainMenuButton);

        // 3. Register click events
        if (pauseButton != null)
        {
            pauseButton.onClick.AddListener(PauseGame);
        }
        else
        {
            Debug.LogWarning("[PauseMenuController] Pause Button reference could not be resolved.");
        }

        if (resumeButton != null)
        {
            resumeButton.onClick.AddListener(UnpauseGame);
        }
        else
        {
            Debug.LogWarning("[PauseMenuController] Resume Button reference could not be resolved.");
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(LoadMainMenu);
        }
        else
        {
            Debug.LogWarning("[PauseMenuController] Main Menu Button reference could not be resolved.");
        }

        // 4. Ensure initial game state is unpaused
        Time.timeScale = 1f;
        _isPaused = false;
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }
        if (pauseButton != null)
        {
            pauseButton.gameObject.SetActive(true);
        }
    }

    private void Update()
    {
        // 5. Monitor keyboard input via the new Input System
        if (Keyboard.current != null)
        {
            if (Keyboard.current.escapeKey.wasPressedThisFrame || Keyboard.current.pKey.wasPressedThisFrame)
            {
                TogglePause();
            }
        }
    }

    /// <summary>
    /// Helper to apply visual spring scaling to the buttons.
    /// </summary>
    private void ApplyJuiceToButton(Button button)
    {
        if (button != null && button.gameObject.GetComponent<UIButtonJuice>() == null)
        {
            button.gameObject.AddComponent<UIButtonJuice>();
        }
    }

    /// <summary>
    /// Toggles the current pause state.
    /// </summary>
    public void TogglePause()
    {
        if (_isPaused)
        {
            UnpauseGame();
        }
        else
        {
            PauseGame();
        }
    }

    /// <summary>
    /// Pauses game simulation and reveals the pause panel.
    /// </summary>
    public void PauseGame()
    {
        if (_isPaused) return;

        Debug.Log("[PauseMenuController] Pausing game simulation...");
        _isPaused = true;
        Time.timeScale = 0f;

        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(true);
        }

        if (pauseButton != null)
        {
            pauseButton.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Resumes active game simulation and hides the pause panel.
    /// </summary>
    public void UnpauseGame()
    {
        if (!_isPaused) return;

        Debug.Log("[PauseMenuController] Resuming game simulation...");
        _isPaused = false;
        Time.timeScale = 1f;

        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }

        if (pauseButton != null)
        {
            pauseButton.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Safely reverts TimeScale to 1 and loads the Main Menu scene.
    /// </summary>
    public void LoadMainMenu()
    {
        Debug.Log("[PauseMenuController] Loading Main Menu scene...");
        
        // Critical: Must reset TimeScale back to 1 before moving scenes to prevent general simulation freezes
        Time.timeScale = 1f;
        _isPaused = false;

        SceneManager.LoadScene("MainMenu");
    }

    private void OnDestroy()
    {
        // Safety: Ensure timescale is restored to normal if this controller is destroyed unexpectedly
        Time.timeScale = 1f;
    }
}
