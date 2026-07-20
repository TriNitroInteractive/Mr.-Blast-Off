using UnityEngine;
using UnityEngine.InputSystem;

public class SphericalCameraFollow : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform target;
    public Transform planet;

    [Header("Camera Control Settings")]
    [SerializeField] private float mouseSensitivity = 0.15f;
    [SerializeField] private float heightOffset = 1.5f;
    [SerializeField] private float distance = 8f;

    [Header("Top-Down Limits")]
    [SerializeField] private float minPitchAngle = 135f; // Tilted up to 45 degrees off straight down
    [SerializeField] private float maxPitchAngle = 175f; // 5 degrees off straight down

    [Header("Smoothness")]
    [SerializeField] private float positionLerpSpeed = 8f;
    [SerializeField] private float rotationLerpSpeed = 8f;

    [HideInInspector] public bool isCinematicActive = false;

    private Quaternion virtualRotation;
    private InputAction lookAction;

    private void Awake()
    {
        // Automatically find target "Mr.Blast" if not set
        if (target == null)
        {
            GameObject playerObj = GameObject.Find("Mr.Blast");
            if (playerObj != null)
            {
                target = playerObj.transform;
            }
        }

        // Automatically find planet "Planet_M" if not set
        if (planet == null)
        {
            GameObject planetObj = GameObject.Find("Planet_M");
            if (planetObj != null)
            {
                planet = planetObj.transform;
            }
        }
    }

    private void Start()
    {
        // Bind to the project-wide Input Actions "Look" action
        if (InputSystem.actions != null)
        {
            lookAction = InputSystem.actions.FindAction("Look");
            lookAction?.Enable();
        }

        // Initialize virtual rotation to look down from a high top-down angle (160 degrees pitch)
        Vector3 gravityUp = target != null ? (target.position - planet.position).normalized : Vector3.up;
        virtualRotation = Quaternion.LookRotation(-gravityUp) * Quaternion.Euler(20f, 0f, 0f);

        // Initialize distance based on current offset if not overridden
        if (target != null && distance <= 0.01f)
        {
            distance = Vector3.Distance(transform.position, target.position);
        }

        // Unparent so the camera moves smoothly and independently from player physics updates
        transform.SetParent(null);
    }

    private void LateUpdate()
    {
        if (isCinematicActive) return;
        if (target == null || planet == null) return;
        if (!target.gameObject.activeInHierarchy) return;

        // Calculate gravity direction at the player's position
        Vector3 gravityUp = (target.position - planet.position).normalized;

        // 1. Align virtual rotation's Up with gravityUp
        Quaternion alignRot = Quaternion.FromToRotation(virtualRotation * Vector3.up, gravityUp);
        virtualRotation = alignRot * virtualRotation;

        // 2. Read mouse look input
        Vector2 mouseInput = Vector2.zero;
        if (lookAction != null)
        {
            mouseInput = lookAction.ReadValue<Vector2>();
        }

        // 3. Apply Yaw (orbital rotation around the gravity Up axis)
        float yawInput = mouseInput.x * mouseSensitivity;
        virtualRotation = Quaternion.AngleAxis(yawInput, gravityUp) * virtualRotation;

        // 4. Apply Pitch (vertical tilt around the virtual camera's local Right axis)
        Vector3 virtualForward = virtualRotation * Vector3.forward;
        Vector3 virtualRight = virtualRotation * Vector3.right;

        // Angle between camera forward and gravityUp: 0 = looking up, 180 = looking down (top-down)
        float currentAngle = Vector3.Angle(virtualForward, gravityUp);
        // Clamp vertical viewing angle to keep a distinct top-down perspective
        float desiredAngle = Mathf.Clamp(currentAngle - mouseInput.y * mouseSensitivity, minPitchAngle, maxPitchAngle);
        float pitchAdjustment = desiredAngle - currentAngle;
        virtualRotation = Quaternion.AngleAxis(pitchAdjustment, virtualRight) * virtualRotation;

        // 5. Calculate target camera position behind its forward vector from target focus point
        Vector3 targetFocusPoint = target.position + gravityUp * heightOffset;
        Vector3 targetPosition = targetFocusPoint - (virtualRotation * Vector3.forward) * distance;

        // 6. Smoothly interpolate towards the target position and orientation
        transform.position = Vector3.Lerp(transform.position, targetPosition, positionLerpSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, virtualRotation, rotationLerpSpeed * Time.deltaTime);
    }
}
