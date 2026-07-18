using UnityEngine;
using UnityEngine.InputSystem;

public class TerminalController : MonoBehaviour
{
    [Header("Interaction Settings")]
    public Transform player;
    public float interactionRange = 3f;

    [Header("UI Prompt")]
    public GameObject promptUI;

    private bool isInRange = false;
    private bool hasTriggered = false;
    private bool showFallbackGUI = false;

    private void Awake()
    {
        // Try to automatically find the player Mr.Blast if not assigned
        if (player == null)
        {
            GameObject playerObj = GameObject.Find("Mr.Blast");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }
    }

    private void Update()
    {
        if (Time.timeScale == 0f) return;

        if (hasTriggered)
        {
            if (promptUI != null && promptUI.activeSelf)
            {
                promptUI.SetActive(false);
            }
            showFallbackGUI = false;
            return;
        }

        if (player == null) return;

        // Check distance to the player
        float distance = Vector3.Distance(transform.position, player.position);
        isInRange = (distance <= interactionRange);

        // Control UI Prompt visibility
        if (promptUI != null)
        {
            if (promptUI.activeSelf != isInRange)
            {
                promptUI.SetActive(isInRange);
            }
        }
        else
        {
            // Use fallback OnGUI if no promptUI is set
            showFallbackGUI = isInRange;
        }

        // Check for detonation key (F)
        if (isInRange)
        {
            if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
            {
                TriggerDetonation();
            }
        }
    }

    private void TriggerDetonation()
    {
        if (GameManager.Instance != null)
        {
            hasTriggered = true;
            if (promptUI != null)
            {
                promptUI.SetActive(false);
            }
            showFallbackGUI = false;
            
            Debug.Log("[TerminalController] Detonation initiated! Querying GameManager for reactivity checks.");
            GameManager.Instance.TryDetonate();
        }
        else
        {
            // Fallback to direct detonation if GameManager is missing
            PlanetBlaster blaster = Object.FindAnyObjectByType<PlanetBlaster>();
            if (blaster != null)
            {
                hasTriggered = true;
                if (promptUI != null)
                {
                    promptUI.SetActive(false);
                }
                showFallbackGUI = false;
                
                Debug.LogWarning("[TerminalController] GameManager not found! Cascading to direct direct detonation.");
                blaster.Detonate();
            }
            else
            {
                Debug.LogError("[TerminalController] PlanetBlaster and GameManager not found! Cannot detonate.");
            }
        }
    }

    private void OnGUI()
    {
        if (showFallbackGUI && !hasTriggered)
        {
            // Draw a neat fallback instruction box on the screen
            GUIStyle style = new GUIStyle(GUI.skin.box);
            style.fontSize = 20;
            style.normal.textColor = Color.yellow;
            style.alignment = TextAnchor.MiddleCenter;

            float width = 350;
            float height = 50;
            float x = (Screen.width - width) / 2;
            float y = Screen.height - 150;

            GUI.Box(new Rect(x, y, width, height), "Press [F] to Detonate Planet_M", style);
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Draw interaction range in editor for easy debugging
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}
