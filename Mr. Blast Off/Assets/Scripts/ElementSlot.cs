using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class ElementSlot : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public string elementName;
    public Color elementColor;
    public bool isSelected = false;

    private Outline outline;
    private Image bgImage;
    private TextMeshProUGUI textMesh;
    private RectTransform rectTransform;

    private System.Action<ElementSlot> onClickCallback;
    private Coroutine _scaleCoroutine;
    private Vector3 _targetScale = Vector3.one;
    private bool _isHovered = false;

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

        // 1. Dynamic Sprite Icon Loading (cached dictionary fallback)
        Sprite elementSprite = null;
        if (ElementSelectionManager.ElementSprites != null && ElementSelectionManager.ElementSprites.TryGetValue(name, out Sprite cachedSprite))
        {
            elementSprite = cachedSprite;
        }
        else
        {
            elementSprite = Resources.Load<Sprite>($"elements/{name}");
        }

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
        _isHovered = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        onClickCallback?.Invoke(this);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _isHovered = true;

        // Find selection manager to update dynamic scientific description
        ElementSelectionManager manager = Object.FindAnyObjectByType<ElementSelectionManager>(FindObjectsInactive.Include);
        if (manager != null)
        {
            manager.SetHoveredElement(elementName);
        }

        // Set outline to neon cyan glow on hover
        if (outline != null)
        {
            outline.effectColor = new Color(0.0f, 0.85f, 1.0f, 1f); // Neon cyan
            outline.enabled = true;
        }

        // Animate hover scale smoothly
        StartScaleTransition(new Vector3(1.08f, 1.08f, 1.08f), false);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _isHovered = false;

        // Reset status text tooltip
        ElementSelectionManager manager = Object.FindAnyObjectByType<ElementSelectionManager>(FindObjectsInactive.Include);
        if (manager != null)
        {
            manager.SetHoveredElement(null);
        }

        // Restore outline color and state
        if (outline != null)
        {
            outline.effectColor = Color.red;
            outline.enabled = isSelected;
        }

        // Restore baseline scale
        Vector3 defaultTarget = isSelected ? new Vector3(0.85f, 0.85f, 0.85f) : Vector3.one;
        StartScaleTransition(defaultTarget, false);
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;
        if (outline != null)
        {
            outline.effectColor = Color.red;
            if (_isHovered)
            {
                outline.effectColor = new Color(0.0f, 0.85f, 1.0f, 1f);
            }
            outline.enabled = selected || _isHovered;
        }

        Vector3 target = selected ? new Vector3(0.85f, 0.85f, 0.85f) : Vector3.one;
        Vector3 finalTarget = target;

        if (_isHovered && !selected)
        {
            finalTarget = new Vector3(1.08f, 1.08f, 1.08f);
        }
        else if (_isHovered && selected)
        {
            finalTarget = new Vector3(0.85f, 0.85f, 0.85f);
        }

        StartScaleTransition(finalTarget, true); // true for click spring bounce!
    }

    private void StartScaleTransition(Vector3 target, bool isClick)
    {
        _targetScale = target;
        if (_scaleCoroutine != null) StopCoroutine(_scaleCoroutine);
        if (gameObject.activeInHierarchy)
        {
            if (isClick)
            {
                _scaleCoroutine = StartCoroutine(ClickBounceRoutine(target));
            }
            else
            {
                _scaleCoroutine = StartCoroutine(AnimateScaleRoutine(target));
            }
        }
        else
        {
            transform.localScale = target;
        }
    }

    private System.Collections.IEnumerator AnimateScaleRoutine(Vector3 target)
    {
        float duration = 0.12f;
        float elapsed = 0f;
        Vector3 startScale = transform.localScale;

        while (elapsed < duration)
         {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float smoothT = t * t * (3f - 2f * t);
            transform.localScale = Vector3.Lerp(startScale, target, smoothT);
            yield return null;
         }
        transform.localScale = target;
        _scaleCoroutine = null;
    }

    private System.Collections.IEnumerator ClickBounceRoutine(Vector3 target)
    {
        float duration = 0.22f;
        float elapsed = 0f;
        Vector3 startScale = transform.localScale;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            // Dampened sine wave spring bounce
            float scaleOffset = Mathf.Sin(t * Mathf.PI * 2.5f) * 0.08f * (1f - t);
            transform.localScale = Vector3.Lerp(startScale, target, t) + new Vector3(scaleOffset, scaleOffset, scaleOffset);
            yield return null;
        }
        transform.localScale = target;
        _scaleCoroutine = null;
    }

    private void OnDisable()
    {
        if (_scaleCoroutine != null)
        {
            StopCoroutine(_scaleCoroutine);
            _scaleCoroutine = null;
        }
        _isHovered = false;
    }
}
