using UnityEngine;

/// <summary>
/// Procedurally generates a glowing, twinkling 3D starfield particle system.
/// Centers around the active camera during LateUpdate to give the illusion of infinite distance.
/// </summary>
public class StarfieldGenerator : MonoBehaviour
{
    [Header("Starfield Settings")]
    [Tooltip("Total number of stars in the sky.")]
    [SerializeField] private int starCount = 800;

    [Tooltip("The outer shell radius of the starfield sphere.")]
    [SerializeField] private float starfieldRadius = 450f;

    [Tooltip("Size range of individual stars.")]
    [SerializeField] private float minStarSize = 0.4f;
    [SerializeField] private float maxStarSize = 1.6f;

    [Header("Twinkle Animation")]
    [Tooltip("How fast the stars twinkle/pulsate.")]
    [SerializeField] private float twinkleSpeed = 1.5f;

    [Tooltip("Depth of twinkle alpha pulsation (0 = none, 1 = maximum flicker).")]
    [Range(0f, 1f)]
    [SerializeField] private float twinkleIntensity = 0.5f;

    private ParticleSystem _particleSystem;
    private ParticleSystem.Particle[] _stars;
    private float[] _twinklePhases;
    private float[] _baseAlphas;

    private Camera _mainCam;
    private Camera _secondaryCam;

    private void Start()
    {
        _mainCam = Camera.main;
        GameObject secCamGo = GameObject.Find("SecondaryCamera");
        if (secCamGo != null)
        {
            _secondaryCam = secCamGo.GetComponent<Camera>();
        }

        GenerateStarfield();
    }

    private void GenerateStarfield()
    {
        // 1. Create a dynamic child GameObject for the particle system
        GameObject psGo = new GameObject("Procedural_Starfield_PS");
        psGo.transform.SetParent(transform, false);

        _particleSystem = psGo.AddComponent<ParticleSystem>();
        var renderer = psGo.GetComponent<ParticleSystemRenderer>();

        // 2. Disable default emission and modules we don't need
        var main = _particleSystem.main;
        main.loop = false;
        main.playOnAwake = false;
        main.simulationSpace = ParticleSystemSimulationSpace.Local; // Crucial for moving with camera
        
        var emission = _particleSystem.emission;
        emission.enabled = false;

        var shape = _particleSystem.shape;
        shape.enabled = false;

        // 3. Configure Renderer
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        
        // Find URP unlit particle shader, fallback to any unlit or default particle shader if not found
        Shader particleShader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (particleShader == null) particleShader = Shader.Find("Particles/Standard Unlit");
        if (particleShader == null) particleShader = Shader.Find("Sprites/Default");
        if (particleShader == null) particleShader = Shader.Find("Hidden/InternalErrorShader");

        if (particleShader != null)
        {
            Material starMaterial = new Material(particleShader);
            // Setup color and additive settings
            starMaterial.SetColor("_BaseColor", Color.white);
            starMaterial.SetColor("_Color", Color.white);
            renderer.sharedMaterial = starMaterial;
        }

        // 4. Pre-populate particle arrays
        _stars = new ParticleSystem.Particle[starCount];
        _twinklePhases = new float[starCount];
        _baseAlphas = new float[starCount];

        for (int i = 0; i < starCount; i++)
        {
            // Position on a massive random sphere shell
            Vector3 spawnDir = Random.onUnitSphere;
            Vector3 localPos = spawnDir * starfieldRadius;

            _stars[i].position = localPos;
            _stars[i].startSize = Random.Range(minStarSize, maxStarSize);
            _stars[i].remainingLifetime = 99999f;
            _stars[i].startLifetime = 99999f;

            // Randomize starting color/opacity (gives realistic depth)
            float baseAlpha = Random.Range(0.3f, 1.0f);
            _baseAlphas[i] = baseAlpha;

            // Give pale yellow, pure white, and soft light blue space tints
            float tintVal = Random.value;
            Color starColor = Color.white;
            if (tintVal < 0.15f) starColor = new Color(0.9f, 0.95f, 1f); // Pale blue star
            else if (tintVal < 0.25f) starColor = new Color(1f, 0.98f, 0.8f); // Soft yellow dwarf
            
            starColor.a = baseAlpha;
            _stars[i].startColor = starColor;

            // Assign individual twinkle phase offsets
            _twinklePhases[i] = Random.Range(0f, Mathf.PI * 2f);
        }

        // Apply particles to system
        _particleSystem.SetParticles(_stars, starCount);
        Debug.LogFormat("[StarfieldGenerator] Successfully initialized procedural starfield with {0} stars.", starCount);
    }

    private void LateUpdate()
    {
        if (_particleSystem == null) return;

        // 1. Position the starfield exactly over the active camera to maintain the skybox parallax illusion
        Camera activeCam = null;
        if (_mainCam != null && _mainCam.isActiveAndEnabled)
        {
            activeCam = _mainCam;
        }
        else if (_secondaryCam != null && _secondaryCam.isActiveAndEnabled)
        {
            activeCam = _secondaryCam;
        }
        else
        {
            // Fallback: try finding main camera again dynamically
            _mainCam = Camera.main;
            if (_mainCam != null) activeCam = _mainCam;
        }

        if (activeCam != null)
        {
            transform.position = activeCam.transform.position;
        }

        // 2. Animate star twinkling (alpha fluctuation over time)
        float time = Time.unscaledTime * twinkleSpeed;
        for (int i = 0; i < starCount; i++)
        {
            float sineValue = Mathf.Sin(time + _twinklePhases[i]);
            // Map sine [-1, 1] to fluctuation range based on intensity
            float alphaScale = 1f - (twinkleIntensity * 0.5f) + (sineValue * twinkleIntensity * 0.5f);
            
            Color c = _stars[i].startColor;
            c.a = _baseAlphas[i] * alphaScale;
            _stars[i].startColor = c;
        }

        // Push particle updates back to the system
        _particleSystem.SetParticles(_stars, starCount);
    }
}
