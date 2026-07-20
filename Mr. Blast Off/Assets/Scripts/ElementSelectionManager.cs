using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class ElementSelectionManager : MonoBehaviour
{
    // Globally accessible data structure containing selected element names
    public static List<string> SelectedElements = new List<string>();
    // Globally accessible color dictionary mapping element names to their HSV color shades
    public static Dictionary<string, Color> ElementColors = new Dictionary<string, Color>();
    // Globally accessible preloaded sprites dictionary
    public static readonly Dictionary<string, Sprite> ElementSprites = new Dictionary<string, Sprite>();

    public struct ElementThematicData
    {
        public string category;
        public string chemicalSymbol;
        public string description;
        public int yield;
    }
    private static readonly Dictionary<string, ElementThematicData> ThematicDatabase = new Dictionary<string, ElementThematicData>();

    [Header("UI References")]
    [Tooltip("The Slot 1 object already in the scene to be used as a template")]
    public GameObject slotTemplate;
    [Tooltip("The Start Button already in the scene")]
    public Button startButton;

    [Header("Procedural Layout Customization")]
    public bool generateLayoutProcedurally = true;

    [Header("Custom UI References (Overrides Procedural)")]
    public GameObject customScrollView;
    public RectTransform customGridRect;
    public TextMeshProUGUI customStatusText;

    [Header("Scroll View Settings")]
    public Vector2 scrollViewSizeDelta = new Vector2(360f, 140f);
    public Vector2 scrollViewAnchoredPosition = new Vector2(0f, 20f);

    [Header("Grid Layout Settings")]
    public Vector2 cellSize = new Vector2(50f, 50f);
    public Vector2 cellSpacing = new Vector2(12f, 12f);
    public int gridColumnCount = 5;

    [Header("Status Text Settings")]
    public Vector2 statusTextSizeDelta = new Vector2(360f, 36f);
    public Vector2 statusTextAnchoredPosition = new Vector2(0f, -60f);
    public float statusTextFontSize = 13f;
    public Color statusTextColor = Color.yellow;

    [Header("Start Button Settings")]
    public Vector2 startButtonSizeDelta = new Vector2(160f, 30f);
    public Vector2 startButtonAnchoredPosition = new Vector2(0f, -100f);

    private RectTransform panelRect;
    private RectTransform gridRect;
    private TextMeshProUGUI statusText;
    private List<ElementSlot> allSlots = new List<ElementSlot>();
    private List<ElementSlot> selectedSlots = new List<ElementSlot>();

    private GameObject proceduralScrollView;
    private GridLayoutGroup proceduralGridLayout;
    private ContentSizeFitter proceduralContentSizeFitter;

    // 30 Sci-Fi and Nuclear Engineering Element Names
    private readonly string[] elementNames = new string[30]
    {
        "Uranium", "Plutonium", "Thorium", "Neptunium", "Radium",
        "Polonium", "Deuterium", "Tritium", "Lithium", "Carbon",
        "Beryllium", "Lead", "Cadmium", "Boron", "Graphite",
        "Steel", "Copper", "Iron", "Titanium", "Xenon",
        "Krypton", "Helium-3", "Sodium", "Zirconium", "Cesium",
        "Cobalt", "Nickel", "Tungsten", "Gold", "Platinum"
    };

    private void Awake()
    {
        // 1. Pause game on load if in the Kickoff scene (for backward compatibility), otherwise keep running
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Kickoff")
        {
            Time.timeScale = 0f;
            Debug.Log("[ElementSelectionManager] Game PAUSED for Element Selection phase.");
        }
        else
        {
            Time.timeScale = 1f;
            Debug.Log("[ElementSelectionManager] Preparation Menu loaded. Time scale initialized to 1.");
        }

        // Cache elements sprites & setup database
        PreloadSpritesAndDatabase();

        panelRect = GetComponent<RectTransform>();

        if (generateLayoutProcedurally)
        {
            CreateGridContainer();
            CreateStatusText();
        }
        else
        {
            // Use custom overrides
            if (customScrollView != null)
            {
                customScrollView.SetActive(true);
            }
            if (customGridRect != null)
            {
                gridRect = customGridRect;
            }
            else
            {
                CreateGridContainer();
            }

            if (customStatusText != null)
            {
                statusText = customStatusText;
            }
            else
            {
                CreateStatusText();
            }
        }

        GenerateSlots();
        ConfigureStartButton();
        ApplyLayoutParameters();
        UpdateStatusDisplay();
    }

    private void PreloadSpritesAndDatabase()
    {
        ElementSprites.Clear();
        foreach (var name in elementNames)
        {
            Sprite sprite = Resources.Load<Sprite>($"elements/{name}");
            if (sprite != null)
            {
                ElementSprites[name] = sprite;
            }
            else
            {
                Debug.LogWarning($"[ElementSelectionManager] Could not load sprite at Resource elements/{name}");
            }
        }

        ThematicDatabase.Clear();
        
        // Actinides
        ThematicDatabase["Uranium"] = new ElementThematicData { category = "Actinide Fuel", chemicalSymbol = "235-U", description = "Standard enriched fuel. Heavy nuclear fission core.", yield = 10 };
        ThematicDatabase["Plutonium"] = new ElementThematicData { category = "Actinide Fuel", chemicalSymbol = "239-Pu", description = "Highly fissile isotope. Extreme neutron propagation.", yield = 10 };
        ThematicDatabase["Thorium"] = new ElementThematicData { category = "Actinide Fuel", chemicalSymbol = "232-Th", description = "Breeder fuel source. Highly stable until irradiated.", yield = 9 };
        ThematicDatabase["Neptunium"] = new ElementThematicData { category = "Actinide Fuel", chemicalSymbol = "237-Np", description = "Transuranic byproduct. Generates high energy beta decay.", yield = 9 };
        ThematicDatabase["Radium"] = new ElementThematicData { category = "Actinide Fuel", chemicalSymbol = "226-Ra", description = "Luminescent hazard. Intense primordial radioactivity.", yield = 8 };
        ThematicDatabase["Polonium"] = new ElementThematicData { category = "Actinide Fuel", chemicalSymbol = "210-Po", description = "Volatile alpha emitter. Produces localized heating.", yield = 8 };
        ThematicDatabase["Cesium"] = new ElementThematicData { category = "Actinide Fuel", chemicalSymbol = "137-Cs", description = "Highly unstable isotope. Spontaneous gamma emission.", yield = 8 };

        // Thermonuclear
        ThematicDatabase["Tritium"] = new ElementThematicData { category = "Thermonuclear Fuel", chemicalSymbol = "3-H", description = "Super-heavy hydrogen. Ideal for fusion initiation.", yield = 10 };
        ThematicDatabase["Helium-3"] = new ElementThematicData { category = "Thermonuclear Fuel", chemicalSymbol = "3-He", description = "Rare lunar isotope. High-energy clean fusion agent.", yield = 10 };
        ThematicDatabase["Deuterium"] = new ElementThematicData { category = "Thermonuclear Fuel", chemicalSymbol = "2-H", description = "Heavy hydrogen. Slows neutrons to maximize reaction.", yield = 9 };
        ThematicDatabase["Lithium"] = new ElementThematicData { category = "Thermonuclear Fuel", chemicalSymbol = "6-Li", description = "Tritium breeder. Undergoes high-yield alpha fusion.", yield = 6 };

        // Moderators / Shielding / Absorbers
        ThematicDatabase["Carbon"] = new ElementThematicData { category = "Neutron Moderator", chemicalSymbol = "C", description = "Light structural core. Regulates atomic density.", yield = 5 };
        ThematicDatabase["Beryllium"] = new ElementThematicData { category = "Neutron Moderator", chemicalSymbol = "Be", description = "Neutron reflector. Bounces escaping energy back to core.", yield = 4 };
        ThematicDatabase["Graphite"] = new ElementThematicData { category = "Neutron Moderator", chemicalSymbol = "C(Gr)", description = "High-purity carbon blocks. Stabilizes runaway neutrons.", yield = 3 };
        ThematicDatabase["Boron"] = new ElementThematicData { category = "Neutron Absorber", chemicalSymbol = "B", description = "Thermal absorber. Prevents premature reactor meltdown.", yield = 2 };
        ThematicDatabase["Steel"] = new ElementThematicData { category = "Heavy Shielding", chemicalSymbol = "Fe-Cr", description = "Reinforced structural casing. Confines thermal waves.", yield = 2 };
        ThematicDatabase["Lead"] = new ElementThematicData { category = "Radiation Shielding", chemicalSymbol = "Pb", description = "Dense isotope barrier. Heavy protection against gamma leaks.", yield = 1 };
        ThematicDatabase["Cadmium"] = new ElementThematicData { category = "Neutron Absorber", chemicalSymbol = "Cd", description = "Emergency control rod material. Dampens neutron flux.", yield = 1 };

        // Core / Transition Metals
        ThematicDatabase["Platinum"] = new ElementThematicData { category = "Precious Conductor", chemicalSymbol = "Pt", description = "Catalytic surface agent. Facilitates seamless heat transfer.", yield = 8 };
        ThematicDatabase["Cobalt"] = new ElementThematicData { category = "Magnetic Metal", chemicalSymbol = "60-Co", description = "Ferromagnetic catalyst. Emits high-energy beta flux.", yield = 7 };
        ThematicDatabase["Tungsten"] = new ElementThematicData { category = "Refractory Metal", chemicalSymbol = "W", description = "Heavy thermal barrier. Withstands extreme blast pressures.", yield = 7 };
        ThematicDatabase["Gold"] = new ElementThematicData { category = "Heavy Reflector", chemicalSymbol = "Au", description = "High-density shield. Reflects electromagnetic radiation.", yield = 7 };
        ThematicDatabase["Zirconium"] = new ElementThematicData { category = "Cladding Material", chemicalSymbol = "Zr", description = "Low-absorption cladding. Confines radioactive gases.", yield = 6 };
        ThematicDatabase["Nickel"] = new ElementThematicData { category = "Transition Metal", chemicalSymbol = "Ni", description = "Corrosion-resistant catalyst. Strengthens reactor grid.", yield = 6 };
        ThematicDatabase["Sodium"] = new ElementThematicData { category = "Coolant Agent", chemicalSymbol = "Na", description = "Liquid metal coolant. Conducts thermal currents rapidly.", yield = 5 };
        ThematicDatabase["Titanium"] = new ElementThematicData { category = "Structural Alloy", chemicalSymbol = "Ti", description = "Ultra-strong framework. Resists thermal cracking.", yield = 5 };
        ThematicDatabase["Xenon"] = new ElementThematicData { category = "Noble Gas", chemicalSymbol = "Xe", description = "Heavy gaseous buffer. Limits premature neutron cascade.", yield = 4 };
        ThematicDatabase["Krypton"] = new ElementThematicData { category = "Noble Gas", chemicalSymbol = "Kr", description = "Unreactive atmosphere. Absorbs energetic plasma waves.", yield = 4 };
        ThematicDatabase["Copper"] = new ElementThematicData { category = "Thermal Conductor", chemicalSymbol = "Cu", description = "High-grade conduit. Transmits electricity instantly.", yield = 3 };
        ThematicDatabase["Iron"] = new ElementThematicData { category = "Magnetic Catalyst", chemicalSymbol = "Fe", description = "Standard core isotope. Forms structural device basis.", yield = 3 };
    }

    public void SetHoveredElement(string name)
    {
        if (statusText == null) return;

        if (string.IsNullOrEmpty(name))
        {
            UpdateStatusDisplay();
            return;
        }

        if (ThematicDatabase.TryGetValue(name, out ElementThematicData data))
        {
            string categoryColor = "#00FFFF"; // default cyan
            if (data.category.Contains("Fuel")) categoryColor = "#FF5500"; // Orange/red for fuel
            else if (data.category.Contains("Moderator") || data.category.Contains("Absorber")) categoryColor = "#00FF66"; // Green for regulation
            else if (data.category.Contains("Shielding")) categoryColor = "#AAAAAA"; // Gray for shield
            else categoryColor = "#FFFF00"; // Yellow for core transition metals

            statusText.text = $"<b><color={categoryColor}>{name} ({data.chemicalSymbol})</color></b> | Yield: <b><color=orange>{data.yield}/10</color></b>\n<size=10.5f>{data.description}</size>";
        }
    }

    private void CreateGridContainer()
    {
        // 1. Create the Scroll View Container Game Object
        GameObject scrollViewGo = new GameObject("ScrollView");
        proceduralScrollView = scrollViewGo;
        scrollViewGo.transform.SetParent(transform, false);

        RectTransform scrollRectTransform = scrollViewGo.AddComponent<RectTransform>();
        scrollRectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        scrollRectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        scrollRectTransform.pivot = new Vector2(0.5f, 0.5f);
        scrollRectTransform.anchoredPosition = scrollViewAnchoredPosition;
        scrollRectTransform.sizeDelta = scrollViewSizeDelta;

        ScrollRect scrollRect = scrollViewGo.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Elastic;

        // 2. Create the Viewport Game Object for clipping/masking
        GameObject viewportGo = new GameObject("Viewport");
        viewportGo.transform.SetParent(scrollViewGo.transform, false);

        RectTransform viewportRect = viewportGo.AddComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.pivot = new Vector2(0.5f, 0.5f);
        viewportRect.sizeDelta = Vector2.zero;

        // Add Image and Mask components to clip slots going beyond viewport bounds
        viewportGo.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.05f); // subtle tint background
        viewportGo.AddComponent<RectMask2D>();

        // 3. Create the Content Grid Game Object
        GameObject gridGo = new GameObject("Grid");
        gridGo.transform.SetParent(viewportGo.transform, false);

        gridRect = gridGo.AddComponent<RectTransform>();
        // Top-align and stretch horizontally
        gridRect.anchorMin = new Vector2(0f, 1f);
        gridRect.anchorMax = new Vector2(1f, 1f);
        gridRect.pivot = new Vector2(0.5f, 1f);
        gridRect.anchoredPosition = Vector2.zero;
        gridRect.sizeDelta = new Vector2(0f, 0f);

        // Add GridLayoutGroup
        proceduralGridLayout = gridGo.AddComponent<GridLayoutGroup>();
        proceduralGridLayout.cellSize = cellSize; // Preserve original template slot size
        proceduralGridLayout.spacing = cellSpacing;
        proceduralGridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        proceduralGridLayout.constraintCount = gridColumnCount; // Fit exactly 5 columns nicely in 360 width
        proceduralGridLayout.childAlignment = TextAnchor.UpperCenter;

        // Add ContentSizeFitter to dynamically expand content height vertically based on slots
        proceduralContentSizeFitter = gridGo.AddComponent<ContentSizeFitter>();
        proceduralContentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // Link the ScrollRect properties
        scrollRect.content = gridRect;
        scrollRect.viewport = viewportRect;
    }

    private void CreateStatusText()
    {
        GameObject textGo = new GameObject("StatusText");
        textGo.transform.SetParent(transform, false);

        RectTransform textRect = textGo.AddComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.5f, 0.5f);
        textRect.anchorMax = new Vector2(0.5f, 0.5f);
        textRect.pivot = new Vector2(0.5f, 0.5f);
        textRect.anchoredPosition = statusTextAnchoredPosition;
        textRect.sizeDelta = statusTextSizeDelta;

        statusText = textGo.AddComponent<TextMeshProUGUI>();
        statusText.fontSize = statusTextFontSize;
        statusText.alignment = TextAlignmentOptions.Center;
        statusText.color = statusTextColor;
        statusText.text = "Selected elements: 0 / 10";
    }

    public void ApplyLayoutParameters()
    {
        // 1. Scroll View Layout
        GameObject sView = generateLayoutProcedurally ? proceduralScrollView : customScrollView;
        if (sView != null)
        {
            RectTransform sViewRect = sView.GetComponent<RectTransform>();
            if (sViewRect != null)
            {
                sViewRect.anchorMin = new Vector2(0.5f, 0.5f);
                sViewRect.anchorMax = new Vector2(0.5f, 0.5f);
                sViewRect.pivot = new Vector2(0.5f, 0.5f);
                sViewRect.anchoredPosition = scrollViewAnchoredPosition;
                sViewRect.sizeDelta = scrollViewSizeDelta;
            }
        }

        // 2. Grid Layout properties
        var gridLayout = gridRect != null ? gridRect.GetComponent<GridLayoutGroup>() : null;
        if (gridLayout != null)
        {
            gridLayout.cellSize = cellSize;
            gridLayout.spacing = cellSpacing;
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = gridColumnCount;
        }

        // 3. Status Text Layout & Styling
        if (statusText != null)
        {
            RectTransform statusRect = statusText.GetComponent<RectTransform>();
            if (statusRect != null)
            {
                statusRect.anchorMin = new Vector2(0.5f, 0.5f);
                statusRect.anchorMax = new Vector2(0.5f, 0.5f);
                statusRect.pivot = new Vector2(0.5f, 0.5f);
                statusRect.anchoredPosition = statusTextAnchoredPosition;
                statusRect.sizeDelta = statusTextSizeDelta;
            }
            statusText.fontSize = statusTextFontSize;
            statusText.color = statusTextColor;
        }

        // 4. Start Button Layout
        if (startButton != null)
        {
            RectTransform startRect = startButton.GetComponent<RectTransform>();
            if (startRect != null)
            {
                startRect.anchorMin = new Vector2(0.5f, 0.5f);
                startRect.anchorMax = new Vector2(0.5f, 0.5f);
                startRect.pivot = new Vector2(0.5f, 0.5f);
                startRect.anchoredPosition = startButtonAnchoredPosition;
                startRect.sizeDelta = startButtonSizeDelta;
            }
        }
    }

    private void OnEnable()
    {
        ApplyLayoutParameters();
    }

#if UNITY_EDITOR
    private void Update()
    {
        if (Application.isPlaying)
        {
            ApplyLayoutParameters();
        }
    }
#endif

    private void GenerateSlots()
    {
        if (slotTemplate == null)
        {
            Debug.LogError("[ElementSelectionManager] Slot Template is null! Cannot generate slots.");
            return;
        }

        // Generate 30 slots using Slot 1 as template and cloning it
        for (int i = 0; i < 30; i++)
        {
            GameObject slotInstance;
            if (i == 0)
            {
                // Reparent and reuse the original Slot 1 so there are no duplicates
                slotInstance = slotTemplate;
                slotInstance.transform.SetParent(gridRect, false);
            }
            else
            {
                slotInstance = Instantiate(slotTemplate, gridRect, false);
                slotInstance.name = $"Slot {i + 1}";
            }

            slotInstance.SetActive(true);

            // Add ElementSlot component to handle interactive click callbacks
            ElementSlot elementSlot = slotInstance.GetComponent<ElementSlot>();
            if (elementSlot == null)
            {
                elementSlot = slotInstance.AddComponent<ElementSlot>();
            }

            // Generate 30 distinct shades of colors using HSV space
            float hue = i / 30f;
            Color shadeColor = Color.HSVToRGB(hue, 0.65f, 0.85f);

            // Register the element color in our global dictionary
            ElementColors[elementNames[i]] = shadeColor;

            // Initialize slot name, unique color and registration
            elementSlot.Init(elementNames[i], shadeColor, OnSlotClicked);
            allSlots.Add(elementSlot);
        }
    }

    private void ConfigureStartButton()
    {
        if (startButton != null)
        {
            RectTransform startRect = startButton.GetComponent<RectTransform>();
            if (startRect != null)
            {
                startRect.anchorMin = new Vector2(0.5f, 0.5f);
                startRect.anchorMax = new Vector2(0.5f, 0.5f);
                startRect.pivot = new Vector2(0.5f, 0.5f);
                startRect.anchoredPosition = new Vector2(0f, -100f);
                startRect.sizeDelta = new Vector2(160f, 30f);
            }

            // Remove any existing listeners and hook up our OnStartClicked method
            startButton.onClick.RemoveAllListeners();
            startButton.onClick.AddListener(OnStartClicked);
        }
        else
        {
            Debug.LogError("[ElementSelectionManager] Start Button reference missing!");
        }
    }

    private void OnSlotClicked(ElementSlot slot)
    {
        if (slot.isSelected)
        {
            // Toggle selection off
            slot.SetSelected(false);
            selectedSlots.Remove(slot);
        }
        else
        {
            // Toggle selection on, if under the 10 limit
            if (selectedSlots.Count < 10)
            {
                slot.SetSelected(true);
                selectedSlots.Add(slot);
            }
            else
            {
                // Play warning feedback (visual pulse/warning status update)
                statusText.text = "<color=red>Max 10 elements reached! Deselect an element first.</color>";
                return;
            }
        }

        UpdateStatusDisplay();
    }

    private static readonly Dictionary<string, int> ElementYieldPoints = new Dictionary<string, int>
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

    private void UpdateStatusDisplay()
    {
        if (statusText != null)
        {
            int count = selectedSlots.Count;
            
            // Calculate current total reaction points
            int currentYield = 0;
            foreach (var slot in selectedSlots)
            {
                if (ElementYieldPoints.TryGetValue(slot.elementName, out int pts))
                {
                    currentYield += pts;
                }
            }

            // Apply Heavy-Water Moderator multiplier upgrade (+10%) if purchased
            if (GameManager.Instance != null && GameManager.Instance.upgradeHeavyWater)
            {
                currentYield = Mathf.RoundToInt(currentYield * 1.10f);
            }

            // Apply Catalyst Pre-heater (+10 default points) if purchased
            if (GameManager.Instance != null && GameManager.Instance.upgradeCatalyst)
            {
                currentYield += 10;
            }

            // Calculate required points based on selected planet
            int baseRequired = 80; // Default: Planet M
            string planetName = "Planet M";
            if (GameManager.Instance != null)
            {
                planetName = GameManager.Instance.selectedPlanetName;
                if (planetName == "Planet A") baseRequired = 60;
                else if (planetName == "Planet B") baseRequired = 90;
                else if (planetName == "Planet C") baseRequired = 75;
                else if (planetName == "Planet D") baseRequired = 65;
                else baseRequired = 80;
            }

            int currentRequired = baseRequired;
            if (GameManager.Instance != null && GameManager.Instance.upgradeSupercritical)
            {
                currentRequired = Mathf.Max(10, baseRequired - 10);
            }

            // Format status text elegantly
            string progressColor = (currentYield >= currentRequired) ? "#00FF66" : "#FFFF00";
            
            statusText.text = $"Selected: <color=yellow>{count}/10</color> | Yield: <color={progressColor}>{currentYield}</color> / {currentRequired} Required <size=11f>({planetName})</size>";
        }
    }

    private void OnStartClicked()
    {
        if (selectedSlots.Count == 10)
        {
            // 1. Populating the SelectedElements list data structure
            SelectedElements.Clear();
            foreach (var slot in selectedSlots)
            {
                SelectedElements.Add(slot.elementName);
            }

            Debug.Log($"[ElementSelectionManager] Core elements stabilized: {string.Join(", ", SelectedElements)}");

            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Preparation")
            {
                // Transition to Kickoff scene
                Time.timeScale = 1f;
                UnityEngine.SceneManagement.SceneManager.LoadScene("Kickoff");
            }
            else
            {
                // Initialize and show the playtime Inventory Bar with selected elements (for fallback/development play from Kickoff scene)
                InventoryBarManager invBar = Object.FindAnyObjectByType<InventoryBarManager>(FindObjectsInactive.Include);
                if (invBar != null)
                {
                    invBar.InitializeInventory(SelectedElements);
                }
                else
                {
                    Debug.LogWarning("[ElementSelectionManager] InventoryBarManager not found in scene!");
                }

                // Register selected elements with the GameManager to compute reaction points
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.InitializeSelectedPoints(SelectedElements);
                }
                else
                {
                    Debug.LogWarning("[ElementSelectionManager] GameManager.Instance is null! Reactivity calculation might be bypassed.");
                }

                // 2. Unpause and start the game!
                Time.timeScale = 1f;
                Debug.Log("[ElementSelectionManager] Game unpaused! Let the engineering cleanup begin.");

                // 3. Deactivate the Element Selection Panel
                gameObject.SetActive(false);
            }
        }
        else
        {
            // Display error warning if count is incorrect
            if (statusText != null)
            {
                statusText.text = $"<color=red>Selection error! Please choose exactly 10 elements (Current: {selectedSlots.Count}/10).</color>";
            }
        }
    }
}
