using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class ElementSlot : MonoBehaviour, IPointerClickHandler
{
    public string elementName;
    public Color elementColor;
    public bool isSelected = false;

    private Outline outline;
    private Image bgImage;
    private TextMeshProUGUI textMesh;
    private RectTransform rectTransform;

    private System.Action<ElementSlot> onClickCallback;

    public void Init(string name, Color color, System.Action<ElementSlot> callback)
    {
        elementName = name;
        elementColor = color;
        onClickCallback = callback;

        rectTransform = GetComponent<RectTransform>();
        bgImage = GetComponent<Image>();
        if (bgImage != null)
        {
            bgImage.color = color;
        }

        // Add or configure Outline component for selection outline
        outline = GetComponent<Outline>();
        if (outline == null)
        {
            outline = gameObject.AddComponent<Outline>();
        }
        outline.effectColor = Color.red;
        outline.effectDistance = new Vector2(4f, -4f);
        outline.enabled = false; // Disabled by default

        // 1. Dynamic Sprite Icon Loading
        Sprite elementSprite = Resources.Load<Sprite>($"elements/{name}");
        Transform iconTrans = transform.Find("Icon");
        Image iconImg = null;

        if (iconTrans == null)
        {
            GameObject iconGo = new GameObject("Icon");
            iconGo.transform.SetParent(transform, false);
            iconImg = iconGo.AddComponent<Image>();
        }
        else
        {
            iconImg = iconTrans.GetComponent<Image>();
        }

        if (iconImg != null)
        {
            iconImg.sprite = elementSprite;
            iconImg.color = Color.white;
            iconImg.preserveAspect = true;

            RectTransform iconRect = iconImg.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.1f, 0.35f); // Top 65%, with small padding
            iconRect.anchorMax = new Vector2(0.9f, 0.95f);
            iconRect.offsetMin = Vector2.zero;
            iconRect.offsetMax = Vector2.zero;
        }

        // 2. Text Component Layout Adjustment
        textMesh = GetComponentInChildren<TextMeshProUGUI>();
        if (textMesh == null)
        {
            GameObject textGo = new GameObject("Text (TMP)");
            textGo.transform.SetParent(transform, false);
            textMesh = textGo.AddComponent<TextMeshProUGUI>();
        }

        textMesh.text = name;
        textMesh.fontSize = 8f; // Thinner readable font size
        textMesh.alignment = TextAlignmentOptions.Center;
        textMesh.color = Color.white;
        textMesh.enableWordWrapping = true;
        textMesh.outlineWidth = 0.2f;
        textMesh.outlineColor = Color.black;

        RectTransform textRect = textMesh.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0f, 0f); // Bottom 35%
        textRect.anchorMax = new Vector2(1f, 0.35f);
        textRect.offsetMin = new Vector2(2f, 2f);
        textRect.offsetMax = new Vector2(-2f, -2f);

        // Reset scale
        transform.localScale = Vector3.one;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        onClickCallback?.Invoke(this);
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;
        if (outline != null)
        {
            outline.enabled = selected;
        }

        // Scale down a bit when selected (0.85 scale as per requirements)
        transform.localScale = selected ? new Vector3(0.85f, 0.85f, 0.85f) : Vector3.one;
    }
}
