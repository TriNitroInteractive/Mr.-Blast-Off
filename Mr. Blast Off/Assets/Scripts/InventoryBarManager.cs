using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class InventoryBarManager : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("The template Slot GameObject inside the Inventory Bar")]
    public GameObject slotTemplate;

    private List<GameObject> activeSlots = new List<GameObject>();

    private void Awake()
    {
        // 1. Initially hide the Inventory Bar during selection phase (playtime HUD only)
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Programmatically configures and displays the 10 selected elements inside the Inventory Bar.
    /// </summary>
    public void InitializeInventory(List<string> selectedElements)
    {
        // 2. Activate the Inventory Bar GameObject
        gameObject.SetActive(true);

        // 3. Clear any existing generated slots to prevent accumulation
        foreach (var slot in activeSlots)
        {
            if (slot != slotTemplate && slot != null)
            {
                Destroy(slot);
            }
        }
        activeSlots.Clear();

        if (slotTemplate == null)
        {
            // Auto-locate if not set
            Transform templateTransform = transform.Find("Slot");
            if (templateTransform != null)
            {
                slotTemplate = templateTransform.gameObject;
            }
            else
            {
                Debug.LogError("[InventoryBarManager] Slot Template not found! Cannot initialize Inventory Bar.");
                return;
            }
        }

        // 4. Set up Horizontal Layout Group programmatically to keep sizes of slots at exactly 50x50
        HorizontalLayoutGroup layoutGroup = GetComponent<HorizontalLayoutGroup>();
        if (layoutGroup == null)
        {
            layoutGroup = gameObject.AddComponent<HorizontalLayoutGroup>();
        }

        layoutGroup.childControlWidth = false;
        layoutGroup.childControlHeight = false;
        layoutGroup.childForceExpandWidth = false;
        layoutGroup.childForceExpandHeight = false;
        layoutGroup.childAlignment = TextAnchor.MiddleCenter;
        layoutGroup.spacing = 20f;
        layoutGroup.padding = new RectOffset(25, 25, 12, 12);

        // 5. Generate and populate slots
        int totalElements = selectedElements.Count;
        for (int i = 0; i < totalElements; i++)
        {
            GameObject slotInstance;
            if (i == 0)
            {
                slotInstance = slotTemplate;
            }
            else
            {
                slotInstance = Instantiate(slotTemplate, transform, false);
                slotInstance.name = $"Slot {i + 1}";
            }

            slotInstance.SetActive(true);
            activeSlots.Add(slotInstance);

            // Set sizeDelta to exactly 50x50 to preserve original Slot size strictly
            RectTransform rect = slotInstance.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.sizeDelta = new Vector2(50f, 50f);
            }

            string name = selectedElements[i];

            // Set Color shade from Selection registry
            Image bgImage = slotInstance.GetComponent<Image>();
            if (bgImage != null)
            {
                if (ElementSelectionManager.ElementColors.TryGetValue(name, out Color elementColor))
                {
                    bgImage.color = elementColor;
                }
                else
                {
                    // Fallback shade color if registry is missed
                    float hue = i / (float)totalElements;
                    bgImage.color = Color.HSVToRGB(hue, 0.65f, 0.85f);
                }
            }

            // Dynamically add a clean name label inside the slot
            // Clear any old text child first if duplicating
            for (int childIdx = slotInstance.transform.childCount - 1; childIdx >= 0; childIdx--)
            {
                Destroy(slotInstance.transform.GetChild(childIdx).gameObject);
            }

            GameObject textGo = new GameObject("Text (TMP)");
            textGo.transform.SetParent(slotInstance.transform, false);

            TextMeshProUGUI textMesh = textGo.AddComponent<TextMeshProUGUI>();
            textMesh.fontSize = 9f; // Fits beautifully inside 50x50 box
            textMesh.alignment = TextAlignmentOptions.Center;
            textMesh.color = Color.white;
            textMesh.text = name;
            textMesh.enableWordWrapping = true;
            textMesh.outlineWidth = 0.25f;
            textMesh.outlineColor = Color.black;

            RectTransform textRect = textMesh.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            textRect.offsetMin = new Vector2(2f, 2f);
            textRect.offsetMax = new Vector2(-2f, -2f);
        }

        Debug.Log($"[InventoryBarManager] Successfully initialized playtime inventory bar with {totalElements} slots.");
    }
}
