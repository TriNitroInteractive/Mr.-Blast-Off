using UnityEngine;

/// <summary>
/// Controls slowly rotating planets in outer space.
/// - Keeps planets rotating for atmospheric depth.
/// - Dynamically pauses rotation if the player is currently standing on this planet.
/// - Dynamically pauses rotation if the player is piloting the shuttle and this is the closest planet (landing/orbiting).
/// This prevents any character movement drift, physics slipping, or docking alignment errors.
/// </summary>
public class PlanetRotator : MonoBehaviour
{
    [Header("Rotation Settings")]
    [Tooltip("Speed of planet rotation on its local axis.")]
    [SerializeField] private float rotationSpeed = 3.5f;

    [Tooltip("The axis of local rotation. Default is Up (Y axis).")]
    [SerializeField] private Vector3 rotationAxis = Vector3.up;

    private SphericalCharacterController _playerController;
    private ShuttleController _shuttleController;
    private Transform _shuttleTransform;

    private void Start()
    {
        // 1. Locate player and its controller
        GameObject playerGo = GameObject.Find("Mr.Blast");
        if (playerGo != null)
        {
            _playerController = playerGo.GetComponent<SphericalCharacterController>();
        }

        // 2. Locate shuttle and its controller
        GameObject shuttleGo = GameObject.Find("Shuttle");
        if (shuttleGo != null)
        {
            _shuttleController = shuttleGo.GetComponent<ShuttleController>();
            _shuttleTransform = shuttleGo.transform;
        }
    }

    private void Update()
    {
        bool shouldRotate = true;

        // --- Check Case A: Player is on foot on this planet ---
        if (_playerController != null && _playerController.gameObject.activeInHierarchy)
        {
            if (_playerController.enabled && _playerController.planet == transform)
            {
                shouldRotate = false; // Pause rotation while player walks on surface
            }
        }

        // --- Check Case B: Player is piloting shuttle and this is the closest planet ---
        if (shouldRotate && _shuttleController != null && _shuttleController.gameObject.activeInHierarchy && _shuttleController.isPiloted)
        {
            Transform closestPlanet = FindClosestPlanetToShuttle();
            if (closestPlanet == transform)
            {
                shouldRotate = false; // Pause rotation of the landing/hover target
            }
        }

        // Apply rotation if no pause overrides are triggered
        if (shouldRotate)
        {
            transform.Rotate(rotationAxis, rotationSpeed * Time.deltaTime, Space.Self);
        }
    }

    /// <summary>
    /// Computes the closest planet transform relative to the Shuttle.
    /// </summary>
    private Transform FindClosestPlanetToShuttle()
    {
        if (_shuttleTransform == null) return null;

        Transform closest = null;
        float minDist = float.MaxValue;

        // Find all active planets in the scene starting with "Planet"
        var allObjects = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach (var go in allObjects)
        {
            if (go.name.StartsWith("Planet"))
            {
                float dist = Vector3.Distance(_shuttleTransform.position, go.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    closest = go.transform;
                }
            }
        }

        return closest;
    }
}
