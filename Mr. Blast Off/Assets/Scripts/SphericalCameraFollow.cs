using UnityEngine;

public class SphericalCameraFollow : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform target;
    public Transform planet;

    [Header("Smoothness")]
    [SerializeField] private float positionLerpSpeed = 5f;
    [SerializeField] private float rotationLerpSpeed = 5f;

    private Vector3 localPositionOffset;
    private Quaternion localRotationOffset;

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

        // Calculate and cache the starting offset relative to the target player
        if (target != null)
        {
            localPositionOffset = target.InverseTransformPoint(transform.position);
            localRotationOffset = Quaternion.Inverse(target.rotation) * transform.rotation;
        }
    }

    private void Start()
    {
        // Snap directly to the starting position/rotation before unparenting to prevent frame 1 snapping artifacts
        if (target != null)
        {
            transform.position = target.TransformPoint(localPositionOffset);
            transform.rotation = target.rotation * localRotationOffset;
        }

        // Unparent so the camera moves smoothly and independently from player physics updates
        transform.SetParent(null);
    }

    private void LateUpdate()
    {
        if (target == null || planet == null) return;

        // Compute where the camera should be in world space based on the target's current position and rotation
        Vector3 targetPosition = target.TransformPoint(localPositionOffset);
        Quaternion targetRotation = target.rotation * localRotationOffset;

        // Smoothly interpolate towards the target position and orientation
        transform.position = Vector3.Lerp(transform.position, targetPosition, positionLerpSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationLerpSpeed * Time.deltaTime);
    }
}
