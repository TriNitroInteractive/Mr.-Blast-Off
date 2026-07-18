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

        // Setup text
        textMesh = GetComponentInChildren<TextMeshProUGUI>();
        if (textMesh == null)
        {
            GameObject textGo = new GameObject("Text (TMP)");
            textGo.transform.SetParent(transform, false);
            textMesh = textGo.AddComponent<TextMeshProUGUI>();
            textMesh.fontSize = 12f;
            textMesh.alignment = TextAlignmentOptions.Center;
            textMesh.color = Color.white;
            
            // Set text style/wrapping to handle elements nicely
            textMesh.enableWordWrapping = true;
            
            // Text shadow/outline to ensure readability on any background color
            textMesh.outlineWidth = 0.25f;
            textMesh.outlineColor = Color.black;

            RectTransform textRect = textMesh.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            
            // Margins to prevent clipping at borders
            textRect.offsetMin = new Vector2(4f, 4f);
            textRect.offsetMax = new Vector2(-4f, -4f);
        }
        textMesh.text = name;

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
