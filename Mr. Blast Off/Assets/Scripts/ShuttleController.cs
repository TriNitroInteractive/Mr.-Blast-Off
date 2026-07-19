using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(SphereCollider))]
public class ShuttleController : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float interactionRange = 4.5f;

    [Header("Flight Settings")]
    public float maxSpeed = 22f;
    public float acceleration = 18f;
    public float yawSpeed = 90f;
    public float pitchSpeed = 70f;
    public float cameraDistance = 9f;
    public float cameraHeight = 3f;
    public float cameraLerpSpeed = 6f;
    [Tooltip("If true, the player will start inside the Shuttle when the scene loads.")]
    public bool startInShuttle = true;

    public bool isPiloted = false;

    private GameObject playerObj;
    private SphericalCharacterController playerController;
    private SphericalCameraFollow cameraFollow;
    private Camera mainCamera;
    private Rigidbody rb;

    private bool isPlayerInRange = false;

    private void Awake()
    {
        // Setup Rigidbody programmatically for proper physics collision
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        rb.useGravity = false;
        rb.isKinematic = true; // Kinematic while idle on the ground
        rb.linearDamping = 1.5f;
        rb.angularDamping = 2.0f;
        rb.constraints = RigidbodyConstraints.FreezeRotation; // We handle rotation manually

        mainCamera = Camera.main;
        if (mainCamera != null)
        {
            cameraFollow = mainCamera.GetComponent<SphericalCameraFollow>();
        }
    }

    private void Start()
    {
        // Position the shuttle above the selected planet at start if in the gameplay scene
        if (GameManager.Instance != null && !string.IsNullOrEmpty(GameManager.Instance.selectedPlanetName))
        {
            GameObject planetGo = GameObject.Find(GameManager.Instance.selectedPlanetName);
            if (planetGo != null)
            {
                float planetRadius = GetPlanetRadius(planetGo);
                // Position above the planet. Let's use a safe direction like (0, 1, 0.3) normalized
                Vector3 orbitDir = new Vector3(0f, 1f, 0.3f).normalized;
                transform.position = planetGo.transform.position + orbitDir * (planetRadius * 1.5f);
                // Face tangent to the planet (pointing forward perpendicular to orbitDir)
                Vector3 forwardDir = Vector3.Cross(orbitDir, Vector3.right).normalized;
                transform.rotation = Quaternion.LookRotation(forwardDir, orbitDir);

                Debug.Log($"[ShuttleController] Spawned Shuttle above selected planet: '{GameManager.Instance.selectedPlanetName}' at position {transform.position}");
            }
        }

        if (startInShuttle)
        {
            if (playerObj == null)
            {
                playerObj = GameObject.Find("Mr.Blast");
                if (playerObj != null)
                {
                    playerController = playerObj.GetComponent<SphericalCharacterController>();
                }
            }

            if (playerObj != null)
            {
                EnterShuttle();

                // Ensure camera snaps to cockpit target position immediately to prevent a large lerp from spawn
                if (mainCamera != null)
                {
                    Vector3 targetCamPos = transform.position - transform.forward * cameraDistance + transform.up * cameraHeight;
                    mainCamera.transform.position = targetCamPos;

                    Vector3 lookTarget = transform.position + transform.forward * 4f;
                    mainCamera.transform.rotation = Quaternion.LookRotation(lookTarget - targetCamPos, transform.up);
                }
            }
        }
    }

    private void Update()
    {
        if (playerObj == null)
        {
            playerObj = GameObject.Find("Mr.Blast");
            if (playerObj != null)
            {
                playerController = playerObj.GetComponent<SphericalCharacterController>();
            }
        }

        if (isPiloted)
        {
            HandleFlightRotation();
            // Press F to exit shuttle
            if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
            {
                ExitShuttle();
            }
        }
        else
        {
            if (playerObj != null)
            {
                float dist = Vector3.Distance(transform.position, playerObj.transform.position);
                isPlayerInRange = (dist <= interactionRange);

                // Press F to enter shuttle
                if (isPlayerInRange && Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
                {
                    EnterShuttle();
                }
            }
        }
    }

    private void FixedUpdate()
    {
        if (isPiloted)
        {
            HandleFlightMovement();
        }
    }

    private void LateUpdate()
    {
        if (isPiloted && mainCamera != null)
        {
            // Custom smooth behind-the-ship flight camera follow
            Vector3 targetCamPos = transform.position - transform.forward * cameraDistance + transform.up * cameraHeight;
            mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, targetCamPos, cameraLerpSpeed * Time.deltaTime);

            Vector3 lookTarget = transform.position + transform.forward * 4f;
            Quaternion targetCamRot = Quaternion.LookRotation(lookTarget - mainCamera.transform.position, transform.up);
            mainCamera.transform.rotation = Quaternion.Slerp(mainCamera.transform.rotation, targetCamRot, cameraLerpSpeed * Time.deltaTime);
        }
    }

    private void HandleFlightRotation()
    {
        if (Keyboard.current == null) return;

        // Yaw (A/D keys)
        float yawInput = 0f;
        if (Keyboard.current.aKey.isPressed) yawInput -= 1f;
        if (Keyboard.current.dKey.isPressed) yawInput += 1f;

        // Pitch (Up/Down arrow keys)
        float pitchInput = 0f;
        if (Keyboard.current.upArrowKey.isPressed) pitchInput += 1f;
        if (Keyboard.current.downArrowKey.isPressed) pitchInput -= 1f;

        // Apply local rotations
        transform.Rotate(Vector3.up, yawInput * yawSpeed * Time.deltaTime, Space.Self);
        transform.Rotate(Vector3.right, pitchInput * pitchSpeed * Time.deltaTime, Space.Self);
    }

    private void HandleFlightMovement()
    {
        if (Keyboard.current == null) return;

        // Forward/Backward (W/S keys)
        float fwdInput = 0f;
        if (Keyboard.current.wKey.isPressed) fwdInput += 1f;
        if (Keyboard.current.sKey.isPressed) fwdInput -= 1f;

        // Vertical lift (Space / Left Shift)
        float vertInput = 0f;
        if (Keyboard.current.spaceKey.isPressed) vertInput += 1f;
        if (Keyboard.current.leftShiftKey.isPressed) vertInput -= 1f;

        // Calculate target velocity
        Vector3 targetVelocity = transform.forward * fwdInput * maxSpeed + transform.up * vertInput * maxSpeed;

        // Accelerate Rigidbody towards target velocity
        rb.linearVelocity = Vector3.MoveTowards(rb.linearVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);
    }

    private void EnterShuttle()
    {
        if (playerObj == null) return;

        isPiloted = true;
        rb.isKinematic = false; // Enable physics simulation for flight

        // Deactivate player character safely
        playerObj.SetActive(false);

        // Pause standard camera spherical following
        if (cameraFollow != null)
        {
            cameraFollow.isCinematicActive = true;
        }

        Debug.Log("[ShuttleController] Mr.Blast entered the Shuttle! Commencing space flight.");
    }

    private void ExitShuttle()
    {
        isPiloted = false;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true; // Make kinematic on ground again

        // 1. Detect the nearest planet in the scene to land the player safely on
        Transform nearestPlanet = FindNearestPlanet(out float radius);

        if (playerObj != null)
        {
            // Spawn player slightly offset from shuttle
            Vector3 exitPos = transform.position - transform.forward * 2.5f;

            if (nearestPlanet != null)
            {
                // Align player position perfectly onto the nearest planet surface
                Vector3 planetUp = (exitPos - nearestPlanet.position).normalized;
                exitPos = nearestPlanet.position + planetUp * (radius + 0.5f);

                // Update gravity and alignment parameters on player and camera
                if (playerController != null)
                {
                    playerController.planet = nearestPlanet;
                }
                if (cameraFollow != null)
                {
                    cameraFollow.planet = nearestPlanet;
                }

                Debug.Log($"[ShuttleController] Aligned Mr.Blast on nearest planet: '{nearestPlanet.name}' with surface radius {radius}.");
            }

            playerObj.transform.position = exitPos;
            playerObj.SetActive(true);
        }

        // Restore standard camera spherical following
        if (cameraFollow != null)
        {
            cameraFollow.isCinematicActive = false;
        }

        Debug.Log("[ShuttleController] Mr.Blast exited the Shuttle.");
    }

    private Transform FindNearestPlanet(out float radius)
    {
        radius = 15f; // Default fallback
        Transform bestPlanet = null;
        float minDist = float.MaxValue;

        // Find all root/non-root objects that represent planets
        var allObjects = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include);
        foreach (var obj in allObjects)
        {
            if (obj.name.StartsWith("Planet"))
            {
                float dist = Vector3.Distance(transform.position, obj.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    bestPlanet = obj.transform;
                    radius = GetPlanetRadius(obj);
                }
            }
        }

        return bestPlanet;
    }

    private float GetPlanetRadius(GameObject planetGo)
    {
        // 1. If there's a SphereCollider on the root, use its radius
        var sphereCol = planetGo.GetComponent<SphereCollider>();
        if (sphereCol != null)
        {
            return sphereCol.radius * planetGo.transform.localScale.x;
        }

        // 2. Otherwise, check for MeshFilter components in children
        var meshFilters = planetGo.GetComponentsInChildren<MeshFilter>(true);
        if (meshFilters.Length > 0)
        {
            float maxLocalDist = 0f;
            foreach (var mf in meshFilters)
            {
                if (mf.sharedMesh == null) continue;
                Vector3 boundsCenter = mf.sharedMesh.bounds.center;
                Vector3 extents = mf.sharedMesh.bounds.extents;
                Vector3[] corners = new Vector3[]
                {
                    boundsCenter + new Vector3(extents.x, extents.y, extents.z),
                    boundsCenter + new Vector3(extents.x, extents.y, -extents.z),
                    boundsCenter + new Vector3(extents.x, -extents.y, extents.z),
                    boundsCenter + new Vector3(extents.x, -extents.y, -extents.z),
                    boundsCenter + new Vector3(-extents.x, extents.y, extents.z),
                    boundsCenter + new Vector3(-extents.x, extents.y, -extents.z),
                    boundsCenter + new Vector3(-extents.x, -extents.y, extents.z),
                    boundsCenter + new Vector3(-extents.x, -extents.y, -extents.z)
                };
                foreach (var corner in corners)
                {
                    Vector3 worldCorner = mf.transform.TransformPoint(corner);
                    float dist = Vector3.Distance(planetGo.transform.position, worldCorner);
                    if (dist > maxLocalDist) maxLocalDist = dist;
                }
            }
            if (maxLocalDist > 0f) return maxLocalDist;
        }

        // 3. Fallback to scale-based estimation
        return planetGo.transform.localScale.x * 0.5f;
    }

    private void OnGUI()
    {
        if (isPiloted)
        {
            // Piloting Flight HUD
            GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
            boxStyle.normal.textColor = Color.green;
            boxStyle.fontSize = 15;
            boxStyle.alignment = TextAnchor.UpperLeft;

            GUILayout.BeginArea(new Rect(25f, 25f, 320f, 180f), boxStyle);
            GUILayout.Label("  SHUTTLE COCKPIT INITIALIZED", GUILayout.ExpandWidth(true));
            GUILayout.Label("  ----------------------------");
            GUILayout.Label("  W / S      : Thrusters Forward / Back");
            GUILayout.Label("  A / D      : Steering Yaw Left / Right");
            GUILayout.Label("  Arrows Up/Dn: Pitch Nose Up / Down");
            GUILayout.Label("  Space / Shift : Fly Up / Down");
            GUILayout.Label("  F Key      : Exit Shuttle / Land");
            GUILayout.EndArea();
        }
        else if (isPlayerInRange)
        {
            // Interaction prompt
            GUIStyle promptStyle = new GUIStyle(GUI.skin.box);
            promptStyle.fontSize = 18;
            promptStyle.normal.textColor = Color.yellow;
            promptStyle.alignment = TextAnchor.MiddleCenter;

            float width = 300f;
            float height = 45f;
            float x = (Screen.width - width) / 2f;
            float y = Screen.height - 180f;

            GUI.Box(new Rect(x, y, width, height), "Press [F] to Enter Shuttle", promptStyle);
        }
    }
}
