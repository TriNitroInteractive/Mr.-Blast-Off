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

    [Header("UI References")]
    [Tooltip("The Slot 1 object already in the scene to be used as a template")]
    public GameObject slotTemplate;
    [Tooltip("The Start Button already in the scene")]
    public Button startButton;

    private RectTransform panelRect;
    private RectTransform gridRect;
    private TextMeshProUGUI statusText;
    private List<ElementSlot> allSlots = new List<ElementSlot>();
    private List<ElementSlot> selectedSlots = new List<ElementSlot>();

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
        // 1. Pause game immediately on load
        Time.timeScale = 0f;
        Debug.Log("[ElementSelectionManager] Game PAUSED for Element Selection phase.");

        panelRect = GetComponent<RectTransform>();
        CreateGridContainer();
        CreateStatusText();
        GenerateSlots();
        ConfigureStartButton();
        UpdateStatusDisplay();
    }

    private void CreateGridContainer()
    {
        // 1. Create the Scroll View Container Game Object
        GameObject scrollViewGo = new GameObject("ScrollView");
        scrollViewGo.transform.SetParent(transform, false);

        RectTransform scrollRectTransform = scrollViewGo.AddComponent<RectTransform>();
        scrollRectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        scrollRectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        scrollRectTransform.pivot = new Vector2(0.5f, 0.5f);
        scrollRectTransform.anchoredPosition = new Vector2(0f, 20f);
        scrollRectTransform.sizeDelta = new Vector2(360f, 140f);

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
        GridLayoutGroup gridLayout = gridGo.AddComponent<GridLayoutGroup>();
        gridLayout.cellSize = new Vector2(50f, 50f); // Preserve original template slot size
        gridLayout.spacing = new Vector2(12f, 12f);
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = 5; // Fit exactly 5 columns nicely in 360 width
        gridLayout.childAlignment = TextAnchor.UpperCenter;

        // Add ContentSizeFitter to dynamically expand content height vertically based on slots
        ContentSizeFitter fitter = gridGo.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

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
        textRect.anchoredPosition = new Vector2(0f, -65f);
        textRect.sizeDelta = new Vector2(360f, 25f);

        statusText = textGo.AddComponent<TextMeshProUGUI>();
        statusText.fontSize = 13f;
        statusText.alignment = TextAlignmentOptions.Center;
        statusText.color = Color.yellow;
        statusText.text = "Selected elements: 0 / 10";
    }

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

    private void UpdateStatusDisplay()
    {
        if (statusText != null)
        {
            int count = selectedSlots.Count;
            if (count < 10)
            {
                statusText.text = $"Select nuclear fuels & elements: <color=yellow>{count} / 10</color>";
            }
            else if (count == 10)
            {
                statusText.text = "Selected: <color=green>10 / 10 (Ready to Kickoff!)</color>";
            }
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

            // Initialize and show the playtime Inventory Bar with selected elements
            InventoryBarManager invBar = Object.FindAnyObjectByType<InventoryBarManager>(FindObjectsInactive.Include);
            if (invBar != null)
            {
                invBar.InitializeInventory(SelectedElements);
            }
            else
            {
                Debug.LogWarning("[ElementSelectionManager] InventoryBarManager not found in scene!");
            }

            // 2. Unpause and start the game!
            Time.timeScale = 1f;
            Debug.Log("[ElementSelectionManager] Game unpaused! Let the engineering cleanup begin.");

            // 3. Deactivate the Element Selection Panel
            gameObject.SetActive(false);
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
