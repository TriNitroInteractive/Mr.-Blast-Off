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

    private Rigidbody rb;
    private InputAction moveAction;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.freezeRotation = true;

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

        // Align the player's Up vector to gravityUp
        Quaternion targetRotation = Quaternion.FromToRotation(transform.up, gravityUp) * transform.rotation;
        Quaternion newRotation = Quaternion.Slerp(transform.rotation, targetRotation, alignmentSpeed * Time.fixedDeltaTime);

        // 2. Read movement input
        Vector2 input = Vector2.zero;
        if (moveAction != null)
        {
            input = moveAction.ReadValue<Vector2>();
        }

        // 3. Move relative to local forward & right on the sphere surface
        Vector3 localMoveDir = transform.forward * input.y + transform.right * input.x;
        
        // Ensure direction is tangent to the surface normal
        localMoveDir = Vector3.ProjectOnPlane(localMoveDir, gravityUp).normalized;

        Vector3 targetHorizontalVelocity = Vector3.zero;
        if (input.magnitude > 0.01f)
        {
            targetHorizontalVelocity = localMoveDir * moveSpeed;

            // Rotate player to face the movement direction around their local Up axis
            Quaternion faceMoveDir = Quaternion.LookRotation(localMoveDir, gravityUp);
            newRotation = Quaternion.Slerp(newRotation, faceMoveDir, rotationSpeed * Time.fixedDeltaTime);
        }

        // Apply the final calculated rotation using Rigidbody.MoveRotation to satisfy the physics engine
        rb.MoveRotation(newRotation);

        // 4. Integrate gravity manually into the vertical velocity component
        // This ensures gravity is never overridden or wiped out by direct velocity assignments.
        float currentVerticalVelocity = Vector3.Dot(rb.linearVelocity, gravityUp);
        currentVerticalVelocity += gravityConstant * Time.fixedDeltaTime;

        // Combine horizontal and vertical velocities and apply to Rigidbody
        rb.linearVelocity = targetHorizontalVelocity + gravityUp * currentVerticalVelocity;
    }
}
