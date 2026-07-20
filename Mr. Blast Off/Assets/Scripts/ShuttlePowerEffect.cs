using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShuttlePowerEffect : MonoBehaviour
{
    [Header("Glassy Material Settings")]
    public Color glassColor = new Color(0.12f, 0.12f, 0.14f, 0.45f);
    public float metallic = 0.95f;
    public float smoothness = 0.92f;

    [Header("Power Ring Settings")]
    public int ringCount = 3;
    public float ringSpeed = 1.2f;
    public float maxRadius = 1.25f;
    public Color ringColor = new Color(0f, 1f, 0.2f, 1f);
    public float ringWidth = 0.08f;
    public int segments = 40;

    private List<LineRenderer> activeRings = new List<LineRenderer>();
    private List<float> ringProgress = new List<float>(); // 0 (top) to 1 (bottom)
    private Material glassMaterial;

    private void Start()
    {
        ApplyGlassyMaterial();
        CreatePowerRings();
    }

    private void ApplyGlassyMaterial()
    {
        MeshRenderer mr = GetComponent<MeshRenderer>();
        if (mr != null)
        {
            Shader litShader = Shader.Find("Universal Render Pipeline/Lit");
            if (litShader == null) litShader = Shader.Find("Standard");
            if (litShader == null) litShader = Shader.Find("Sprites/Default");
            if (litShader == null) litShader = Shader.Find("Hidden/InternalErrorShader");

            if (litShader != null)
            {
                glassMaterial = new Material(litShader);
                // Configure URP Transparent Lit properties
                glassMaterial.SetFloat("_Surface", 1); // Transparent
                glassMaterial.SetFloat("_Blend", 0);   // Alpha Blend

                // Color & shading properties
                glassMaterial.SetColor("_BaseColor", glassColor);
                glassMaterial.SetFloat("_Metallic", metallic);
                glassMaterial.SetFloat("_Smoothness", smoothness);

                // Set blending factors
                glassMaterial.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
                glassMaterial.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                glassMaterial.SetFloat("_ZWrite", 0); // Disable depth write for transparent glass look

                // Enable necessary keywords and render queue
                glassMaterial.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                glassMaterial.EnableKeyword("_ALPHAPREMULTIPLY_ON_OFF");
                glassMaterial.renderQueue = 3000; // Transparent queue

                mr.sharedMaterial = glassMaterial;
                Debug.Log("[ShuttlePowerEffect] Applied premium glassy material to Shuttle.");
            }
        }
    }

    private void CreatePowerRings()
    {
        // Find the "Universal Render Pipeline/Particles/Unlit" shader as per guidelines
        Shader ringShader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (ringShader == null) ringShader = Shader.Find("Particles/Standard Unlit");
        if (ringShader == null) ringShader = Shader.Find("Sprites/Default");
        if (ringShader == null) ringShader = Shader.Find("Hidden/InternalErrorShader");

        if (ringShader != null)
        {
            Material ringMat = new Material(ringShader);
        // Enable alpha blending on particle shader
        ringMat.SetFloat("_Surface", 1);
        ringMat.SetFloat("_Blend", 0);
        ringMat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
        ringMat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        ringMat.SetFloat("_ZWrite", 0);
        ringMat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        ringMat.renderQueue = 3000;

        for (int i = 0; i < ringCount; i++)
        {
            GameObject ringGo = new GameObject($"PowerRing_{i}");
            ringGo.transform.SetParent(transform, false);
            ringGo.transform.localPosition = Vector3.zero;
            ringGo.transform.localRotation = Quaternion.identity;

            LineRenderer lr = ringGo.AddComponent<LineRenderer>();
            lr.useWorldSpace = false; // Stay in local space of shuttle
            lr.loop = true;
            lr.startWidth = ringWidth;
            lr.endWidth = ringWidth;
            lr.positionCount = segments + 1;
            lr.sharedMaterial = ringMat;

            activeRings.Add(lr);
            // Stagger the initial start progress (e.g. 0.0, 0.33, 0.66)
            ringProgress.Add((float)i / ringCount);
        }
        }
    }

    private void Update()
    {
        for (int i = 0; i < activeRings.Count; i++)
        {
            // Advance progress from 0 (top) to 1 (bottom)
            ringProgress[i] += Time.deltaTime * ringSpeed;
            if (ringProgress[i] > 1.0f)
            {
                ringProgress[i] -= 1.0f; // Loop back to top
            }

            float p = ringProgress[i];

            // Local Y offset goes from top (+1.2f) to bottom (-1.2f)
            float localY = Mathf.Lerp(1.2f, -1.2f, p);

            // Compute dynamic radius based on sphere's contour to wrap beautifully, or slightly larger
            // Radius at y is sqrt(R^2 - y^2). R is 1.0f.
            float localYClamped = Mathf.Clamp(localY, -1.0f, 1.0f);
            float contourRadius = Mathf.Sqrt(Mathf.Max(0.01f, 1.0f - localYClamped * localYClamped));
            float radius = contourRadius * maxRadius;

            // Compute positions in horizontal local XZ plane
            for (int s = 0; i < activeRings.Count && s <= segments; s++)
            {
                float angle = (s / (float)segments) * Mathf.PI * 2f;
                Vector3 pos = new Vector3(Mathf.Cos(angle) * radius, localY, Mathf.Sin(angle) * radius);
                activeRings[i].SetPosition(s, pos);
            }

            // Alpha fades out near the top and bottom to represent power-up materialization and disappearance
            float alpha = 1.0f;
            if (p < 0.2f)
            {
                alpha = p / 0.2f; // Fade in
            }
            else if (p > 0.8f)
            {
                alpha = (1.0f - p) / 0.2f; // Fade out
            }

            // Apply color with alpha
            Color currentRingColor = ringColor;
            currentRingColor.a = alpha;

            // Enable emission using particle tint properties
            activeRings[i].startColor = currentRingColor;
            activeRings[i].endColor = currentRingColor;
            if (activeRings[i].sharedMaterial != null)
            {
                activeRings[i].sharedMaterial.SetColor("_BaseColor", currentRingColor);
                activeRings[i].sharedMaterial.SetColor("_Color", currentRingColor);
            }
        }
    }

    private void OnDestroy()
    {
        if (glassMaterial != null)
        {
            Destroy(glassMaterial);
        }
    }
}
