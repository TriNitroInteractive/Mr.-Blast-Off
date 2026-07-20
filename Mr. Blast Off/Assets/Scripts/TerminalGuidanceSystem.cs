using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A dynamic, neon-colored guidance system that draws a circular geodesic dotted path on the planet's surface
/// connecting the player directly to the planet's control terminal.
/// - Operates dynamically only when the player is on foot.
/// - Calculates a curved spherical projection path that hugs the planet's surface.
/// - Periodically spawns larger, pulsing "runner dots" that travel from the player to the terminal, directing the way.
/// - Smoothly fades out as the player approaches the terminal to keep the interaction area clean.
/// </summary>
[ExecuteAlways]
public class TerminalGuidanceSystem : MonoBehaviour
{
    [Header("Visual Style Settings")]
    [Tooltip("Base color for the dotted line (Neon Cyan).")]
    [SerializeField] private Color pathColor = new Color(0.0f, 1.8f, 2.0f, 1.0f); // High intensity HDR Cyan

    [Tooltip("Color for the animated runner dots (Neon Yellow/Orange).")]
    [SerializeField] private Color runnerColor = new Color(2.0f, 1.5f, 0.0f, 1.0f); // High intensity HDR Yellow

    [Header("Exposed Sizing Customizations")]
    [Tooltip("The local scale size of the static path dots.")]
    [SerializeField] private float pathDotSize = 0.18f;

    [Tooltip("The local scale size of the traveling runner dots.")]
    [SerializeField] private float runnerDotSize = 0.65f;

    [Tooltip("Distance threshold (units) within which the line fades/deactivates to stay clean.")]
    [SerializeField] private float hideProximity = 1.8f;

    [Header("Path Spacing Settings")]
    [Tooltip("Total number of static dots used to trace the geodesic path.")]
    [SerializeField] private int pathDotCount = 35;

    [Tooltip("Distance above the planet's surface to draw the dots to prevent clipping.")]
    [SerializeField] private float altitudeOffset = 0.5f;

    [Header("Runner Animation Settings")]
    [Tooltip("Speed at which runner dots travel along the path (0 to 1 progress per second).")]
    [SerializeField] private float runnerSpeed = 0.35f;

    [Tooltip("How many runner dots are in the traveling group wave.")]
    [SerializeField] private int runnerGroupSize = 4;

    [Tooltip("Spacing/Delay between consecutive runner dots in the group wave (progress delta).")]
    [SerializeField] private float runnerSpacing = 0.04f;

    [Tooltip("Interval in seconds between consecutive runner waves starting from the player.")]
    [SerializeField] private float waveInterval = 3.0f;

    [Header("Flight Guidance")]
    [Tooltip("Show the guidance path projected on the surface underneath the Shuttle while flying in space.")]
    [SerializeField] private bool showInShuttle = true;

    private SphericalCharacterController _playerController;
    private ShuttleController _shuttleController;
    private List<TerminalController> _terminals = new List<TerminalController>();

    // Object Pooling
    private GameObject[] _pathDotPool;
    private GameObject[] _runnerDotPool;
    private Material _pathMaterial;
    private Material _runnerMaterial;

    private float _waveTimer = 0.0f;
    private float _runnerProgressOffset = 0.0f;
    private bool _isWaveActive = false;

    private void Start()
    {
        FindReferences();
        SetupMaterials();
        InitializePools();
    }

    private void FindReferences()
    {
        // 1. Locate the player character
        GameObject playerGo = GameObject.Find("Mr.Blast");
        if (playerGo != null)
        {
            _playerController = playerGo.GetComponent<SphericalCharacterController>();
        }

        // 2. Locate the shuttle controller
        GameObject shuttleGo = GameObject.Find("Shuttle");
        if (shuttleGo != null)
        {
            _shuttleController = shuttleGo.GetComponent<ShuttleController>();
        }

        // 3. Locate all terminals in the scene
        _terminals.Clear();
        var terminalsInScene = Object.FindObjectsByType<TerminalController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        _terminals.AddRange(terminalsInScene);
    }

    private void SetupMaterials()
    {
        // Destroy existing material instances to prevent memory leaks if re-running Setup
        if (_pathMaterial != null) { DestroyImmediate(_pathMaterial); _pathMaterial = null; }
        if (_runnerMaterial != null) { DestroyImmediate(_runnerMaterial); _runnerMaterial = null; }

        // Try locating a standard URP Unlit shader
        Shader unlitShader = Shader.Find("Universal Render Pipeline/Unlit");
        if (unlitShader == null) unlitShader = Shader.Find("Particles/Standard Unlit");
        if (unlitShader == null) unlitShader = Shader.Find("Sprites/Default");
        if (unlitShader == null) unlitShader = Shader.Find("Hidden/InternalErrorShader");

        if (unlitShader != null)
        {
            _pathMaterial = new Material(unlitShader);
            _pathMaterial.SetColor("_BaseColor", pathColor);
            _pathMaterial.SetColor("_Color", pathColor);

            _runnerMaterial = new Material(unlitShader);
            _runnerMaterial.SetColor("_BaseColor", runnerColor);
            _runnerMaterial.SetColor("_Color", runnerColor);
        }
    }

    private void InitializePools()
    {
        ClearPools();

        // Create Path Dot Pool (Small Spheres)
        _pathDotPool = new GameObject[pathDotCount];
        for (int i = 0; i < pathDotCount; i++)
        {
            GameObject dot = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            dot.name = $"PathDot_{i}";
            dot.transform.SetParent(transform, false);
            dot.transform.localScale = Vector3.one * pathDotSize;

            // Remove sphere collider to optimize physics ticks
            var col = dot.GetComponent<Collider>();
            if (col != null)
            {
                if (Application.isPlaying) Destroy(col);
                else DestroyImmediate(col);
            }

            var mr = dot.GetComponent<MeshRenderer>();
            if (mr != null) mr.sharedMaterial = _pathMaterial;

            dot.SetActive(false);
            _pathDotPool[i] = dot;
        }

        // Create Runner Dot Pool (Larger Spheres)
        _runnerDotPool = new GameObject[runnerGroupSize];
        for (int i = 0; i < runnerGroupSize; i++)
        {
            GameObject runner = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            runner.name = $"RunnerDot_{i}";
            runner.transform.SetParent(transform, false);
            runner.transform.localScale = Vector3.one * runnerDotSize;

            var col = runner.GetComponent<Collider>();
            if (col != null)
            {
                if (Application.isPlaying) Destroy(col);
                else DestroyImmediate(col);
            }

            var mr = runner.GetComponent<MeshRenderer>();
            if (mr != null) mr.sharedMaterial = _runnerMaterial;

            runner.SetActive(false);
            _runnerDotPool[i] = runner;
        }
    }

    private void LateUpdate()
    {
        // Resilient dynamically-fetched references for EditMode or hotloaded states
        if (_playerController == null || _shuttleController == null || _terminals.Count == 0)
        {
            FindReferences();
        }

        // Ensure pools are validly allocated
        if (_pathDotPool == null || _pathDotPool.Length != pathDotCount || _pathDotPool[0] == null ||
            _runnerDotPool == null || _runnerDotPool.Length != runnerGroupSize || _runnerDotPool[0] == null)
        {
            SetupMaterials();
            InitializePools();
        }

        // 1. If blast is commanded or game over, deactivate
        if (Application.isPlaying && GameManager.Instance != null && (GameManager.Instance.IsCountdownActive || GameManager.Instance.IsGameOver))
        {
            HideAllDots();
            return;
        }

        // 2. If inside the Shuttle (piloted), deactivate
        if (Application.isPlaying && _shuttleController != null && _shuttleController.gameObject.activeInHierarchy && _shuttleController.isPiloted)
        {
            HideAllDots();
            return;
        }

        Vector3 trackingPosition = Vector3.zero;
        Transform activePlanet = null;

        // DUAL-STATE EVALUATION:
        // A. If player is active in hierarchy on-foot, trace path from character's feet
        if (_playerController != null && _playerController.gameObject.activeInHierarchy && _playerController.enabled)
        {
            trackingPosition = _playerController.transform.position;
            activePlanet = _playerController.planet;
        }
        // B. If player is inside the Shuttle (piloted), project the Shuttle position onto the closest planet surface
        else if (showInShuttle && _shuttleController != null && _shuttleController.gameObject.activeInHierarchy && _shuttleController.isPiloted)
        {
            activePlanet = FindNearestPlanetToShuttle(_shuttleController.transform.position);
            if (activePlanet != null)
            {
                float radius = GetPlanetRadius(activePlanet.gameObject);
                Vector3 offsetDir = (_shuttleController.transform.position - activePlanet.position).normalized;
                trackingPosition = activePlanet.position + offsetDir * radius; // Projected point on sphere surface!
            }
        }
        // C. Fallback for EditMode preview (if not playing, trace path from shuttle location on Planet M)
        else if (!Application.isPlaying)
        {
            GameObject previewPlayer = GameObject.Find("Mr.Blast");
            if (previewPlayer != null)
            {
                trackingPosition = previewPlayer.transform.position;
                var cc = previewPlayer.GetComponent<SphericalCharacterController>();
                activePlanet = cc != null ? cc.planet : null;
            }

            if (activePlanet == null)
            {
                GameObject pM = GameObject.Find("Planet M");
                if (pM != null) activePlanet = pM.transform;
            }
        }

        // Exit cleanly if no active coordinates can be resolved
        if (activePlanet == null || trackingPosition == Vector3.zero)
        {
            HideAllDots();
            return;
        }

        // 1. Locate the nearest Terminal to this planet
        TerminalController targetTerminal = FindTerminalForPlanet(activePlanet);
        if (targetTerminal == null)
        {
            HideAllDots();
            return;
        }

        // Calculate dynamic properties
        Vector3 planetCenter = activePlanet.position;
        float planetRadius = GetPlanetRadius(activePlanet.gameObject);
        float guideAltitude = planetRadius + altitudeOffset;

        Vector3 terminalPos = targetTerminal.transform.position;

        // Verify distance: If extremely close, hide path to keep interaction clean and unobstructed
        float distanceToTerminal = Vector3.Distance(trackingPosition, terminalPos);
        if (distanceToTerminal < hideProximity)
        {
            HideAllDots();
            return;
        }

        // 2. Project Geodesic Path & Position Static Dots
        for (int i = 0; i < pathDotCount; i++)
        {
            float t = (float)i / (pathDotCount - 1);
            Vector3 worldPos = CalculateGeodesicPoint(trackingPosition, terminalPos, planetCenter, guideAltitude, t);

            _pathDotPool[i].transform.position = worldPos;
            _pathDotPool[i].SetActive(true);
        }

        // 3. Animate and Position Runner Dot Waves
        AnimateRunners(trackingPosition, terminalPos, planetCenter, guideAltitude);
    }

    private void AnimateRunners(Vector3 start, Vector3 end, Vector3 center, float altitude)
    {
        // Avoid running animations in Edit Mode to prevent dirtying scene cycles
        if (!Application.isPlaying)
        {
            foreach (var r in _runnerDotPool)
            {
                if (r != null) r.SetActive(false);
            }
            return;
        }

        _waveTimer += Time.deltaTime;

        // Trigger a new wave periodically
        if (!_isWaveActive && _waveTimer >= waveInterval)
        {
            _waveTimer = 0.0f;
            _runnerProgressOffset = 0.0f;
            _isWaveActive = true;
        }

        if (_isWaveActive)
        {
            _runnerProgressOffset += Time.deltaTime * runnerSpeed;

            bool allFinished = true;

            for (int i = 0; i < runnerGroupSize; i++)
            {
                // Each runner in the wave travels with a cascading spacing offset
                float individualT = _runnerProgressOffset - (i * runnerSpacing);

                if (individualT >= 0f && individualT <= 1f)
                {
                    Vector3 worldPos = CalculateGeodesicPoint(start, end, center, altitude, individualT);
                    _runnerDotPool[i].transform.position = worldPos;
                    _runnerDotPool[i].SetActive(true);
                    allFinished = false;
                }
                else
                {
                    if (_runnerDotPool[i] != null) _runnerDotPool[i].SetActive(false);
                }
            }

            if (allFinished && _runnerProgressOffset > 1.0f)
            {
                _isWaveActive = false;
                _waveTimer = 0.0f; // Wait for interval before next wave
            }
        }
        else
        {
            // Deactivate runners during idle interval
            foreach (var r in _runnerDotPool)
            {
                if (r != null) r.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Computes a projected circular point on the sphere surface.
    /// Uses linear interpolation projected outwards from center to represent a perfect geodesic great-circle path.
    /// </summary>
    private Vector3 CalculateGeodesicPoint(Vector3 p1, Vector3 p2, Vector3 center, float radius, float t)
    {
        // Linear interpolation cut through sphere interior
        Vector3 lerpedPos = Vector3.Lerp(p1, p2, t);

        // Project vector outwards from planet center
        Vector3 directionFromCenter = (lerpedPos - center).normalized;

        // Position on surface boundary at calculated altitude
        return center + directionFromCenter * radius;
    }

    private TerminalController FindTerminalForPlanet(Transform planet)
    {
        TerminalController closest = null;
        float minDist = float.MaxValue;

        foreach (var term in _terminals)
        {
            if (term == null) continue;
            float dist = Vector3.Distance(term.transform.position, planet.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = term;
            }
        }

        return closest;
    }

    private Transform FindNearestPlanetToShuttle(Vector3 shuttlePos)
    {
        Transform closest = null;
        float minDist = float.MaxValue;

        // Find all active planets
        var allObjects = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach (var go in allObjects)
        {
            if (go.name.StartsWith("Planet"))
            {
                float dist = Vector3.Distance(shuttlePos, go.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    closest = go.transform;
                }
            }
        }

        return closest;
    }

    private float GetPlanetRadius(GameObject planetGo)
    {
        // Check for sphere collider
        var sphereCol = planetGo.GetComponent<SphereCollider>();
        if (sphereCol != null)
        {
            return sphereCol.radius * planetGo.transform.localScale.x;
        }

        // Fallback to mesh bounds if available
        var meshFilters = planetGo.GetComponentsInChildren<MeshFilter>(true);
        if (meshFilters.Length > 0)
        {
            float maxDist = 0f;
            foreach (var filter in meshFilters)
            {
                if (filter.sharedMesh == null) continue;
                Vector3 boundsSize = filter.sharedMesh.bounds.extents;
                float dist = Mathf.Max(boundsSize.x, boundsSize.y, boundsSize.z) * planetGo.transform.localScale.x;
                if (dist > maxDist) maxDist = dist;
            }
            return maxDist;
        }

        // Final safe default based on general planet scale
        return planetGo.transform.localScale.x * 0.5f;
    }

    private void HideAllDots()
    {
        if (_pathDotPool != null)
        {
            foreach (var dot in _pathDotPool)
            {
                if (dot != null) dot.SetActive(false);
            }
        }

        if (_runnerDotPool != null)
        {
            foreach (var r in _runnerDotPool)
            {
                if (r != null) r.SetActive(false);
            }
        }
    }

    private void ClearPools()
    {
        if (_pathDotPool != null)
        {
            foreach (var dot in _pathDotPool)
            {
                if (dot != null)
                {
                    if (Application.isPlaying) Destroy(dot);
                    else DestroyImmediate(dot);
                }
            }
            _pathDotPool = null;
        }

        if (_runnerDotPool != null)
        {
            foreach (var r in _runnerDotPool)
            {
                if (r != null)
                {
                    if (Application.isPlaying) Destroy(r);
                    else DestroyImmediate(r);
                }
            }
            _runnerDotPool = null;
        }
    }

    private void OnDisable()
    {
        ClearPools();
        // Destroy procedural materials to prevent graphic memory leaks
        if (_pathMaterial != null) { DestroyImmediate(_pathMaterial); _pathMaterial = null; }
        if (_runnerMaterial != null) { DestroyImmediate(_runnerMaterial); _runnerMaterial = null; }
    }

    private void OnDestroy()
    {
        ClearPools();
        if (_pathMaterial != null) { DestroyImmediate(_pathMaterial); _pathMaterial = null; }
        if (_runnerMaterial != null) { DestroyImmediate(_runnerMaterial); _runnerMaterial = null; }
    }
}
