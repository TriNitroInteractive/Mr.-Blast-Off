using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

/// <summary>
/// Controls the introductory "Loading Plot" screen at the start of the Preparation scene.
/// - Deactivates the Cockpit workspace initially so players read the plot.
/// - Types out the story text dynamically (with choices of word-by-word or teletype character reveal).
/// - Pulsates/breaths the "Press Any Key" prompt via continuous alpha fading.
/// - Monitors inputs (any key, mouse click, tap) to transition instantly to the Cockpit screen.
/// </summary>
public class LoadingPlotController : MonoBehaviour
{
    [Header("Panel References")]
    [Tooltip("The Loading Plot UI Panel GameObject containing the story text and any-key prompt.")]
    [SerializeField] private GameObject loadingPlotPanel;

    [Tooltip("The main Cockpit workspace panel that should activate after the plot is skipped.")]
    [SerializeField] private GameObject cockpitPanel;

    [Header("Text Components")]
    [Tooltip("TextMeshProUGUI component displaying the typed narrative story.")]
    [SerializeField] private TextMeshProUGUI plotTextComp;

    [Tooltip("TextMeshProUGUI component representing the breathing 'Press Any Key' prompt.")]
    [SerializeField] private TextMeshProUGUI pressAnyKeyText;

    [Header("Typing Effect Settings")]
    [Tooltip("Toggle between a word-by-word reveal or a standard retro teletype character typing effect.")]
    [SerializeField] private bool revealWordByWord = true;

    [Tooltip("Delay in seconds between typing consecutive elements (words or characters).")]
    [SerializeField] private float typingDelay = 0.05f;

    [Header("Breathing Prompt Settings")]
    [Tooltip("How fast the Press Any Key prompt fades in and out.")]
    [SerializeField] private float breathingSpeed = 3f;

    [Range(0f, 1f)]
    [Tooltip("Minimum transparency value of the breathing prompt.")]
    [SerializeField] private float minAlpha = 0.15f;

    [Range(0f, 1f)]
    [Tooltip("Maximum transparency value of the breathing prompt.")]
    [SerializeField] private float maxAlpha = 1.0f;

    private string _fullPlotText = "";
    private Coroutine _typingCoroutine;
    private Coroutine _breathingCoroutine;
    private bool _isTransitionStarted = false;

    private void Awake()
    {
        // 1. Initial State: Cockpit must be fully hidden, Loading Plot active
        if (cockpitPanel != null)
        {
            cockpitPanel.SetActive(false);
        }

        if (loadingPlotPanel != null)
        {
            loadingPlotPanel.SetActive(true);
        }
    }

    private void Start()
    {
        // 2. Fetch, clear, and begin typing out the story plot
        if (plotTextComp != null)
        {
            _fullPlotText = plotTextComp.text;
            plotTextComp.text = ""; // Empty on startup

            _typingCoroutine = StartCoroutine(TypePlotTextRoutine());
        }

        // 3. Begin breathing effect for the prompt text
        if (pressAnyKeyText != null)
        {
            _breathingCoroutine = StartCoroutine(BreathingPromptRoutine());
        }
    }

    private void Update()
    {
        // 4. Continuously listen for any keystroke, mouse click, or tap to advance
        if (!_isTransitionStarted)
        {
            bool inputDetected = false;

            // Check Keyboard anyKey
            if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
            {
                inputDetected = true;
            }

            // Check Mouse click (or any pointer press)
            if (!inputDetected && Pointer.current != null && Pointer.current.press.wasPressedThisFrame)
            {
                inputDetected = true;
            }

            if (inputDetected)
            {
                TransitionToCockpit();
            }
        }
    }

    private IEnumerator TypePlotTextRoutine()
    {
        if (revealWordByWord)
        {
            // Split words using spaces (keeping line breaks intact)
            string[] words = _fullPlotText.Split(' ');
            string currentText = "";

            for (int i = 0; i < words.Length; i++)
            {
                currentText += (i == 0 ? "" : " ") + words[i];
                plotTextComp.text = currentText;

                yield return new WaitForSecondsRealtime(typingDelay);
            }
        }
        else
        {
            // Character-by-character standard retro teletype typing
            char[] chars = _fullPlotText.ToCharArray();
            string currentText = "";

            for (int i = 0; i < chars.Length; i++)
            {
                currentText += chars[i];
                plotTextComp.text = currentText;

                yield return new WaitForSecondsRealtime(typingDelay);
            }
        }
    }

    private IEnumerator BreathingPromptRoutine()
    {
        Color baseColor = pressAnyKeyText.color;

        while (true)
        {
            // Compute sine-wave oscillation to represent light breathing in/out
            float sine = Mathf.Sin(Time.unscaledTime * breathingSpeed); // Oscillates between -1 and 1
            float alpha = Mathf.Lerp(minAlpha, maxAlpha, (sine + 1f) * 0.5f);

            baseColor.a = alpha;
            pressAnyKeyText.color = baseColor;

            yield return null;
        }
    }

    /// <summary>
    /// Deactivates the plot panel, stops running coroutines, activates the cockpit screen, and disables self.
    /// </summary>
    public void TransitionToCockpit()
    {
        if (_isTransitionStarted) return;
        _isTransitionStarted = true;

        Debug.Log("[LoadingPlotController] Transitioning to cockpit space selection menu...");

        // Stop animations
        if (_typingCoroutine != null) StopCoroutine(_typingCoroutine);
        if (_breathingCoroutine != null) StopCoroutine(_breathingCoroutine);

        // Ensure plot text is fully revealed in case skipped early
        if (plotTextComp != null)
        {
            plotTextComp.text = _fullPlotText;
        }

        // Toggle UI Panels
        if (loadingPlotPanel != null)
        {
            loadingPlotPanel.SetActive(false);
        }

        if (cockpitPanel != null)
        {
            cockpitPanel.SetActive(true);
            
            // Show GameplayInstructionsHUD along with cockpit workspace
            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetInstructionsVisible(true);
            }

            // To ensure CockpitManager or any other start sequences behave correctly,
            // we broadcast or force refresh UI if necessary
            CockpitManager cockpitManager = cockpitPanel.GetComponent<CockpitManager>();
            if (cockpitManager != null)
            {
                // Trigger any public refreshes if needed (or let CockpitManager handle its own OnEnable)
                Debug.Log("[LoadingPlotController] Cockpit panel activated. Core systems engaged.");
            }
        }

        // Deactivate this controller as it is no longer required in this scene lifecycle
        gameObject.SetActive(false);
    }
}
