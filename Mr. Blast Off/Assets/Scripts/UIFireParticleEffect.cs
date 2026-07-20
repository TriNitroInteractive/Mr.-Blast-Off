using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Procedural C#-based UI Image particle emission that floats upwards, color-transitions
/// from electric yellow to orange to red, fades out, and applies a progressively
/// intensifying screenshake to the target button. After 5 seconds, it loads the 'Preparation' scene.
/// </summary>
public class UIFireParticleEffect : MonoBehaviour
{
    [Header("Target References")]
    [Tooltip("The button (RectTransform) that will emit fire and shake. If null, will default to this component's RectTransform.")]
    [SerializeField] private RectTransform targetButton;

    [Tooltip("The parent Canvas or panel to spawn particles under. If null, will search for a parent Canvas.")]
    [SerializeField] private Transform particleParent;

    [Header("Particle Settings")]
    [Tooltip("Number of particles spawned per second.")]
    [SerializeField] private float emissionRate = 60f;

    [Tooltip("How long each particle lives in seconds.")]
    [SerializeField] private float particleMinLifetime = 0.4f;
    [SerializeField] private float particleMaxLifetime = 1.0f;

    [Tooltip("Initial upward speed range of particles.")]
    [SerializeField] private float minUpwardSpeed = 100f;
    [SerializeField] private float maxUpwardSpeed = 250f;

    [Tooltip("Initial horizontal drift speed range of particles.")]
    [SerializeField] private float maxHorizontalDrift = 50f;

    [Tooltip("Particle size range.")]
    [SerializeField] private float minSize = 10f;
    [SerializeField] private float maxSize = 25f;

    [Header("Screenshake Settings")]
    [Tooltip("Maximum screenshake magnitude reached at the end of 5 seconds.")]
    [SerializeField] private float maxShakeMagnitude = 15f;

    private Vector3 _originalButtonLocalPos;
    private bool _isEffectActive = false;
    private float _effectTimer = 0f;
    private const float EffectDuration = 5.0f;

    // Track active particles
    private List<ActiveParticle> _activeParticles = new List<ActiveParticle>();
    private float _spawnAccumulator = 0f;

    private struct ActiveParticle
    {
        public GameObject gameObject;
        public RectTransform rectTransform;
        public Image image;
        public Vector2 velocity;
        public float lifetime;
        public float age;
        public float startSize;
    }

    private void Awake()
    {
        if (targetButton == null)
        {
            targetButton = GetComponent<RectTransform>();
        }

        if (particleParent == null)
        {
            // Find parent Canvas
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                particleParent = canvas.transform;
            }
            else
            {
                particleParent = transform.parent != null ? transform.parent : transform;
            }
        }
    }

    /// <summary>
    /// Starts the ignition fire effect, screenshake, and 5-second transition countdown.
    /// </summary>
    public void StartIgnitionEffect()
    {
        if (_isEffectActive) return;

        Debug.Log("[UIFireParticleEffect] Rocket core igniting! Commencing fire particle emission and cockpit vibration...");

        if (targetButton != null)
        {
            _originalButtonLocalPos = targetButton.localPosition;
        }

        _isEffectActive = true;
        _effectTimer = 0f;
        _spawnAccumulator = 0f;

        StartCoroutine(EffectCountdownRoutine());
    }

    private void Update()
    {
        if (!_isEffectActive) return;

        float dt = Time.deltaTime;
        _effectTimer += dt;

        // 1. Procedural particle spawning
        _spawnAccumulator += dt * emissionRate;
        while (_spawnAccumulator >= 1.0f)
        {
            SpawnParticle();
            _spawnAccumulator -= 1.0f;
        }

        // 2. Update and animate active particles
        UpdateParticles(dt);

        // 3. Progressively intensifying screenshake on the button
        if (targetButton != null && _effectTimer < EffectDuration)
        {
            // Normalized progression of the effect (0 to 1)
            float t = _effectTimer / EffectDuration;
            
            // Intensify progressively with an exponential curve for a more dramatic buildup
            float currentIntensity = Mathf.Pow(t, 2f) * maxShakeMagnitude;

            // Generate shake offset
            Vector2 shakeOffset = Random.insideUnitCircle * currentIntensity;
            targetButton.localPosition = _originalButtonLocalPos + new Vector3(shakeOffset.x, shakeOffset.y, 0f);
        }
    }

    private void SpawnParticle()
    {
        if (targetButton == null || particleParent == null) return;

        // Create particle game object and setup RectTransform
        GameObject particleGo = new GameObject("UIFireParticle");
        particleGo.transform.SetParent(particleParent, false);

        RectTransform pRect = particleGo.AddComponent<RectTransform>();
        
        // Match anchoring to target button center for correct relative placement
        pRect.anchorMin = targetButton.anchorMin;
        pRect.anchorMax = targetButton.anchorMax;
        pRect.pivot = new Vector2(0.5f, 0.5f);

        // Position slightly randomly around the button's current world position
        Vector3 spawnWorldPos = targetButton.position + (Vector3)(Random.insideUnitCircle * (targetButton.rect.width * 0.4f));
        pRect.position = spawnWorldPos;

        // Setup size
        float size = Random.Range(minSize, maxSize);
        pRect.sizeDelta = new Vector2(size, size);

        // Add Image component to represent the fire particle
        Image img = particleGo.AddComponent<Image>();
        img.raycastTarget = false; // Best practice: do not block UI interactions

        // Determine particle velocity: floating upwards (positive Y) with slight horizontal drift
        Vector2 velocity = new Vector2(
            Random.Range(-maxHorizontalDrift, maxHorizontalDrift),
            Random.Range(minUpwardSpeed, maxUpwardSpeed)
        );

        _activeParticles.Add(new ActiveParticle
        {
            gameObject = particleGo,
            rectTransform = pRect,
            image = img,
            velocity = velocity,
            lifetime = Random.Range(particleMinLifetime, particleMaxLifetime),
            age = 0f,
            startSize = size
        });
    }

    private void UpdateParticles(float dt)
    {
        for (int i = _activeParticles.Count - 1; i >= 0; i--)
        {
            ActiveParticle p = _activeParticles[i];
            p.age += dt;

            if (p.age >= p.lifetime)
            {
                Destroy(p.gameObject);
                _activeParticles.RemoveAt(i);
                continue;
            }

            // Move particle
            p.rectTransform.anchoredPosition += p.velocity * dt;

            // Rotate particle slightly for organic visual variance
            p.rectTransform.Rotate(0f, 0f, 180f * dt);

            // Compute normalized age
            float t = p.age / p.lifetime;

            // Shrink particle over its life
            float currentSize = Mathf.Lerp(p.startSize, 2f, t);
            p.rectTransform.sizeDelta = new Vector2(currentSize, currentSize);

            // Color transition: Electric Yellow -> Orange -> Red and Fade Out
            Color pColor = Color.white;
            if (t < 0.35f)
            {
                // Electric Yellow to bright Orange
                pColor = Color.Lerp(new Color(1f, 1f, 0.2f, 1f), new Color(1f, 0.5f, 0f, 1f), t / 0.35f);
            }
            else if (t < 0.75f)
            {
                // Orange to deep Red
                float normalizedMid = (t - 0.35f) / 0.40f;
                pColor = Color.Lerp(new Color(1f, 0.5f, 0f, 1f), new Color(0.9f, 0.1f, 0f, 1f), normalizedMid);
            }
            else
            {
                // Red fading out completely
                float normalizedEnd = (t - 0.75f) / 0.25f;
                pColor = Color.Lerp(new Color(0.9f, 0.1f, 0f, 1f), new Color(0.5f, 0f, 0f, 0f), normalizedEnd);
            }

            p.image.color = pColor;

            // Re-assign struct since it's a value type in the list
            _activeParticles[i] = p;
        }
    }

    private IEnumerator EffectCountdownRoutine()
    {
        // Execute active sequence for 5 seconds
        yield return new WaitForSeconds(EffectDuration);

        // Turn off effect to stop updating
        _isEffectActive = false;

        // Clean up any remaining particles
        foreach (var p in _activeParticles)
        {
            if (p.gameObject != null)
            {
                Destroy(p.gameObject);
            }
        }
        _activeParticles.Clear();

        // Restore button back to original position
        if (targetButton != null)
        {
            targetButton.localPosition = _originalButtonLocalPos;
        }

        Debug.Log("[UIFireParticleEffect] Countdown finished! Launching shuttle and loading 'Preparation' scene...");

        // Load the Preparation scene
        SceneManager.LoadScene("Preparation");
    }

    private void OnDestroy()
    {
        // Safety cleanup of any dynamically spawned particles
        foreach (var p in _activeParticles)
        {
            if (p.gameObject != null)
            {
                Destroy(p.gameObject);
            }
        }
    }
}
