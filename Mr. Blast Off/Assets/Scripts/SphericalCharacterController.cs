using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class SphericalCharacterController : MonoBehaviour
{
    [Header("Gravity Settings")]
    public Transform planet;
    [SerializeField] private float gravityConstant = -9.81f;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float alignmentSpeed = 15f;
    [SerializeField] private float acceleration = 25f;
    [SerializeField] private float deceleration = 30f;

    private Rigidbody rb;
    private InputAction moveAction;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        // Automatically find the planet if not assigned
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
        // Bind to the project-wide Input Actions "Move" action
        if (InputSystem.actions != null)
        {
            moveAction = InputSystem.actions.FindAction("Move");
            moveAction?.Enable();
        }
    }

    private void FixedUpdate()
    {
        if (planet == null) return;

        // 1. Calculate gravity direction
        Vector3 gravityUp = (transform.position - planet.position).normalized;

        // 2. Read movement input
        Vector2 input = Vector2.zero;
        if (moveAction != null)
        {
            input = moveAction.ReadValue<Vector2>();
        }

        // 3. Move relative to camera forward & right projected on the sphere surface
        Vector3 localMoveDir = Vector3.zero;
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            Vector3 camForward = mainCam.transform.forward;
            Vector3 camRight = mainCam.transform.right;

            Vector3 moveForward = Vector3.ProjectOnPlane(camForward, gravityUp).normalized;
            Vector3 moveRight = Vector3.ProjectOnPlane(camRight, gravityUp).normalized;

            localMoveDir = moveForward * input.y + moveRight * input.x;
        }
        else
        {
            localMoveDir = transform.forward * input.y + transform.right * input.x;
        }
        
        // Ensure direction is tangent to the surface normal
        localMoveDir = Vector3.ProjectOnPlane(localMoveDir, gravityUp).normalized;

        // Unified single-pass rotation Slerp
        Quaternion targetRotation;
        float currentRotationSpeed;

        if (input.magnitude > 0.01f && localMoveDir.sqrMagnitude > 0.001f)
        {
            // If moving, face the move direction and align with gravity
            targetRotation = Quaternion.LookRotation(localMoveDir, gravityUp);
            currentRotationSpeed = rotationSpeed;
        }
        else
        {
            // If stationary, keep current forward direction and align with gravity
            Vector3 currentForward = Vector3.ProjectOnPlane(transform.forward, gravityUp).normalized;
            if (currentForward.sqrMagnitude < 0.001f)
            {
                currentForward = transform.forward;
            }
            targetRotation = Quaternion.LookRotation(currentForward, gravityUp);
            currentRotationSpeed = alignmentSpeed;
        }

        Quaternion newRotation = Quaternion.Slerp(transform.rotation, targetRotation, currentRotationSpeed * Time.fixedDeltaTime);
        rb.MoveRotation(newRotation);

        // Smoothly accelerate / decelerate horizontal velocity
        Vector3 currentHorizontalVelocity = Vector3.ProjectOnPlane(rb.linearVelocity, gravityUp);
        Vector3 targetHorizontalVelocity = Vector3.zero;

        if (input.magnitude > 0.01f && localMoveDir.sqrMagnitude > 0.001f)
        {
            targetHorizontalVelocity = localMoveDir * moveSpeed;
        }

        float speedFactor = (input.magnitude > 0.01f) ? acceleration : deceleration;
        Vector3 newHorizontalVelocity = Vector3.MoveTowards(currentHorizontalVelocity, targetHorizontalVelocity, speedFactor * Time.fixedDeltaTime);

        // 4. Integrate gravity manually into the vertical velocity component
        float currentVerticalVelocity = Vector3.Dot(rb.linearVelocity, gravityUp);
        currentVerticalVelocity += gravityConstant * Time.fixedDeltaTime;

        // Combine horizontal and vertical velocities and apply to Rigidbody
        rb.linearVelocity = newHorizontalVelocity + gravityUp * currentVerticalVelocity;
    }
}
