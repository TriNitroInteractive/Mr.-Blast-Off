using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Reactivity Settings")]
    [Tooltip("Minimum required reaction points to detonate the planet")]
    public int requiredPoints = 80;

    [Header("Visual References")]
    [Tooltip("Optional custom material for player debris. If null, a red/orange glowing procedural color is used.")]
    public Material debrisMaterial;

    public int TotalReactionPoints { get; private set; } = 0;
    public bool IsGameOver { get; private set; } = false;

    private TextMeshProUGUI restartText;
    private float flashTimer = 0f;

    // Reactivity points mapping for the 30 scientific/nuclear elements (scale 1-10)
    private static readonly Dictionary<string, int> ElementPoints = new Dictionary<string, int>
    {
        { "Uranium", 10 }, { "Plutonium", 10 }, { "Tritium", 10 }, { "Helium-3", 10 },
        { "Deuterium", 9 }, { "Thorium", 9 }, { "Neptunium", 9 },
        { "Radium", 8 }, { "Polonium", 8 }, { "Cesium", 8 }, { "Platinum", 8 },
        { "Cobalt", 7 }, { "Tungsten", 7 }, { "Gold", 7 },
        { "Lithium", 6 }, { "Zirconium", 6 }, { "Nickel", 6 },
        { "Carbon", 5 }, { "Sodium", 5 }, { "Titanium", 5 },
        { "Beryllium", 4 }, { "Xenon", 4 }, { "Krypton", 4 },
        { "Graphite", 3 }, { "Copper", 3 }, { "Iron", 3 },
        { "Boron", 2 }, { "Steel", 2 },
        { "Lead", 1 }, { "Cadmium", 1 }
    };

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (IsGameOver)
        {
            // Flash the press R text
            if (restartText != null)
            {
                flashTimer += Time.deltaTime;
                float alpha = 0.3f + Mathf.PingPong(flashTimer * 2f, 0.7f);
                Color c = restartText.color;
                c.a = alpha;
                restartText.color = c;
            }

            // Keyboard input check to restart the level
            if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
            {
                RestartGame();
            }
        }
    }

    /// <summary>
    /// Computes the total points from selected elements list.
    /// </summary>
    public void InitializeSelectedPoints(List<string> selectedElements)
    {
        TotalReactionPoints = 0;
        IsGameOver = false;

        foreach (string el in selectedElements)
        {
            if (ElementPoints.TryGetValue(el, out int pts))
            {
                TotalReactionPoints += pts;
                Debug.Log($"[GameManager] Element: {el} is worth {pts} reaction points.");
            }
            else
            {
                // Fallback if element not found in dictionary
                TotalReactionPoints += 5;
                Debug.LogWarning($"[GameManager] Unknown element: {el}. Defaulting to 5 reaction points.");
            }
        }

        Debug.Log($"[GameManager] Elements loaded. Total reactor reactivity sum: {TotalReactionPoints} / {requiredPoints} Required.");
    }

    /// <summary>
    /// Invoked when player presses detonation button at the terminal.
    /// </summary>
    public void TryDetonate()
    {
        if (IsGameOver) return;

        Debug.Log($"[GameManager] Detonation requested! Evaluation reaction points: {TotalReactionPoints}");

        if (TotalReactionPoints > requiredPoints)
        {
            // Success! Detonate the planet
            PlanetBlaster blaster = Object.FindAnyObjectByType<PlanetBlaster>();
            if (blaster != null)
            {
                Debug.Log("[GameManager] Reaction CRITICALITY EXCEEDED! Initiating planetary destruction...");
                blaster.Detonate();
            }
            else
            {
                Debug.LogError("[GameManager] PlanetBlaster component not found in scene! Detonation aborted.");
            }
        }
        else
        {
            // Failure! Catastrophic meltdown vaporizes player immediately
            Debug.LogWarning("[GameManager] Reaction UNDER-CRITICAL! Catastrophic backfire vaporizes Mr. Blast!");
            KillPlayer();
        }
    }

    private void KillPlayer()
    {
        IsGameOver = true;

        GameObject playerObj = GameObject.Find("Mr.Blast");
        if (playerObj == null)
        {
            Debug.LogError("[GameManager] Player GameObject 'Mr.Blast' not found! Cannot execute vaporization visual effect.");
            CreateGameOverUI();
            return;
        }

        // 1. Disable player control and movement
        var charController = playerObj.GetComponent<SphericalCharacterController>();
        if (charController != null) charController.enabled = false;

        var rb = playerObj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        // 2. Hide player renderer to represent vaporization
        var renderer = playerObj.GetComponent<MeshRenderer>();
        if (renderer != null) renderer.enabled = false;

        var col = playerObj.GetComponent<Collider>();
        if (col != null) col.enabled = false;

        // 3. Procedurally disintegrate the player into scattering debris
        SpawnVaporizationDebris(playerObj.transform.position);

        // 4. Create the Game Over UI Screen
        CreateGameOverUI();
    }

    private void SpawnVaporizationDebris(Vector3 playerPos)
    {
        GameObject debrisContainer = new GameObject("Player_Debris_Container");
        debrisContainer.transform.position = playerPos;

        int numDebris = 18;
        List<GameObject> debrisList = new List<GameObject>();

        for (int i = 0; i < numDebris; i++)
        {
            GameObject piece = GameObject.CreatePrimitive(PrimitiveType.Cube);
            piece.name = $"VaporizedDebris_{i}";
            piece.transform.position = playerPos + Random.insideUnitSphere * 0.3f;
            piece.transform.rotation = Random.rotation;
            piece.transform.SetParent(debrisContainer.transform);

            // Set random size
            float size = Random.Range(0.12f, 0.28f);
            piece.transform.localScale = new Vector3(size, size, size);

            // Apply reddish/orange hot molten color material
            var pieceRenderer = piece.GetComponent<MeshRenderer>();
            if (pieceRenderer != null)
            {
                if (debrisMaterial != null)
                {
                    pieceRenderer.sharedMaterial = debrisMaterial;
                }
                else
                {
                    // Procedural material with hot core glowing color
                    Material mat = new Material(Shader.Find("Standard"));
                    mat.color = Color.Lerp(Color.red, new Color(1f, 0.4f, 0f), Random.value);
                    mat.EnableKeyword("_EMISSION");
                    mat.SetColor("_EmissionColor", mat.color * 2f);
                    pieceRenderer.sharedMaterial = mat;
                }
            }

            // Rigidbody simulation
            Rigidbody pieceRb = piece.AddComponent<Rigidbody>();
            pieceRb.useGravity = false; // We are in spherical space, so simulate custom explosion drift

            // Burst outwards from center
            Vector3 forceDir = Random.onUnitSphere;
            pieceRb.AddForce(forceDir * Random.Range(3f, 7f), ForceMode.Impulse);
            pieceRb.AddTorque(Random.insideUnitSphere * Random.Range(10f, 30f), ForceMode.Impulse);

            debrisList.Add(piece);
        }

        // Clean up debris smoothly over time
        StartCoroutine(ShrinkDebrisOverTime(debrisContainer, debrisList, 2.2f));
    }

    private IEnumerator ShrinkDebrisOverTime(GameObject container, List<GameObject> debrisList, float duration)
    {
        float elapsed = 0f;
        List<Vector3> startScales = new List<Vector3>();

        foreach (var piece in debrisList)
        {
            if (piece != null)
                startScales.Add(piece.transform.localScale);
            else
                startScales.Add(Vector3.zero);
        }

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            for (int i = 0; i < debrisList.Count; i++)
            {
                if (debrisList[i] != null)
                {
                    debrisList[i].transform.localScale = Vector3.Lerp(startScales[i], Vector3.zero, t);
                }
            }
            yield return null;
        }

        if (container != null)
        {
            Destroy(container);
        }
    }

    private void CreateGameOverUI()
    {
        Canvas canvas = Object.FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("[GameManager] Main Canvas not found in scene! Cannot display Game Over screen.");
            return;
        }

        // Create Panel
        GameObject panelGo = new GameObject("GameOverPanel");
        panelGo.transform.SetParent(canvas.transform, false);
        RectTransform panelRect = panelGo.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;

        Image panelImg = panelGo.AddComponent<Image>();
        panelImg.color = new Color(0.04f, 0.04f, 0.06f, 0.88f);

        // Vertical Layout Group to stack components nicely
        VerticalLayoutGroup layout = panelGo.AddComponent<VerticalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.spacing = 30f;
        layout.childControlHeight = false;
        layout.childControlWidth = false;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = false;

        // Title text
        GameObject titleGo = new GameObject("TitleText");
        titleGo.transform.SetParent(panelGo.transform, false);
        var titleRect = titleGo.AddComponent<RectTransform>();
        titleRect.sizeDelta = new Vector2(600f, 60f);
        var titleText = titleGo.AddComponent<TextMeshProUGUI>();
        titleText.text = "CRITICAL FAILURE";
        titleText.color = Color.red;
        titleText.fontSize = 32f;
        titleText.fontStyle = FontStyles.Bold;
        titleText.alignment = TextAlignmentOptions.Center;

        // Subtext / Description
        GameObject descGo = new GameObject("DescText");
        descGo.transform.SetParent(panelGo.transform, false);
        var descRect = descGo.AddComponent<RectTransform>();
        descRect.sizeDelta = new Vector2(600f, 120f);
        var descText = descGo.AddComponent<TextMeshProUGUI>();
        descText.text = $"Reactor Yield: <color=yellow>{TotalReactionPoints}</color> / {requiredPoints} Required\n\nReactivity was insufficient to trigger planetary detonation.\nThe core backfired, vaporizing Mr. Blast immediately!";
        descText.color = Color.white;
        descText.fontSize = 17f;
        descText.alignment = TextAlignmentOptions.Center;

        // Press R to restart
        GameObject restartGo = new GameObject("RestartText");
        restartGo.transform.SetParent(panelGo.transform, false);
        var restartRect = restartGo.AddComponent<RectTransform>();
        restartRect.sizeDelta = new Vector2(600f, 40f);
        restartText = restartGo.AddComponent<TextMeshProUGUI>();
        restartText.text = "Press [R] to Restart Cleanup";
        restartText.color = Color.yellow;
        restartText.fontSize = 16f;
        restartText.alignment = TextAlignmentOptions.Center;
    }

    private void RestartGame()
    {
        Debug.Log("[GameManager] Restarting Kickoff cleanup level...");
        IsGameOver = false;

        // Clean up the dynamic Game Over Panel
        GameObject panelGo = GameObject.Find("GameOverPanel");
        if (panelGo != null)
        {
            Destroy(panelGo);
        }

        // Reload the current active scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
