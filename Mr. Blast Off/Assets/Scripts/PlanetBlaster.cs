using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetBlaster : MonoBehaviour
{
    [Header("Material References")]
    public Material earthMaterial;
    public Material blastMaterial;
    public Material rayMaterial;

    [Header("Explosion Settings")]
    [Tooltip("Outward force applied to planet debris")]
    public float explosionForce = 18f;
    [Tooltip("Spin torque applied to planet debris")]
    public float torqueForce = 12f;
    [Tooltip("Number of outer crust debris pieces")]
    public int numOuterDebris = 64;
    [Tooltip("Number of inner burning core debris pieces")]
    public int numInnerDebris = 32;
    [Tooltip("Duration of the entire blast animation")]
    public float blastDuration = 5f;

    [Header("Volumetric Ray Settings")]
    [Tooltip("Number of light ray beams that burst out")]
    public int numRays = 24;
    [Tooltip("Max length of light rays")]
    public float rayMaxLength = 35f;
    [Tooltip("Width/Thickness of light rays")]
    public float rayThickness = 3f;
    [Tooltip("Spin rotation speed of light rays during the blast")]
    public float rayRotationSpeed = 45f;

    [Header("Player Interaction Settings")]
    [Tooltip("Outward impulse applied to launch Mr.Blast into space")]
    public float playerLaunchForce = 25f;
    [Tooltip("Random torque applied to spin Mr.Blast")]
    public float playerLaunchTorque = 15f;

    [Header("Light Flash Settings")]
    public Color lightColor = new Color(1f, 0.45f, 0.1f);
    public float maxLightIntensity = 150f;
    public float maxLightRange = 50f;

    [Header("Cinematic Build-up Settings")]
    [Tooltip("How long the pre-explosion build-up, rumble, and cracking lasts in seconds")]
    public float buildUpDuration = 3f;
    [Tooltip("Maximum intensity of planet vibration shake during rumble")]
    public float shakeStrength = 0.25f;

    private bool isBlasted = false;
    private float planetRadius = 5.0f; // From planet scale / 2

    private void Awake()
    {
        // Calculate radius based on sphere's local scale (standard sphere mesh diameter is 1 unit)
        planetRadius = transform.localScale.x * 0.5f;
    }

    /// <summary>
    /// Generates a procedural double-sided intersecting cross-quad mesh representing a 3D ray beam.
    /// This mesh has its pivot at the base (Y = 0) and extends to Y = 1, allowing natural Y-scaling.
    /// </summary>
    private Mesh CreateCrossQuadMesh()
    {
        Mesh mesh = new Mesh();
        mesh.name = "ProceduralCrossQuad";

        Vector3[] vertices = new Vector3[8];
        Vector2[] uvs = new Vector2[8];
        int[] triangles = new int[12];

        // Quad 1 (XY Plane)
        vertices[0] = new Vector3(-0.5f, 0f, 0f);
        vertices[1] = new Vector3(0.5f, 0f, 0f);
        vertices[2] = new Vector3(-0.5f, 1f, 0f);
        vertices[3] = new Vector3(0.5f, 1f, 0f);

        uvs[0] = new Vector2(0f, 0f);
        uvs[1] = new Vector2(1f, 0f);
        uvs[2] = new Vector2(0f, 1f);
        uvs[3] = new Vector2(1f, 1f);

        // Quad 2 (ZY Plane)
        vertices[4] = new Vector3(0f, 0f, -0.5f);
        vertices[5] = new Vector3(0f, 0f, 0.5f);
        vertices[6] = new Vector3(0f, 1f, -0.5f);
        vertices[7] = new Vector3(0f, 1f, 0.5f);

        uvs[4] = new Vector2(0f, 0f);
        uvs[5] = new Vector2(1f, 0f);
        uvs[6] = new Vector2(0f, 1f);
        uvs[7] = new Vector2(1f, 1f);

        // Triangles for both quads (double-sided rendering is also enabled in shader Cull Off)
        triangles[0] = 0; triangles[1] = 2; triangles[2] = 1;
        triangles[3] = 1; triangles[4] = 2; triangles[5] = 3;

        triangles[6] = 4; triangles[7] = 6; triangles[8] = 5;
        triangles[9] = 5; triangles[10] = 6; triangles[11] = 7;

        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }

    /// <summary>
    /// Triggers the full planetary detonation!
    /// </summary>
    public void Detonate()
    {
        if (isBlasted) return;
        isBlasted = true;

        Debug.Log("[PlanetBlaster] DETONATION COMMAND INITIATED. BRACE FOR IMPACT!");

        // Start the delayed multi-stage cinematic kickoff sequence
        StartCoroutine(DetonationSequence());
    }

    private IEnumerator DetonationSequence()
    {
        Vector3 originalPlanetPos = transform.position;
        Vector3 originalPlanetScale = transform.localScale;

        // 1. Create Internal Light Source immediately (disabled / intensity 0 initially)
        GameObject lightGo = new GameObject("Blast_InnerLight");
        lightGo.transform.position = transform.position;
        Light innerLight = lightGo.AddComponent<Light>();
        innerLight.type = LightType.Point;
        innerLight.color = lightColor;
        innerLight.intensity = 0f;
        innerLight.range = 0f;

        // 2. Spawn Volumetric Light Rays immediately (flat, but start extending slowly through cracks)
        GameObject rayContainer = new GameObject("Blast_Ray_Container");
        rayContainer.transform.position = transform.position;

        List<GameObject> rayGameObjects = new List<GameObject>();
        List<Vector3> rayTargetScales = new List<Vector3>();
        List<float> rayRotSpeeds = new List<float>();
        List<Material> rayInstances = new List<Material>();

        Mesh rayMesh = CreateCrossQuadMesh();

        for (int i = 0; i < numRays; i++)
        {
            GameObject rayGo = new GameObject($"LightRay_{i}");
            rayGo.transform.position = transform.position;
            rayGo.transform.SetParent(rayContainer.transform);

            // Point local Y axis outwards in a random 3D direction
            Vector3 randomDir = Random.onUnitSphere;
            rayGo.transform.rotation = Quaternion.LookRotation(randomDir) * Quaternion.Euler(90f, 0f, 0f);

            // Set up mesh filter and renderer
            MeshFilter filter = rayGo.AddComponent<MeshFilter>();
            filter.sharedMesh = rayMesh;

            MeshRenderer rayRenderer = rayGo.AddComponent<MeshRenderer>();
            if (rayRenderer != null && rayMaterial != null)
            {
                Material instantiatedMat = new Material(rayMaterial);
                rayRenderer.sharedMaterial = instantiatedMat;
                rayInstances.Add(instantiatedMat);
            }

            rayGo.transform.localScale = Vector3.zero;

            // Compute randomized target dimensions
            float length = rayMaxLength * Random.Range(0.7f, 1.3f);
            float thickness = rayThickness * Random.Range(0.6f, 1.4f);
            rayTargetScales.Add(new Vector3(thickness, length, thickness));

            rayRotSpeeds.Add(Random.Range(-rayRotationSpeed, rayRotationSpeed));
            rayGameObjects.Add(rayGo);
        }

        // 3. Prepare Debris Containers
        GameObject debrisContainer = new GameObject("Planet_Debris_Container");
        debrisContainer.transform.position = transform.position;

        List<Rigidbody> debrisRigidbodies = new List<Rigidbody>();
        List<GameObject> debrisObjects = new List<GameObject>();

        // We will spawn a portion (e.g. 12 pieces) of the outer debris early to look like cracking/lifting off chunks
        int numEarlyDebris = Mathf.Min(12, numOuterDebris);
        int remainingOuterDebris = numOuterDebris - numEarlyDebris;

        List<GameObject> earlyDebrisObjs = new List<GameObject>();
        List<Rigidbody> earlyDebrisRbs = new List<Rigidbody>();

        // Generate Early Cracking Debris
        for (int i = 0; i < numEarlyDebris; i++)
        {
            float y = 1f - (i / (float)(numEarlyDebris - 1)) * 2f; 
            float r = Mathf.Sqrt(1f - y * y); 
            float goldenAngle = Mathf.PI * (3f - Mathf.Sqrt(5f));
            float theta = goldenAngle * i;

            Vector3 dir = new Vector3(Mathf.Cos(theta) * r, y, Mathf.Sin(theta) * r);
            Vector3 spawnPos = transform.position + dir * planetRadius;

            GameObject piece = GameObject.CreatePrimitive(PrimitiveType.Cube);
            piece.name = $"EarlyCrackingDebris_{i}";
            piece.transform.position = spawnPos;
            piece.transform.rotation = Random.rotation;
            piece.transform.SetParent(debrisContainer.transform);

            // Start very small or scale up during cracks
            float size = Random.Range(0.5f, 1.0f);
            piece.transform.localScale = Vector3.zero; // Scale up dynamically

            var pieceRenderer = piece.GetComponent<MeshRenderer>();
            if (pieceRenderer != null && earthMaterial != null)
            {
                pieceRenderer.sharedMaterial = earthMaterial;
            }

            // Set up a collider but disable initially to prevent player tripping over early cracking chunks
            var col = piece.GetComponent<Collider>();
            if (col != null) col.enabled = false;

            earlyDebrisObjs.Add(piece);
            debrisObjects.Add(piece);
        }

        // ==========================================
        // STAGE 1: Pre-Explosion Rumble, Shaking, Cracking, Piercing Rays (Build-up Phase)
        // ==========================================
        Debug.Log("[PlanetBlaster] STAGE 1: INITIATING REACTION. CORE PRESSURE SWELLING...");
        float buildUpElapsed = 0f;

        while (buildUpElapsed < buildUpDuration)
        {
            buildUpElapsed += Time.deltaTime;
            float t = buildUpElapsed / buildUpDuration;

            // A. Rumble: Shake the planet, with increasing intensity (exponential back-weighted)
            float currentShake = shakeStrength * (t * t);
            transform.position = originalPlanetPos + Random.insideUnitSphere * currentShake;

            // B. Pressure Pulsation: Let the planet expand slightly
            transform.localScale = originalPlanetScale * (1f + 0.08f * (t * t));

            // C. Light leak: Slowly swell the central light
            if (innerLight != null)
            {
                innerLight.intensity = Mathf.Lerp(0f, maxLightIntensity * 0.25f, t);
                innerLight.range = Mathf.Lerp(0f, maxLightRange * 0.5f, t);
            }

            // D. Piercing Rays: Volumetric light rays slowly stretch out through the cracking crust
            for (int i = 0; i < rayGameObjects.Count; i++)
            {
                if (rayGameObjects[i] != null)
                {
                    // Grow to 30% length during buildup
                    Vector3 partialScale = rayTargetScales[i] * Mathf.Lerp(0f, 0.35f, t);
                    rayGameObjects[i].transform.localScale = partialScale;
                    rayGameObjects[i].transform.Rotate(Vector3.up, rayRotSpeeds[i] * 0.3f * Time.deltaTime, Space.Self);
                }
            }

            // E. Cracking Debris: Scale up early chunks and slowly lift them away from the surface
            for (int i = 0; i < earlyDebrisObjs.Count; i++)
            {
                if (earlyDebrisObjs[i] != null)
                {
                    float size = Random.Range(0.5f, 1.0f);
                    earlyDebrisObjs[i].transform.localScale = Vector3.Lerp(Vector3.zero, new Vector3(size, size, size), t);

                    // Slowly drift outwards
                    Vector3 dir = (earlyDebrisObjs[i].transform.position - transform.position).normalized;
                    earlyDebrisObjs[i].transform.position += dir * (0.8f * Time.deltaTime);
                    earlyDebrisObjs[i].transform.Rotate(Random.insideUnitSphere * 15f * Time.deltaTime);
                }
            }

            yield return null;
        }

        // Restore original positions/scale before deactivating
        transform.position = originalPlanetPos;
        transform.localScale = originalPlanetScale;

        // ==========================================
        // STAGE 2: THE BIG BOOM! (Full vaporization, debris launch, fireball, player launch)
        // ==========================================
        Debug.Log("[PlanetBlaster] STAGE 2: MAXIMUM REACTOR CRITICALITY. DETONATION!");

        // 1. Disable the planet's visual and collider components in the entire hierarchy
        var planetRenderers = GetComponentsInChildren<MeshRenderer>();
        foreach (var r in planetRenderers)
        {
            r.enabled = false;
        }

        var planetColliders = GetComponentsInChildren<Collider>();
        foreach (var col in planetColliders)
        {
            col.enabled = false;
        }

        // 2. Locate and launch Mr.Blast
        GameObject playerObj = GameObject.Find("Mr.Blast");
        if (playerObj != null)
        {
            var movementController = playerObj.GetComponent<SphericalCharacterController>();
            if (movementController != null)
            {
                movementController.enabled = false;
            }

            var playerRb = playerObj.GetComponent<Rigidbody>();
            if (playerRb != null)
            {
                playerRb.freezeRotation = false;
                playerRb.constraints = RigidbodyConstraints.None;

                Vector3 launchDir = (playerObj.transform.position - transform.position).normalized;
                playerRb.AddForce(launchDir * playerLaunchForce, ForceMode.Impulse);
                playerRb.AddTorque(Random.insideUnitSphere * playerLaunchTorque, ForceMode.Impulse);
                Debug.Log("[PlanetBlaster] Mr.Blast launched into deep space with impulse force!");
            }
        }

        // 3. Create Expanding Fireball Core
        GameObject fireball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        fireball.name = "Blast_Fireball";
        fireball.transform.position = transform.position;
        fireball.transform.localScale = Vector3.zero;
        
        var fireballCollider = fireball.GetComponent<Collider>();
        if (fireballCollider != null) Destroy(fireballCollider);

        var fireballRenderer = fireball.GetComponent<MeshRenderer>();
        if (fireballRenderer != null && blastMaterial != null)
        {
            fireballRenderer.sharedMaterial = blastMaterial;
        }

        // 4. Activate Physics on early debris & blast them away violently
        foreach (var piece in earlyDebrisObjs)
        {
            if (piece != null)
            {
                var col = piece.GetComponent<Collider>();
                if (col != null) col.enabled = true;

                Rigidbody rb = piece.AddComponent<Rigidbody>();
                rb.useGravity = false;
                rb.linearVelocity = Vector3.zero;

                Vector3 dir = (piece.transform.position - transform.position).normalized;
                rb.AddForce(dir * (explosionForce * Random.Range(1.2f, 1.8f)), ForceMode.Impulse);
                rb.AddTorque(Random.insideUnitSphere * (torqueForce * Random.Range(1.0f, 2.0f)), ForceMode.Impulse);

                debrisRigidbodies.Add(rb);
            }
        }

        // 5. Generate remaining Outer Crust Debris
        for (int i = 0; i < remainingOuterDebris; i++)
        {
            float y = 1f - (i / (float)(remainingOuterDebris - 1)) * 2f; 
            float r = Mathf.Sqrt(1f - y * y); 
            float goldenAngle = Mathf.PI * (3f - Mathf.Sqrt(5f));
            float theta = goldenAngle * i;

            Vector3 dir = new Vector3(Mathf.Cos(theta) * r, y, Mathf.Sin(theta) * r);
            Vector3 spawnPos = transform.position + dir * planetRadius;

            GameObject piece = GameObject.CreatePrimitive(PrimitiveType.Cube);
            piece.name = $"OuterDebris_{i}";
            piece.transform.position = spawnPos;
            piece.transform.rotation = Random.rotation;
            piece.transform.SetParent(debrisContainer.transform);

            float size = Random.Range(0.6f, 1.3f);
            piece.transform.localScale = new Vector3(size, size, size);

            var pieceRenderer = piece.GetComponent<MeshRenderer>();
            if (pieceRenderer != null && earthMaterial != null)
            {
                pieceRenderer.sharedMaterial = earthMaterial;
            }

            Rigidbody rb = piece.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.linearVelocity = Vector3.zero;

            rb.AddForce(dir * (explosionForce * Random.Range(0.8f, 1.4f)), ForceMode.Impulse);
            rb.AddTorque(Random.insideUnitSphere * (torqueForce * Random.Range(0.5f, 1.5f)), ForceMode.Impulse);

            debrisRigidbodies.Add(rb);
            debrisObjects.Add(piece);
        }

        // 6. Generate Inner Glowing Core Debris
        for (int i = 0; i < numInnerDebris; i++)
        {
            float y = 1f - (i / (float)(numInnerDebris - 1)) * 2f;
            float r = Mathf.Sqrt(1f - y * y);
            float goldenAngle = Mathf.PI * (3f - Mathf.Sqrt(5f));
            float theta = goldenAngle * i;

            Vector3 dir = new Vector3(Mathf.Cos(theta) * r, y, Mathf.Sin(theta) * r);
            Vector3 spawnPos = transform.position + dir * (planetRadius * Random.Range(0.2f, 0.7f));

            GameObject piece = GameObject.CreatePrimitive(PrimitiveType.Cube);
            piece.name = $"InnerDebris_{i}";
            piece.transform.position = spawnPos;
            piece.transform.rotation = Random.rotation;
            piece.transform.SetParent(debrisContainer.transform);

            float size = Random.Range(0.4f, 0.9f);
            piece.transform.localScale = new Vector3(size, size, size);

            var pieceRenderer = piece.GetComponent<MeshRenderer>();
            if (pieceRenderer != null && blastMaterial != null)
            {
                pieceRenderer.sharedMaterial = blastMaterial;
            }

            Rigidbody rb = piece.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.linearVelocity = Vector3.zero;

            rb.AddForce(dir * (explosionForce * Random.Range(1.3f, 2.0f)), ForceMode.Impulse);
            rb.AddTorque(Random.insideUnitSphere * (torqueForce * Random.Range(1.0f, 2.0f)), ForceMode.Impulse);

            debrisRigidbodies.Add(rb);
            debrisObjects.Add(piece);
        }

        // ==========================================
        // STAGE 3: Settle Phase, Fading, and Physics Cleanup
        // ==========================================
        float elapsed = 0f;
        float peakTime = blastDuration * 0.25f; // Reach peak size/glow at 25% duration

        List<Color> rayStartColors = new List<Color>();
        foreach (var mat in rayInstances)
        {
            if (mat != null && mat.HasProperty("_Color"))
            {
                rayStartColors.Add(mat.GetColor("_Color"));
            }
            else
            {
                rayStartColors.Add(Color.white);
            }
        }

        while (elapsed < blastDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / blastDuration;

            if (elapsed < peakTime)
            {
                float t = elapsed / peakTime;
                
                // Point Light swelling rapidly to max
                if (innerLight != null)
                {
                    innerLight.intensity = Mathf.Lerp(maxLightIntensity * 0.25f, maxLightIntensity, t);
                    innerLight.range = Mathf.Lerp(maxLightRange * 0.5f, maxLightRange, t);
                }
                
                // Fireball swelling rapidly
                if (fireball != null)
                {
                    float currentScale = Mathf.Lerp(0f, planetRadius * 2.4f, t);
                    fireball.transform.localScale = new Vector3(currentScale, currentScale, currentScale);
                }

                // Ray beams shooting outwards at maximum speed!
                for (int i = 0; i < rayGameObjects.Count; i++)
                {
                    if (rayGameObjects[i] != null)
                    {
                        Vector3 currentMinScale = rayTargetScales[i] * 0.35f;
                        rayGameObjects[i].transform.localScale = Vector3.Lerp(currentMinScale, rayTargetScales[i], t);
                    }
                }
            }
            else
            {
                float t = (elapsed - peakTime) / (blastDuration - peakTime);
                
                // Point Light fading
                if (innerLight != null)
                {
                    innerLight.intensity = Mathf.Lerp(maxLightIntensity, 0f, t);
                    innerLight.range = Mathf.Lerp(maxLightRange, 0f, t);
                }
                
                // Fireball shrinking
                if (fireball != null)
                {
                    float currentScale = Mathf.Lerp(planetRadius * 2.4f, 0f, t);
                    fireball.transform.localScale = new Vector3(currentScale, currentScale, currentScale);
                }

                // Debris damping increase to slow them down smoothly before freeze
                foreach (var rb in debrisRigidbodies)
                {
                    if (rb != null)
                    {
                        rb.linearDamping = Mathf.Lerp(0f, 3f, t);
                        rb.angularDamping = Mathf.Lerp(0f, 3f, t);
                    }
                }

                // Ray beams slowly fading out and extending slightly further
                for (int i = 0; i < rayGameObjects.Count; i++)
                {
                    if (rayGameObjects[i] != null)
                    {
                        rayGameObjects[i].transform.localScale = Vector3.Lerp(rayTargetScales[i], rayTargetScales[i] * 1.15f, t);

                        if (i < rayInstances.Count && rayInstances[i] != null)
                        {
                            Color currentColor = Color.Lerp(rayStartColors[i], Color.clear, t);
                            rayInstances[i].SetColor("_Color", currentColor);
                        }
                    }
                }
            }

            // Continuous ray spin rotation around local Y-axis for shimmer effect
            for (int i = 0; i < rayGameObjects.Count; i++)
            {
                if (rayGameObjects[i] != null)
                {
                    rayGameObjects[i].transform.Rotate(Vector3.up, rayRotSpeeds[i] * Time.deltaTime, Space.Self);
                }
            }

            yield return null;
        }

        Debug.Log("[PlanetBlaster] Settle phase complete. Disabling physics on all debris.");

        // Clean up ray materials to prevent memory leak
        foreach (var mat in rayInstances)
        {
            if (mat != null) Destroy(mat);
        }

        // Smoothly shrink static debris pieces to zero before destruction
        float shrinkElapsed = 0f;
        float shrinkDuration = 1.5f;
        List<Vector3> startScales = new List<Vector3>();

        foreach (var go in debrisObjects)
        {
            if (go != null)
            {
                startScales.Add(go.transform.localScale);
                
                var rb = go.GetComponent<Rigidbody>();
                if (rb != null) Destroy(rb);

                var col = go.GetComponent<Collider>();
                if (col != null) Destroy(col);
            }
            else
            {
                startScales.Add(Vector3.zero);
            }
        }

        while (shrinkElapsed < shrinkDuration)
        {
            shrinkElapsed += Time.deltaTime;
            float t = shrinkElapsed / shrinkDuration;

            for (int i = 0; i < debrisObjects.Count; i++)
            {
                if (debrisObjects[i] != null)
                {
                    debrisObjects[i].transform.localScale = Vector3.Lerp(startScales[i], Vector3.zero, t);
                }
            }

            yield return null;
        }

        // Final cleanup of GameObjects
        if (lightGo != null) Destroy(lightGo);
        if (fireball != null) Destroy(fireball);
        if (rayContainer != null) Destroy(rayContainer);
        if (debrisContainer != null) Destroy(debrisContainer);

        Debug.Log("[PlanetBlaster] Planet detonation sequence fully complete. All debris and rays cleared.");
    }
}

