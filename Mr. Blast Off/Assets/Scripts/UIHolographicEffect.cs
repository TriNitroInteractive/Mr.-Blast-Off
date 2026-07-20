using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Procedural C#-based holographic transition effect for UI buttons.
/// When triggered:
/// - Transforms button colors to an electric cyan/neon sci-fi tint.
/// - Simulates a retro sci-fi projection using alpha flicker and glitch jitter.
/// - Sweeps procedural scanlines down the button.
/// - Slowly fades the button elements (alpha 1.0 down to 0.0) over a configurable duration before loading the next scene.
/// </summary>
public class UIHolographicEffect : MonoBehaviour
{
    [Header("Hologram Style Settings")]
    [Tooltip("Holographic color scheme to apply to text and panels.")]
    [SerializeField] private Color hologramColor = new Color(0.0f, 0.85f, 1.0f, 1f); // Electric Cyan

    [Tooltip("Duration of the holographic fade-out sequence.")]
    [SerializeField] private float transitionDuration = 3.0f;

    [Header("Scanline & Sweep Settings")]
    [Tooltip("Speed of the horizontal sweep scanline overlay.")]
    [SerializeField] private float scanlineSweepSpeed = 4f;

    [Tooltip("Color of the scanline bar.")]
    [SerializeField] private Color scanlineColor = new Color(0.0f, 0.95f, 1.0f, 0.35f);

    private RectTransform _buttonRect;
    private Vector3 _originalLocalPos;
    private bool _isTransitioning = false;

    // References to button's visuals
    private List<GraphicColorState> _buttonGraphics = new List<GraphicColorState>();
    private Image _scanlineBar;

    private struct GraphicColorState
    {
        public Graphic graphic;
        public Color originalColor;
    }

    private void Awake()
    {
        _buttonRect = GetComponent<RectTransform>();
        if (_buttonRect != null)
        {
            _originalLocalPos = _buttonRect.localPosition;
        }
    }

    /// <summary>
    /// Starts the holographic glitch transition and handles loading the Preparation scene.
    /// </summary>
    public void StartHolographicTransition()
    {
        if (_isTransitioning) return;

        Debug.Log("[UIHolographicEffect] Core projection engaged! Initializing holographic button de-materialization...");

        _isTransitioning = true;
        
        // 1. Gather all Graphic components on the button and its children (Texts, Images, Outlines)
        _buttonGraphics.Clear();
        foreach (Graphic g in GetComponentsInChildren<Graphic>(true))
        {
            // Skip existing scanlines or other dynamic components if any
            if (g.gameObject.name == "Holo_Scanline") continue;

            _buttonGraphics.Add(new GraphicColorState
            {
                graphic = g,
                originalColor = g.color
            });
        }

        // 2. Create the procedural scanline sweep bar
        CreateProceduralScanline();

        // 3. Kickoff the coroutine to animate the hologram and load the scene
        StartCoroutine(HologramAnimateAndLoadRoutine());
    }

    private void CreateProceduralScanline()
    {
        if (_buttonRect == null) return;

        // Create scanline game object
        GameObject scanlineGo = new GameObject("Holo_Scanline");
        scanlineGo.transform.SetParent(transform, false);

        RectTransform scanlineRect = scanlineGo.AddComponent<RectTransform>();
        
        // Stretch horizontally across the button
        scanlineRect.anchorMin = new Vector2(0f, 0.5f);
        scanlineRect.anchorMax = new Vector2(1f, 0.5f);
        scanlineRect.pivot = new Vector2(0.5f, 0.5f);
        
        // Match height to a thin horizontal sci-fi scanline (e.g., 6 pixels)
        scanlineRect.sizeDelta = new Vector2(0f, 8f);
        scanlineRect.anchoredPosition = new Vector2(0f, _buttonRect.rect.height / 2f);

        _scanlineBar = scanlineGo.AddComponent<Image>();
        _scanlineBar.color = scanlineColor;
        _scanlineBar.raycastTarget = false; // Never block UI pointer inputs
    }

    private IEnumerator HologramAnimateAndLoadRoutine()
    {
        float elapsed = 0f;
        float scanlineProgress = 0.5f;

        while (elapsed < transitionDuration)
        {
            // We use unscaledDeltaTime for smooth UI transitions regardless of timescale
            float dt = Time.unscaledDeltaTime;
            elapsed += dt;

            float progress = Mathf.Clamp01(elapsed / transitionDuration);
            float overallAlpha = 1f - progress; // Linear fade out

            // --- 1. Hologram Visual Transition & Glitch Flicker ---
            bool isFlickerFrame = Random.value < 0.12f; // 12% chance to flicker on each frame
            float flickerAlphaMultiplier = isFlickerFrame ? Random.Range(0.15f, 0.45f) : 1f;

            foreach (var state in _buttonGraphics)
            {
                if (state.graphic == null) continue;

                // Shift graphic color towards the holographic electric cyan
                Color targetedBaseColor = Color.Lerp(state.originalColor, hologramColor, Mathf.Min(progress * 1.5f, 1f));
                
                // Set the final graphic color applying the fade out and flickering
                targetedBaseColor.a = targetedBaseColor.a * overallAlpha * flickerAlphaMultiplier;
                state.graphic.color = targetedBaseColor;
            }

            // --- 2. Micro-Jitter Position Glitches ---
            if (_buttonRect != null)
            {
                if (isFlickerFrame)
                {
                    // Random micro positional displacements to represent connection interference
                    float jitterX = Random.Range(-4f, 4f);
                    float jitterY = Random.Range(-3f, 3f);
                    _buttonRect.localPosition = _originalLocalPos + new Vector3(jitterX, jitterY, 0f);
                }
                else
                {
                    _buttonRect.localPosition = _originalLocalPos;
                }
            }

            // --- 3. Procedural Scanline Sweep ---
            if (_scanlineBar != null && _buttonRect != null)
            {
                // Animate sweep: cycle from top to bottom
                scanlineProgress -= dt * scanlineSweepSpeed;
                if (scanlineProgress < -0.5f)
                {
                    scanlineProgress = 0.5f; // Loop sweep
                }

                float buttonHeight = _buttonRect.rect.height;
                _scanlineBar.rectTransform.anchoredPosition = new Vector2(0f, scanlineProgress * buttonHeight);

                // Apply fade out directly to the scanline
                Color sColor = scanlineColor;
                sColor.a = sColor.a * overallAlpha * (isFlickerFrame ? 0.2f : 1f);
                _scanlineBar.color = sColor;
            }

            yield return null;
        }

        // --- Clean Up ---
        if (_scanlineBar != null)
        {
            Destroy(_scanlineBar.gameObject);
        }

        if (_buttonRect != null)
        {
            _buttonRect.localPosition = _originalLocalPos;
        }

        Debug.Log("[UIHolographicEffect] Hologram transition complete. Loading 'Preparation' scene...");

        // Load the Preparation scene
        SceneManager.LoadScene("Preparation");
    }

    private void OnDestroy()
    {
        // Safety: ensure local position is restored if object is destroyed mid-transition
        if (_buttonRect != null)
        {
            _buttonRect.localPosition = _originalLocalPos;
        }
    }
}
