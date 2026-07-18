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

        // 1. Disable the planet's visual and collider components
        var renderer = GetComponent<MeshRenderer>();
        if (renderer != null) renderer.enabled = false;

        var collider = GetComponent<Collider>();
        if (collider != null) collider.enabled = false;

        // 2. Locate and launch Mr.Blast
        GameObject playerObj = GameObject.Find("Mr.Blast");
        if (playerObj != null)
        {
            // Disable movement control so player floats freely under physics
            var movementController = playerObj.GetComponent<SphericalCharacterController>();
            if (movementController != null)
            {
                movementController.enabled = false;
            }

            var playerRb = playerObj.GetComponent<Rigidbody>();
            if (playerRb != null)
            {
                // Remove rotation locking so Mr.Blast spins dramatically in 3D space
                playerRb.freezeRotation = false;
                playerRb.constraints = RigidbodyConstraints.None;

                // Calculate outward direction
                Vector3 launchDir = (playerObj.transform.position - transform.position).normalized;
                
                // Add sudden explosive blast impulse
                playerRb.AddForce(launchDir * playerLaunchForce, ForceMode.Impulse);
                playerRb.AddTorque(Random.insideUnitSphere * playerLaunchTorque, ForceMode.Impulse);
                
                Debug.Log("[PlanetBlaster] Mr.Blast launched into deep space with impulse force!");
            }
        }

        // 3. Create Internal Light Source
        GameObject lightGo = new GameObject("Blast_InnerLight");
        lightGo.transform.position = transform.position;
        Light innerLight = lightGo.AddComponent<Light>();
        innerLight.type = LightType.Point;
        innerLight.color = lightColor;
        innerLight.intensity = 0f;
        innerLight.range = 0f;

        // 4. Create Expanding Fireball Core
        GameObject fireball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        fireball.name = "Blast_Fireball";
        fireball.transform.position = transform.position;
        fireball.transform.localScale = Vector3.zero;
        
        // Remove collider from fireball to prevent physics collisions
        var fireballCollider = fireball.GetComponent<Collider>();
        if (fireballCollider != null) Destroy(fireballCollider);

        var fireballRenderer = fireball.GetComponent<MeshRenderer>();
        if (fireballRenderer != null && blastMaterial != null)
        {
            fireballRenderer.sharedMaterial = blastMaterial;
        }

        // 5. Generate Volumetric Light Rays (Rays released when blast happens!)
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
                // Create unique material instance for dynamic fading
                Material instantiatedMat = new Material(rayMaterial);
                rayRenderer.sharedMaterial = instantiatedMat;
                rayInstances.Add(instantiatedMat);
            }

            // Start flat/small, animate scale later
            rayGo.transform.localScale = Vector3.zero;

            // Compute randomized target dimensions
            float length = rayMaxLength * Random.Range(0.7f, 1.3f);
            float thickness = rayThickness * Random.Range(0.6f, 1.4f);
            rayTargetScales.Add(new Vector3(thickness, length, thickness));

            // Random rotation spin around local Y axis
            rayRotSpeeds.Add(Random.Range(-rayRotationSpeed, rayRotationSpeed));
            rayGameObjects.Add(rayGo);
        }

        Debug.Log($"[PlanetBlaster] Spawned {numRays} volumetric light rays shooting out from core!");

        // 6. Generate Debris (Both Outer Crust and Inner Burning Core)
        GameObject debrisContainer = new GameObject("Planet_Debris_Container");
        debrisContainer.transform.position = transform.position;

        List<Rigidbody> debrisRigidbodies = new List<Rigidbody>();
        List<GameObject> debrisObjects = new List<GameObject>();

        // Generate Outer Crust Debris (using Earth material)
        for (int i = 0; i < numOuterDebris; i++)
        {
            float y = 1f - (i / (float)(numOuterDebris - 1)) * 2f; 
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

        // Generate Inner Glowing Core Debris (using Blast material)
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

        // 7. Start the sequence timeline
        StartCoroutine(DetonationSequence(
            innerLight, 
            fireball, 
            debrisRigidbodies, 
            debrisObjects, 
            lightGo, 
            fireball, 
            debrisContainer, 
            rayGameObjects, 
            rayTargetScales, 
            rayRotSpeeds, 
            rayInstances, 
            rayContainer));
    }

    private IEnumerator DetonationSequence(
        Light innerLight, 
        GameObject fireball, 
        List<Rigidbody> debrisRbs, 
        List<GameObject> debrisObjs,
        GameObject lightGo,
        GameObject fireballGo,
        GameObject debrisContainerGo,
        List<GameObject> rayGos,
        List<Vector3> rayTargetScales,
        List<float> rayRotSpeeds,
        List<Material> rayMats,
        GameObject rayContainerGo)
    {
        float elapsed = 0f;
        float peakTime = blastDuration * 0.25f; // Reach peak size/glow at 25% duration

        // Capture starting colors of ray materials
        List<Color> rayStartColors = new List<Color>();
        foreach (var mat in rayMats)
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

            // A. Handle lighting and fireball expansion
            if (elapsed < peakTime)
            {
                float t = elapsed / peakTime;
                
                // Point Light swelling
                if (innerLight != null)
                {
                    innerLight.intensity = Mathf.Lerp(0f, maxLightIntensity, t);
                    innerLight.range = Mathf.Lerp(0f, maxLightRange, t);
                }
                
                // Fireball swelling
                if (fireball != null)
                {
                    float currentScale = Mathf.Lerp(0f, planetRadius * 2.4f, t);
                    fireball.transform.localScale = new Vector3(currentScale, currentScale, currentScale);
                }

                // Ray beams shooting outwards rapidly!
                for (int i = 0; i < rayGos.Count; i++)
                {
                    if (rayGos[i] != null)
                    {
                        // Interpolate scale from 0 to target size
                        rayGos[i].transform.localScale = Vector3.Lerp(Vector3.zero, rayTargetScales[i], t);
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
                foreach (var rb in debrisRbs)
                {
                    if (rb != null)
                    {
                        rb.linearDamping = Mathf.Lerp(0f, 3f, t);
                        rb.angularDamping = Mathf.Lerp(0f, 3f, t);
                    }
                }

                // Ray beams slowly fading out and extending slightly further
                for (int i = 0; i < rayGos.Count; i++)
                {
                    if (rayGos[i] != null)
                    {
                        // Gently continue growing slightly during fadeout
                        rayGos[i].transform.localScale = Vector3.Lerp(rayTargetScales[i], rayTargetScales[i] * 1.15f, t);

                        // Fade the instantiated material color to 0
                        if (i < rayMats.Count && rayMats[i] != null)
                        {
                            Color currentColor = Color.Lerp(rayStartColors[i], Color.clear, t);
                            rayMats[i].SetColor("_Color", currentColor);
                        }
                    }
                }
            }

            // Continuous ray spin rotation around local Y-axis for shimmer effect
            for (int i = 0; i < rayGos.Count; i++)
            {
                if (rayGos[i] != null)
                {
                    rayGos[i].transform.Rotate(Vector3.up, rayRotSpeeds[i] * Time.deltaTime, Space.Self);
                }
            }

            yield return null;
        }

        // B. Clean up physics completely ("after, there will be no physics")
        Debug.Log("[PlanetBlaster] Settle phase complete. Disabling physics on all debris.");

        // Clean up ray materials to prevent memory leak
        foreach (var mat in rayMats)
        {
            if (mat != null) Destroy(mat);
        }

        // Smoothly shrink static debris pieces to zero before destruction
        float shrinkElapsed = 0f;
        float shrinkDuration = 1.5f;
        List<Vector3> startScales = new List<Vector3>();

        foreach (var go in debrisObjs)
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

            for (int i = 0; i < debrisObjs.Count; i++)
            {
                if (debrisObjs[i] != null)
                {
                    debrisObjs[i].transform.localScale = Vector3.Lerp(startScales[i], Vector3.zero, t);
                }
            }

            yield return null;
        }

        // Final cleanup of GameObjects
        if (lightGo != null) Destroy(lightGo);
        if (fireballGo != null) Destroy(fireballGo);
        if (rayContainerGo != null) Destroy(rayContainerGo);
        if (debrisContainerGo != null) Destroy(debrisContainerGo);

        Debug.Log("[PlanetBlaster] Planet detonation sequence fully complete. All debris and rays cleared.");
    }
}

