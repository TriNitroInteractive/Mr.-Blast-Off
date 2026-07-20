using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Handles smooth, springy scaling on hover and click using IPointer*Handler interfaces in unscaled time.
/// Perfect for UI buttons to give them an organic, highly responsive feel.
/// </summary>
public class UIButtonJuice : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Juice Scale Settings")]
    [Tooltip("Target scale multiplier when the mouse hovers over the button.")]
    [SerializeField] private float hoverScale = 1.1f;

    [Tooltip("Target scale multiplier when the button is pressed down.")]
    [SerializeField] private float pressedScale = 0.9f;

    [Header("Spring Physics Settings")]
    [Tooltip("Stiffness of the spring. Higher values make the spring snap faster.")]
    [SerializeField] private float stiffness = 150f;

    [Tooltip("Damping of the spring. Lower values cause more bounciness/overshoot.")]
    [SerializeField] private float damping = 12f;

    private Vector3 _originalScale;
    private Vector3 _targetScale;
    private Vector3 _velocity;
    private bool _isHovered = false;
    private bool _isPressed = false;

    private void Awake()
    {
        _originalScale = transform.localScale;
        _targetScale = _originalScale;
    }

    private void OnDisable()
    {
        // Reset scale and states immediately on disable to avoid stuck visual states
        transform.localScale = _originalScale;
        _targetScale = _originalScale;
        _velocity = Vector3.zero;
        _isHovered = false;
        _isPressed = false;
    }

    private void Update()
    {
        // Spring physics simulation using unscaledDeltaTime so animations remain responsive even when the game is paused
        float dt = Time.unscaledDeltaTime;
        
        // Protect against extreme deltaTime spikes (e.g. during scene loads)
        if (dt > 0.1f) dt = 0.1f;

        Vector3 currentScale = transform.localScale;
        Vector3 displacement = _targetScale - currentScale;
        Vector3 springForce = displacement * stiffness;
        Vector3 dampingForce = -_velocity * damping;
        Vector3 acceleration = springForce + dampingForce;

        _velocity += acceleration * dt;
        transform.localScale += _velocity * dt;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _isHovered = true;
        UpdateTargetScale();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _isHovered = false;
        UpdateTargetScale();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _isPressed = true;
        UpdateTargetScale();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _isPressed = false;
        UpdateTargetScale();
    }

    private void UpdateTargetScale()
    {
        if (_isPressed)
        {
            _targetScale = _originalScale * pressedScale;
        }
        else if (_isHovered)
        {
            _targetScale = _originalScale * hoverScale;
        }
        else
        {
            _targetScale = _originalScale;
        }
    }
}
