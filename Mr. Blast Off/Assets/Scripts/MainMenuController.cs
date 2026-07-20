using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controller for the Main Menu. Automatically finds the 'Play' and 'Quit' buttons,
/// applies juicy scaling effects, and hooks up the play button to the rocket ignition effect
/// and the quit button to application exit.
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [Header("Manual Assignments (Optional)")]
    [Tooltip("Reference to the Play Button. If null, will be auto-located by name.")]
    [SerializeField] private Button playButton;

    [Tooltip("Reference to the Quit Button. If null, will be auto-located by name.")]
    [SerializeField] private Button quitButton;

    private void Start()
    {
        // 1. Locate buttons if not manually assigned
        if (playButton == null)
        {
            GameObject playGo = GameObject.Find("Play");
            if (playGo != null)
            {
                playButton = playGo.GetComponent<Button>();
            }
        }

        if (quitButton == null)
        {
            GameObject quitGo = GameObject.Find("Quit");
            if (quitGo != null)
            {
                quitButton = quitGo.GetComponent<Button>();
            }
        }

        // 2. Validate play button and setup behaviors
        if (playButton != null)
        {
            Debug.Log("[MainMenuController] Play button found and registered.");
            
            // Add visual button juice (scaling spring behavior) dynamically if not already attached
            if (playButton.gameObject.GetComponent<UIButtonJuice>() == null)
            {
                playButton.gameObject.AddComponent<UIButtonJuice>();
            }

            // Setup or find the holographic fade-out effect on the Play button
            UIHolographicEffect holoEffect = playButton.gameObject.GetComponent<UIHolographicEffect>();
            if (holoEffect == null)
            {
                holoEffect = playButton.gameObject.AddComponent<UIHolographicEffect>();
            }

            // Hook up the button click event to trigger the hologram de-materialization
            playButton.onClick.AddListener(() =>
            {
                // Disable button interactions once clicked to prevent double launches
                playButton.interactable = false;
                if (quitButton != null) quitButton.interactable = false;

                // Start the holographic fade-out sequence!
                holoEffect.StartHolographicTransition();
            });
        }
        else
        {
            Debug.LogError("[MainMenuController] Could not find GameObject named 'Play' with a Button component in the scene!");
        }

        // 3. Validate quit button and setup behaviors
        if (quitButton != null)
        {
            Debug.Log("[MainMenuController] Quit button found and registered.");

            // Add visual button juice dynamically if not already attached
            if (quitButton.gameObject.GetComponent<UIButtonJuice>() == null)
            {
                quitButton.gameObject.AddComponent<UIButtonJuice>();
            }

            // Hook up the button click event to exit the application
            quitButton.onClick.AddListener(() =>
            {
                Debug.Log("[MainMenuController] Quit requested. Shutting down Mr. Blast Off...");
                Application.Quit();

#if UNITY_EDITOR
                // Provide visual confirmation of quit state when playing in Editor
                UnityEditor.EditorApplication.isPlaying = false;
#endif
            });
        }
        else
        {
            Debug.LogError("[MainMenuController] Could not find GameObject named 'Quit' with a Button component in the scene!");
        }
    }
}
