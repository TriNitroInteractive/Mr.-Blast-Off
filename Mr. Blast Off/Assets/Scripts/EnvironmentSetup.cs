using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Configures the space environment to look dark, deep, and cohesive.
/// - Sets camera clear flags to Solid Color (pure black) for both gameplay and cinematic cameras.
/// - Removes the default skybox.
/// - Configures a flat, subtle ambient lighting so player and shuttle features remain visible.
/// </summary>
public class EnvironmentSetup : MonoBehaviour
{
    [Header("Ambient Colors")]
    [Tooltip("The flat ambient color used to keep unlit surfaces beautifully visible in deep space.")]
    [SerializeField] private Color spaceAmbientColor = new Color(0.12f, 0.12f, 0.15f, 1f);

    private void Start()
    {
        ConfigureCameras();
        ConfigureAmbientLighting();
    }

    /// <summary>
    /// Finds and configures all rendering cameras in the scene to clear to solid black.
    /// </summary>
    public void ConfigureCameras()
    {
        var cameras = Object.FindObjectsByType<Camera>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        Debug.LogFormat("[EnvironmentSetup] Configuring {0} cameras to pure black space background...", cameras.Length);
        
        foreach (var cam in cameras)
        {
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Color.black;
        }
    }

    /// <summary>
    /// Tweaks the lighting and skybox settings for realistic outer space atmosphere.
    /// </summary>
    public void ConfigureAmbientLighting()
    {
        // 1. Remove skybox
        RenderSettings.skybox = null;

        // 2. Set ambient mode to Flat with a dark, cool color
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = spaceAmbientColor;

        Debug.Log("[EnvironmentSetup] Global space lighting and skybox configuration complete.");
    }
}
