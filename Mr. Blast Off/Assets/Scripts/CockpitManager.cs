using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CockpitManager : MonoBehaviour
{
    [Header("Panel References")]
    public GameObject elementSelectionPanel;

    [Header("Text References")]
    public TextMeshProUGUI solacsText;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI scoreText;

    [Header("Planets Section")]
    public Transform planetsContainer;
    public GameObject planetSlotTemplate;

    [System.Serializable]
    public struct PlanetSpriteConfig
    {
        public string planetName;
        public Sprite planetSprite;
    }

    [Header("Planet Sprite Settings")]
    public List<PlanetSpriteConfig> planetSprites = new List<PlanetSpriteConfig>();

    [Header("Designer UI Customization")]
    [Tooltip("Custom font asset for the Preparation scene text elements.")]
    public TMP_FontAsset customFont;

    [Tooltip("Force bold styling on text elements.")]
    public bool forceBold = true;

    [Tooltip("The base text color. Defaults to dark charcoal for maximum contrast.")]
    public Color textBaseColor = new Color(0.12f, 0.12f, 0.15f, 1f);

    [Tooltip("Scale size delta of the planet icons in the list.")]
    public Vector2 planetIconScale = new Vector2(36f, 36f);

    [Tooltip("Position offset of the planet icon from the left center of the slot.")]
    public Vector2 planetIconPosition = new Vector2(22f, 0f);

    [Tooltip("The padding/offset inside the slot when the planet icon is present.")]
    public float textLeftOffset = 44f;

    [Header("Reactor Section")]
    public Transform reactorContainer;
    public GameObject reactorSlotTemplate;

    [Header("Shuttle Preview Section")]
    public GameObject shuttlePreviewContainer;

    // Local lists for generated UI items to easily refresh states
    private List<GameObject> activePlanetSlots = new List<GameObject>();
    private List<GameObject> activeReactorSlots = new List<GameObject>();

    // 3D Preview objects
    private RenderTexture previewTexture;
    private Camera previewCam;
    private GameObject previewModel;
    private GameObject previewContainerGo;

    private struct PlanetData
    {
        public string name;
        public string displayName;
        public string description;
        public int threshold;
        public Color themeColor;
    }

    private struct UpgradeData
    {
        public string name;
        public string description;
        public int cost;
        public System.Func<bool> checkUnlocked;
        public System.Action buyAction;
    }

    private List<PlanetData> planetsList;
    private List<UpgradeData> upgradesList;

    private void Awake()
    {
        InitializeData();
    }

    private void Start()
    {
        // Automatically find target panel references in scene if not dragged
        if (elementSelectionPanel == null)
        {
            var canvas = transform.parent;
            if (canvas != null)
            {
                var selectionTrans = canvas.Find("Element Selection");
                if (selectionTrans != null)
                {
                    elementSelectionPanel = selectionTrans.gameObject;
                }
            }
        }

        // Link template references if not set
        if (planetsContainer == null) planetsContainer = transform.Find("Planets");
        if (planetSlotTemplate == null && planetsContainer != null)
        {
            var slotTrans = planetsContainer.Find("Slot");
            if (slotTrans != null) planetSlotTemplate = slotTrans.gameObject;
        }

        if (reactorContainer == null) reactorContainer = transform.Find("Reactor");
        if (reactorSlotTemplate == null && reactorContainer != null)
        {
            var slotTrans = reactorContainer.Find("Slot");
            if (slotTrans != null) reactorSlotTemplate = slotTrans.gameObject;
        }

        if (shuttlePreviewContainer == null) shuttlePreviewContainer = transform.Find("Shuttle Preview").gameObject;

        SetupShuttle3DPreview();
        GeneratePlanetSlots();
        GenerateReactorSlots();
        RefreshUI();
    }

    private void InitializeData()
    {
        planetsList = new List<PlanetData>
        {
            new PlanetData { name = "Planet M", displayName = "Planet M", description = "The Core Hazard", threshold = 80, themeColor = new Color(0.9f, 0.7f, 0.1f) },
            new PlanetData { name = "Planet A", displayName = "Planet A", description = "Frozen Moon", threshold = 60, themeColor = new Color(0.1f, 0.75f, 0.9f) },
            new PlanetData { name = "Planet B", displayName = "Planet B", description = "Iron Giant", threshold = 90, themeColor = new Color(0.95f, 0.2f, 0.15f) },
            new PlanetData { name = "Planet C", displayName = "Planet C", description = "Carbon Rogue", threshold = 75, themeColor = new Color(0.55f, 0.55f, 0.55f) },
            new PlanetData { name = "Planet D", displayName = "Planet D", description = "Shard Asteroid", threshold = 65, themeColor = new Color(0.95f, 0.45f, 0.1f) }
        };

        upgradesList = new List<UpgradeData>
        {
            new UpgradeData
            {
                name = "Catalyst Pre-heater",
                description = "Injects chemical catalysts, adding +10 default points to total yield.",
                cost = 50,
                checkUnlocked = () => GameManager.Instance != null && GameManager.Instance.upgradeCatalyst,
                buyAction = () => { if (GameManager.Instance != null) GameManager.Instance.upgradeCatalyst = true; }
            },
            new UpgradeData
            {
                name = "Super-critical Core",
                description = "Enhances fuel compression, lowering required threshold by -10 points.",
                cost = 75,
                checkUnlocked = () => GameManager.Instance != null && GameManager.Instance.upgradeSupercritical,
                buyAction = () => { if (GameManager.Instance != null) GameManager.Instance.upgradeSupercritical = true; }
            },
            new UpgradeData
            {
                name = "Heavy-Water Moderator",
                description = "Optimizes neutron flux, multiplying element reaction points by 1.1x.",
                cost = 100,
                checkUnlocked = () => GameManager.Instance != null && GameManager.Instance.upgradeHeavyWater,
                buyAction = () => { if (GameManager.Instance != null) GameManager.Instance.upgradeHeavyWater = true; }
            }
        };
    }

    private void GeneratePlanetSlots()
    {
        if (planetSlotTemplate == null || planetsContainer == null)
        {
            Debug.LogError("[CockpitManager] Planet templates missing! Cannot generate planet slots.");
            return;
        }

        // Configure Layout Group programmatically to layout planets beautifully vertically
        var layout = planetsContainer.GetComponent<VerticalLayoutGroup>();
        if (layout == null) layout = planetsContainer.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.spacing = 10f;
        layout.padding = new RectOffset(10, 10, 50, 10);
        layout.childControlHeight = false;
        layout.childControlWidth = false;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = false;

        // Hide original template slot
        planetSlotTemplate.SetActive(false);

        for (int i = 0; i < planetsList.Count; i++)
        {
            var data = planetsList[i];
            GameObject slotInstance = Instantiate(planetSlotTemplate, planetsContainer, false);
            slotInstance.name = $"PlanetSlot_{data.name}";
            slotInstance.SetActive(true);

            // Re-style slot size to look elegant
            var rect = slotInstance.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(110f, 50f);

            // Back Color with subtle tinted theme
            var bgImg = slotInstance.GetComponent<Image>();
            if (bgImg != null)
            {
                bgImg.color = new Color(data.themeColor.r, data.themeColor.g, data.themeColor.b, 0.2f); // 20% opacity background for premium glow
            }

            // Dynamic clean labels (clear children if any duplicate first)
            for (int c = slotInstance.transform.childCount - 1; c >= 0; c--)
            {
                Destroy(slotInstance.transform.GetChild(c).gameObject);
            }

            // Try to find the planet's sprite
            Sprite planetSprite = null;
            if (planetSprites != null)
            {
                foreach (var config in planetSprites)
                {
                    if (config.planetName == data.name)
                    {
                        planetSprite = config.planetSprite;
                        break;
                    }
                }
            }

            // If sprite is found, create the icon on the left
            if (planetSprite != null)
            {
                GameObject iconGo = new GameObject("PlanetIcon");
                iconGo.transform.SetParent(slotInstance.transform, false);
                
                var iconRect = iconGo.AddComponent<RectTransform>();
                iconRect.anchorMin = new Vector2(0f, 0.5f);
                iconRect.anchorMax = new Vector2(0f, 0.5f);
                iconRect.pivot = new Vector2(0.5f, 0.5f);
                iconRect.sizeDelta = planetIconScale;
                iconRect.anchoredPosition = planetIconPosition;

                var iconImg = iconGo.AddComponent<Image>();
                iconImg.sprite = planetSprite;
                iconImg.raycastTarget = false; // Optimize raycasts
            }

            // Add Text
            GameObject textGo = new GameObject("PlanetText");
            textGo.transform.SetParent(slotInstance.transform, false);
            var textRect = textGo.AddComponent<RectTransform>();
            
            var label = textGo.AddComponent<TextMeshProUGUI>();
            label.fontSize = 8.5f;
            label.text = $"<b>{data.displayName}</b>\nReq: {data.threshold} pts";
            ApplyCustomTextSettings(label);

            if (planetSprite != null)
            {
                // Align text elegantly to the right of the planet icon
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.offsetMin = new Vector2(textLeftOffset, 0f);
                textRect.offsetMax = new Vector2(-4f, 0f);
                label.alignment = TextAlignmentOptions.MidlineLeft;
            }
            else
            {
                // Fallback to centered if no sprite is assigned
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.sizeDelta = Vector2.zero;
                label.alignment = TextAlignmentOptions.Center;
            }

            // Click response via Button component
            var btn = slotInstance.GetComponent<Button>();
            if (btn == null) btn = slotInstance.AddComponent<Button>();
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => OnPlanetSelected(data.name));

            activePlanetSlots.Add(slotInstance);
        }
    }

    private void GenerateReactorSlots()
    {
        if (reactorSlotTemplate == null || reactorContainer == null)
        {
            Debug.LogError("[CockpitManager] Reactor templates missing! Cannot generate upgrade slots.");
            return;
        }

        // Configure Layout Group programmatically to layout upgrades horizontally
        var layout = reactorContainer.GetComponent<HorizontalLayoutGroup>();
        if (layout == null) layout = reactorContainer.gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.spacing = 20f;
        layout.padding = new RectOffset(15, 15, 10, 10);
        layout.childControlHeight = false;
        layout.childControlWidth = false;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = false;

        // Hide original template slot
        reactorSlotTemplate.SetActive(false);

        for (int i = 0; i < upgradesList.Count; i++)
        {
            var data = upgradesList[i];
            GameObject slotInstance = Instantiate(reactorSlotTemplate, reactorContainer, false);
            slotInstance.name = $"ReactorSlot_{data.name}";
            slotInstance.SetActive(true);

            // Re-style slot size to look elegant
            var rect = slotInstance.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(150f, 95f);

            // Click response via Button component
            var btn = slotInstance.GetComponent<Button>();
            if (btn == null) btn = slotInstance.AddComponent<Button>();
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => OnUpgradeClicked(data));

            activeReactorSlots.Add(slotInstance);
        }

        RefreshReactorSlotsUI();
    }

    private void RefreshReactorSlotsUI()
    {
        for (int i = 0; i < upgradesList.Count; i++)
        {
            var data = upgradesList[i];
            var slotInstance = activeReactorSlots[i];

            // Setup colors based on unlock state
            bool isUnlocked = data.checkUnlocked();
            var bgImg = slotInstance.GetComponent<Image>();
            if (bgImg != null)
            {
                bgImg.color = isUnlocked ? new Color(0.1f, 0.6f, 0.2f, 0.8f) : new Color(0.15f, 0.15f, 0.18f, 0.85f);
            }

            // Clear children and build description texts nicely
            for (int c = slotInstance.transform.childCount - 1; c >= 0; c--)
            {
                Destroy(slotInstance.transform.GetChild(c).gameObject);
            }

            GameObject textGo = new GameObject("UpgradeText");
            textGo.transform.SetParent(slotInstance.transform, false);
            var textRect = textGo.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            textRect.offsetMin = new Vector2(6f, 6f);
            textRect.offsetMax = new Vector2(-6f, -6f);

            var label = textGo.AddComponent<TextMeshProUGUI>();
            label.fontSize = 8f;
            label.alignment = TextAlignmentOptions.Center;
            string costString = isUnlocked ? "<color=green>ACTIVE</color>" : $"<color=yellow>{data.cost} Solacs</color>";
            label.text = $"<b>{data.name}</b>\n{data.description}\n{costString}";
            ApplyCustomTextSettings(label);
        }
    }

    private void SetupShuttle3DPreview()
    {
        if (shuttlePreviewContainer == null) return;

        // Remove existing Image component if it exists, since a GameObject can only contain one UI Graphic component
        var existingImg = shuttlePreviewContainer.GetComponent<Image>();
        if (existingImg != null)
        {
            DestroyImmediate(existingImg);
        }

        // Add RawImage component
        var rawImg = shuttlePreviewContainer.GetComponent<RawImage>();
        if (rawImg == null) rawImg = shuttlePreviewContainer.AddComponent<RawImage>();

        // Create RenderTexture
        previewTexture = new RenderTexture(320, 200, 16, RenderTextureFormat.ARGB32);
        previewTexture.antiAliasing = 4;
        previewTexture.Create();
        rawImg.texture = previewTexture;
        rawImg.color = Color.white;

        // Programmatically setup preview camera rig (placed far away at safe 1000f, 1000f, 1000f coordinates)
        previewContainerGo = new GameObject("3DPreviewCamRig");
        previewContainerGo.transform.position = new Vector3(1000f, 1000f, 1000f);

        GameObject camGo = new GameObject("PreviewCamera");
        camGo.transform.SetParent(previewContainerGo.transform, false);
        camGo.transform.localPosition = new Vector3(0f, 0.5f, -4.5f);
        camGo.transform.localRotation = Quaternion.Euler(6f, 0f, 0f);

        previewCam = camGo.AddComponent<Camera>();
        previewCam.clearFlags = CameraClearFlags.SolidColor;
        previewCam.backgroundColor = new Color(0.04f, 0.04f, 0.06f, 1f);
        previewCam.fieldOfView = 35f;
        previewCam.nearClipPlane = 0.3f;
        previewCam.farClipPlane = 20f;
        previewCam.targetTexture = previewTexture;

        // Spawn beautiful rotating preview model matching Shuttle mesh (sphere)
        previewModel = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        previewModel.name = "PreviewShuttleModel";
        previewModel.transform.SetParent(previewContainerGo.transform, false);
        previewModel.transform.localPosition = Vector3.zero;
        previewModel.transform.localScale = new Vector3(1.35f, 1.35f, 1.35f);

        var collider = previewModel.GetComponent<Collider>();
        if (collider != null) Destroy(collider);

        // Apply our gorgeous black glass material
        var mr = previewModel.GetComponent<MeshRenderer>();
        if (mr != null)
        {
            Shader litShader = Shader.Find("Universal Render Pipeline/Lit");
            if (litShader == null) litShader = Shader.Find("Standard");
            if (litShader == null) litShader = Shader.Find("Sprites/Default");
            if (litShader == null) litShader = Shader.Find("Hidden/InternalErrorShader");

            if (litShader != null)
            {
                Material mat = new Material(litShader);
                mat.SetFloat("_Surface", 1);
                mat.SetFloat("_Blend", 0);
                mat.SetColor("_BaseColor", new Color(0.12f, 0.12f, 0.14f, 0.45f));
                mat.SetFloat("_Metallic", 0.95f);
                mat.SetFloat("_Smoothness", 0.92f);
                mat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetFloat("_ZWrite", 0);
                mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                mat.EnableKeyword("_ALPHAPREMULTIPLY_ON_OFF");
                mat.renderQueue = 3000;
                mr.sharedMaterial = mat;
            }
        }

        // Add 2 decorative green glowing rings wrapping the preview shuttle
        for (int i = 0; i < 2; i++)
        {
            GameObject ringGo = new GameObject($"PreviewRing_{i}");
            ringGo.transform.SetParent(previewModel.transform, false);
            ringGo.transform.localPosition = Vector3.zero;

            var lr = ringGo.AddComponent<LineRenderer>();
            lr.useWorldSpace = false;
            lr.loop = true;
            lr.startWidth = 0.04f;
            lr.endWidth = 0.04f;
            lr.positionCount = 31;

            Shader ringShader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (ringShader == null) ringShader = Shader.Find("Particles/Standard Unlit");
            if (ringShader == null) ringShader = Shader.Find("Sprites/Default");
            if (ringShader == null) ringShader = Shader.Find("Hidden/InternalErrorShader");

            if (ringShader != null)
            {
                Material ringMat = new Material(ringShader);
                ringMat.SetFloat("_Surface", 1);
                ringMat.SetFloat("_Blend", 0);
                ringMat.SetColor("_BaseColor", new Color(0f, 1f, 0.2f, 1f));
                lr.sharedMaterial = ringMat;
            }

            // Simple static offset circles
            float offset = (i == 0) ? 0.35f : -0.35f;
            float r = Mathf.Sqrt(1f - offset * offset) * 0.7f;
            for (int s = 0; s <= 30; s++)
            {
                float angle = (s / 30f) * Mathf.PI * 2f;
                lr.SetPosition(s, new Vector3(Mathf.Cos(angle) * r, offset, Mathf.Sin(angle) * r));
            }
        }
    }

    private void Update()
    {
        // Rotate 3D Shuttle preview model programmatically
        if (previewModel != null)
        {
            previewModel.transform.Rotate(Vector3.up, 35f * Time.deltaTime, Space.World);
            previewModel.transform.Rotate(Vector3.right, 10f * Time.deltaTime, Space.Self);
        }
    }

    private void OnPlanetSelected(string planetName)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.selectedPlanetName = planetName;
            Debug.Log($"[CockpitManager] Target destination locked: '{planetName}'");
            GameManager.Instance.UpdateInstructions("<b>STABILIZE CORE:</b> Select exactly <color=#FFFF00>10 elements</color> to balance reactor yield, then click <color=#00FF66>Start</color>!");
        }

        // Deactivate Cockpit panel and activate Element Selection panel
        gameObject.SetActive(false);
        if (elementSelectionPanel != null)
        {
            elementSelectionPanel.SetActive(true);
        }
    }

    private void OnUpgradeClicked(UpgradeData data)
    {
        if (GameManager.Instance == null) return;

        if (data.checkUnlocked())
        {
            return; // Already purchased
        }

        if (GameManager.Instance.solacs >= data.cost)
        {
            GameManager.Instance.solacs -= data.cost;
            data.buyAction();
            Debug.Log($"[CockpitManager] Successfully purchased upgrade: '{data.name}'!");

            RefreshUI();
            RefreshReactorSlotsUI();
        }
        else
        {
            Debug.LogWarning("[CockpitManager] Insufficient Solacs!");
            StartCoroutine(FlashTextRed(solacsText));
        }
    }

    private void RefreshUI()
    {
        if (GameManager.Instance == null) return;

        if (solacsText != null)
        {
            solacsText.text = $"Solacs Collected: <color=yellow>{GameManager.Instance.solacs}</color>";
            ApplyCustomTextSettings(solacsText);
        }
        if (nameText != null)
        {
            nameText.text = $"Engineer Profile: <color=green>{GameManager.Instance.playerName}</color>";
            ApplyCustomTextSettings(nameText);
        }
        if (scoreText != null)
        {
            scoreText.text = $"Max Cleanup Score: <color=cyan>{GameManager.Instance.highScore}</color> pts";
            ApplyCustomTextSettings(scoreText);
        }

        if (levelText != null)
        {
            // Calculate Shuttle Level (MK I to MK IV) based on number of active upgrades
            int activeCount = 0;
            if (GameManager.Instance.upgradeCatalyst) activeCount++;
            if (GameManager.Instance.upgradeSupercritical) activeCount++;
            if (GameManager.Instance.upgradeHeavyWater) activeCount++;

            string rankStr = "MK I (Standard Class)";
            if (activeCount == 1) rankStr = "MK II (Enhanced Core)";
            if (activeCount == 2) rankStr = "MK III (Advanced Cleanup Rig)";
            if (activeCount == 3) rankStr = "MK IV (Master Reactor Cruiser)";

            levelText.text = $"Shuttle Frame: <color=yellow>{rankStr}</color>";
            ApplyCustomTextSettings(levelText);
        }
    }

    /// <summary>
    /// Programmatically style TextMeshPro text components based on exposed designer options.
    /// </summary>
    public void ApplyCustomTextSettings(TextMeshProUGUI textComp)
    {
        if (textComp == null) return;

        if (customFont != null)
        {
            textComp.font = customFont;
        }

        if (forceBold)
        {
            textComp.fontStyle = FontStyles.Bold;
        }

        textComp.color = textBaseColor;
    }

    private IEnumerator FlashTextRed(TextMeshProUGUI text)
    {
        if (text == null) yield break;
        Color originalColor = text.color;
        text.color = Color.red;
        yield return new WaitForSeconds(0.8f);
        text.color = originalColor;
    }

    private void OnDestroy()
    {
        // Cleanup 3D Preview objects
        if (previewTexture != null)
        {
            previewTexture.Release();
            Destroy(previewTexture);
        }

        if (previewContainerGo != null)
        {
            Destroy(previewContainerGo);
        }
    }
}
