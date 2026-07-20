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

    [Header("Cockpit Persistent States")]
    public string selectedPlanetName = "Planet M";
    public bool upgradeCatalyst = false;
    public bool upgradeSupercritical = false;
    public bool upgradeHeavyWater = false;
    public int solacs = 150;
    public int highScore = 0;
    public string playerName = "Mr. Blast";

    public int TotalReactionPoints { get; private set; } = 0;
    public bool IsGameOver { get; private set; } = false;
    public bool IsCountdownActive { get; private set; } = false;
    public bool IsExplosionCinematicActive { get; set; } = false;

    private TextMeshProUGUI restartText;
    private float flashTimer = 0f;

    private TextMeshProUGUI _countdownText;
    private string _deathReasonMessage = "";

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

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        CreateInstructionsUI(scene.name);

        if (scene.name == "Preparation")
        {
            // Initial Panel State Correction: ensure perfect panel starting states regardless of Editor-time setup.
            var plotController = Object.FindAnyObjectByType<LoadingPlotController>(FindObjectsInactive.Include);
            if (plotController != null)
            {
                plotController.gameObject.SetActive(true);
            }

            var cockpit = Object.FindAnyObjectByType<CockpitManager>(FindObjectsInactive.Include);
            if (cockpit != null)
            {
                cockpit.gameObject.SetActive(false);
            }

            var selection = Object.FindAnyObjectByType<ElementSelectionManager>(FindObjectsInactive.Include);
            if (selection != null)
            {
                selection.gameObject.SetActive(false);
            }
        }

        if (scene.name == "Kickoff")
        {
            InitializeGameplayState();
        }
    }

    private void InitializeGameplayState()
    {
        // Clear game over and countdown state
        IsGameOver = false;
        IsCountdownActive = false;

        // Set planet-specific threshold
        if (selectedPlanetName == "Planet A") requiredPoints = 60;
        else if (selectedPlanetName == "Planet B") requiredPoints = 90;
        else if (selectedPlanetName == "Planet C") requiredPoints = 75;
        else if (selectedPlanetName == "Planet D") requiredPoints = 65;
        else requiredPoints = 80; // Default: Planet M

        // If SelectedElements is empty (e.g., developer launched Kickoff scene directly), fill with 10 default elements
        if (ElementSelectionManager.SelectedElements == null || ElementSelectionManager.SelectedElements.Count == 0)
        {
            Debug.LogWarning("[GameManager] SelectedElements list is empty. Populating with default high-reactivity elements for instant developer testing.");
            ElementSelectionManager.SelectedElements = new List<string>
            {
                "Uranium", "Plutonium", "Tritium", "Helium-3", "Deuterium",
                "Thorium", "Neptunium", "Radium", "Polonium", "Cesium"
            };

            // Register standard HSV colors for default elements so Inventory Bar renders beautifully
            for (int i = 0; i < ElementSelectionManager.SelectedElements.Count; i++)
            {
                float hue = i / 30f;
                Color shadeColor = Color.HSVToRGB(hue, 0.65f, 0.85f);
                ElementSelectionManager.ElementColors[ElementSelectionManager.SelectedElements[i]] = shadeColor;
            }
        }

        // Initialize the Inventory Bar
        InventoryBarManager invBar = Object.FindAnyObjectByType<InventoryBarManager>(FindObjectsInactive.Include);
        if (invBar != null)
        {
            invBar.InitializeInventory(ElementSelectionManager.SelectedElements);
        }
        else
        {
            Debug.LogWarning("[GameManager] InventoryBarManager not found in scene!");
        }

        // Initialize reaction points mapping
        InitializeSelectedPoints(ElementSelectionManager.SelectedElements);

        // Find the Count Down text dynamically
        GameObject cdGo = GameObject.Find("Count Down");
        if (cdGo != null)
        {
            _countdownText = cdGo.GetComponent<TextMeshProUGUI>();
            if (_countdownText != null)
            {
                _countdownText.text = "";
                _countdownText.gameObject.SetActive(false);
            }
        }

        // Ensure time scale is unpaused
        Time.timeScale = 1f;
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
        int basePoints = 0;
        IsGameOver = false;

        foreach (string el in selectedElements)
        {
            if (ElementPoints.TryGetValue(el, out int pts))
            {
                basePoints += pts;
                Debug.Log($"[GameManager] Element: {el} is worth {pts} reaction points.");
            }
            else
            {
                basePoints += 5;
                Debug.LogWarning($"[GameManager] Unknown element: {el}. Defaulting to 5 reaction points.");
            }
        }

        // Apply Heavy-Water Moderator multiplier upgrade (+10%)
        if (upgradeHeavyWater)
        {
            TotalReactionPoints = Mathf.RoundToInt(basePoints * 1.10f);
            Debug.Log($"[GameManager] Heavy-Water Moderator active! Multiplied points: {basePoints} -> {TotalReactionPoints}");
        }
        else
        {
            TotalReactionPoints = basePoints;
        }

        Debug.Log($"[GameManager] Elements loaded. Base points: {basePoints}. Active reactor points: {TotalReactionPoints} / {requiredPoints} Required.");
    }

    /// <summary>
    /// Invoked when player presses detonation button at the terminal.
    /// </summary>
    public void TryDetonate()
    {
        if (IsGameOver) return;

        Debug.Log($"[GameManager] Detonation requested! Evaluation reaction points: {TotalReactionPoints}");

        // Adjust target required points if Super-critical Core upgrade is active (-10 threshold)
        int currentRequired = requiredPoints;
        if (upgradeSupercritical)
        {
            currentRequired = Mathf.Max(10, requiredPoints - 10);
            Debug.Log($"[GameManager] Super-critical Core active! Lowered requirement from {requiredPoints} to {currentRequired}");
        }

        // Apply Catalyst Pre-heater upgrade (+10 points)
        int finalReactionPoints = TotalReactionPoints;
        if (upgradeCatalyst)
        {
            finalReactionPoints += 10;
            Debug.Log($"[GameManager] Catalyst Pre-heater active! Added +10 reactor points. Total: {finalReactionPoints}");
        }

        if (finalReactionPoints > currentRequired)
        {
            // Success! Detonate the planet
            PlanetBlaster blaster = null;
            GameObject playerObj = GameObject.Find("Mr.Blast");
            if (playerObj != null)
            {
                var scc = playerObj.GetComponent<SphericalCharacterController>();
                if (scc != null && scc.planet != null)
                {
                    blaster = scc.planet.GetComponent<PlanetBlaster>();
                }
            }
            if (blaster == null)
            {
                blaster = Object.FindAnyObjectByType<PlanetBlaster>();
            }

            if (blaster != null)
            {
                Debug.Log("[GameManager] Reaction CRITICALITY EXCEEDED! Initiating 10-second escape countdown...");
                
                // Calculate and record high score on explosion
                int finalScore = finalReactionPoints * 100;
                if (finalScore > highScore)
                {
                    highScore = finalScore;
                    Debug.Log($"[GameManager] NEW HIGH SCORE RECORDED: {highScore}");
                }

                StartCoroutine(CountdownAndDetonateRoutine(blaster));
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

    private IEnumerator CountdownAndDetonateRoutine(PlanetBlaster blaster)
    {
        IsCountdownActive = true;
        UpdateInstructions("<b>CRITICAL CORE DETONATION:</b> Return to the <color=#FF5500>Shuttle [F]</color> and escape immediately!");

        if (_countdownText != null)
        {
            _countdownText.gameObject.SetActive(true);
        }

        int countdownSecs = 10;
        while (countdownSecs > 0)
        {
            if (_countdownText != null)
            {
                _countdownText.text = $"<color=red>T-MINUS {countdownSecs}</color>";
            }
            Debug.LogFormat("[GameManager] Detonation Countdown: T-MINUS {0}", countdownSecs);

            yield return new WaitForSeconds(1.0f);
            countdownSecs--;
        }

        if (_countdownText != null)
        {
            _countdownText.text = "";
            _countdownText.gameObject.SetActive(false);
        }

        IsCountdownActive = false;

        // Check if player successfully boarded the shuttle
        ShuttleController shuttle = Object.FindAnyObjectByType<ShuttleController>();
        if (shuttle != null && shuttle.isPiloted)
        {
            Debug.Log("[GameManager] Countdown reached 0. Mr.Blast is safe inside the Shuttle! Executing planetary detonation cinematic.");
            StartCoroutine(TransitionToSecondaryCamera(blaster));
            blaster.Detonate();
        }
        else
        {
            Debug.LogWarning("[GameManager] Countdown reached 0. Mr.Blast failed to board the shuttle in time!");
            KillPlayer("T-MINUS ZERO REACHED!\nYou failed to board the shuttle and escape the planet before detonation!");
        }
    }

    private IEnumerator TransitionToSecondaryCamera(PlanetBlaster blaster)
    {
        IsExplosionCinematicActive = true;
        UpdateInstructions("<b>ORBITAL COLLAPSE:</b> Reactor core detonating! Watch planetary vaporization...");

        Transform targetPlanet = blaster.transform;
        GameObject secCam = GameObject.Find("SecondaryCamera");
        if (secCam == null)
        {
            Debug.LogWarning("[GameManager] SecondaryCamera not found in scene! Camera transition bypassed.");
            yield break;
        }

        // Dynamically position and orient the secondary camera based on the detonating planet
        if (targetPlanet != null)
        {
            float radius = GetPlanetRadius(targetPlanet.gameObject);
            // Position camera well outside the planet visual bounds (3.2x actual radius is ideal)
            float targetDistance = radius * 3.2f;
            secCam.transform.position = targetPlanet.position + Vector3.back * targetDistance;
            secCam.transform.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);
            Debug.LogFormat("[GameManager] Dynamically aligned SecondaryCamera relative to detonating planet '{0}' (Pos: {1}, Distance: {2}, Calculated Radius: {3})", 
                targetPlanet.name, secCam.transform.position, targetDistance, radius);
        }

        var mainCam = Camera.main;
        if (mainCam == null)
        {
            Debug.LogError("[GameManager] Main Camera not found in scene!");
            yield break;
        }

        var followScript = mainCam.GetComponent<SphericalCameraFollow>();
        if (followScript != null)
        {
            followScript.isCinematicActive = true;
        }

        Vector3 startPos = mainCam.transform.position;
        Quaternion startRot = mainCam.transform.rotation;

        Vector3 targetPos = secCam.transform.position;
        Quaternion targetRot = secCam.transform.rotation;

        float elapsed = 0f;
        float panDuration = 2.0f; // Pacing: smoothly pan over 2.0 seconds during the buildup

        // --- 1. Transition TO Secondary Camera ---
        while (elapsed < panDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / panDuration);
            float smoothT = t * t * (3f - 2f * t);

            mainCam.transform.position = Vector3.Lerp(startPos, targetPos, smoothT);
            mainCam.transform.rotation = Quaternion.Slerp(startRot, targetRot, smoothT);
            yield return null;
        }

        mainCam.transform.position = targetPos;
        mainCam.transform.rotation = targetRot;
        Debug.Log("[GameManager] Cinematic camera transition to SecondaryCamera complete. Watching explosion...");

        // --- 2. Wait for Explosion Duration ---
        // Total wait: Buildup + Blast + brief settle time
        float totalWait = blaster.buildUpDuration + blaster.blastDuration + 1.5f;
        yield return new WaitForSeconds(totalWait);

        Debug.Log("[GameManager] Explosion sequence finished. Returning camera to player...");

        // --- 3. Transition BACK to Player/Shuttle ---
        elapsed = 0f;
        float returnDuration = 2.5f;
        Vector3 finalCamStartPos = mainCam.transform.position;
        Quaternion finalCamStartRot = mainCam.transform.rotation;

        while (elapsed < returnDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / returnDuration);
            float smoothT = t * t * (3f - 2f * t);

            // Dynamically calculate the target return position (handles shuttle movement during blast)
            Vector3 returnPos;
            Quaternion returnRot;

            ShuttleController shuttle = Object.FindAnyObjectByType<ShuttleController>();
            if (shuttle != null && shuttle.isPiloted)
            {
                returnPos = shuttle.transform.position - shuttle.transform.forward * shuttle.cameraDistance + shuttle.transform.up * shuttle.cameraHeight;
                Vector3 lookTarget = shuttle.transform.position + shuttle.transform.forward * 4f;
                returnRot = Quaternion.LookRotation(lookTarget - returnPos, shuttle.transform.up);
            }
            else
            {
                // Fallback to Mr.Blast follow (even though he's usually launched/disabled, this handles ground death etc)
                returnPos = finalCamStartPos; // Placeholder if no valid target
                returnRot = finalCamStartRot;
            }

            mainCam.transform.position = Vector3.Lerp(finalCamStartPos, returnPos, smoothT);
            mainCam.transform.rotation = Quaternion.Slerp(finalCamStartRot, returnRot, smoothT);
            yield return null;
        }

        if (followScript != null)
        {
            followScript.isCinematicActive = false;
        }
        IsExplosionCinematicActive = false;
        UpdateInstructions("<b>CLEANUP COMPLETE:</b> Target planet successfully vaporized! Pilot the Shuttle into orbit.");
        Debug.Log("[GameManager] Cinematic camera returned to player control.");
    }

    private void KillPlayer(string customReason = null)
    {
        IsGameOver = true;

        if (string.IsNullOrEmpty(customReason))
        {
            _deathReasonMessage = $"Reactor Yield: <color=yellow>{TotalReactionPoints}</color> / {requiredPoints} Required\n\nReactivity was insufficient to trigger planetary detonation.\nThe core backfired, vaporizing Mr. Blast immediately!";
        }
        else
        {
            _deathReasonMessage = customReason;
        }

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
        descText.text = _deathReasonMessage;
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

    private TextMeshProUGUI _instructionsText;
    private GameObject _instructionsHUD;

    public void SetInstructionsVisible(bool visible)
    {
        if (_instructionsHUD == null)
        {
            GameObject canvas = GameObject.Find("Canvas");
            if (canvas != null)
            {
                Transform hudTrans = canvas.transform.Find("GameplayInstructionsHUD");
                if (hudTrans != null)
                {
                    _instructionsHUD = hudTrans.gameObject;
                }
            }
        }

        if (_instructionsHUD != null)
        {
            _instructionsHUD.SetActive(visible);
        }
    }

    private void CreateInstructionsUI(string sceneName)
    {
        GameObject existing = GameObject.Find("GameplayInstructionsHUD");
        if (existing != null)
        {
            Destroy(existing);
        }

        GameObject canvas = GameObject.Find("Canvas");
        if (canvas == null) return;

        GameObject hudGo = new GameObject("GameplayInstructionsHUD");
        _instructionsHUD = hudGo;
        hudGo.transform.SetParent(canvas.transform, false);

        RectTransform rect = hudGo.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(0f, -15f);
        rect.sizeDelta = new Vector2(620f, 32f);

        Image img = hudGo.AddComponent<Image>();
        img.color = new Color(0.04f, 0.04f, 0.06f, 0.88f);

        Outline outline = hudGo.AddComponent<Outline>();
        outline.effectColor = new Color(0f, 0.85f, 1f, 0.5f);
        outline.effectDistance = new Vector2(1.5f, -1.5f);

        GameObject textGo = new GameObject("InstructionText");
        textGo.transform.SetParent(hudGo.transform, false);

        RectTransform textRect = textGo.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.offsetMin = new Vector2(8f, 2f);
        textRect.offsetMax = new Vector2(-8f, -2f);

        _instructionsText = textGo.AddComponent<TextMeshProUGUI>();
        _instructionsText.fontSize = 11.5f;
        _instructionsText.alignment = TextAlignmentOptions.Center;
        _instructionsText.color = Color.white;
        _instructionsText.textWrappingMode = TextWrappingModes.Normal;

        if (sceneName == "Preparation")
        {
            _instructionsText.text = "<b>MISSION SETUP:</b> <color=#00FFFF>Select Planet</color> to scan coordinates & <color=#FFFF00>Select Upgrades</color> to optimize core reaction!";
            
            // Hide HUD initially if LoadingPlotController is present in the scene
            if (Object.FindAnyObjectByType<LoadingPlotController>() != null)
            {
                hudGo.SetActive(false);
            }
        }
        else if (sceneName == "Kickoff")
        {
            _instructionsText.text = "<b>MISSION HAZARD:</b> Locate and interact with the <color=#00FFFF>Control Terminal [F]</color> to prime core reaction!";
        }
    }

    public void UpdateInstructions(string text)
    {
        if (_instructionsText != null)
        {
            _instructionsText.text = text;
        }
    }

    private float GetPlanetRadius(GameObject planetGo)
    {
        var sphereCol = planetGo.GetComponent<SphereCollider>();
        if (sphereCol != null)
        {
            return sphereCol.radius * planetGo.transform.localScale.x;
        }

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

        return planetGo.transform.localScale.x * 0.5f;
    }
}
