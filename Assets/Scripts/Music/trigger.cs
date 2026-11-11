using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DialogueTrigger : MonoBehaviour
{
    [Header("Dialogue")]
    [TextArea(3, 8)]
    public string message = "Hello, traveler...";

    [Header("References")]
    public DialogueUI dialogueUI;

    [Tooltip("Tag that activates this trigger")]
    public string playerTag = "Player";

    [Tooltip("If true, trigger only fires once ever. If false, fires once per entry.")]
    public bool triggerOnlyOnce = true;

    [Header("Options")]
    [Tooltip("Lock trigger while text is still typing (prevents rapid re-trigger)")]
    public bool lockWhilePlaying = true;

    private bool _hasTriggered = false;
    private bool _playerInside = false;

    void Reset()
    {
        // Auto-setup: make collider a trigger
        Collider col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }

    void Awake()
    {
        // Safety: ensure this is a trigger
        Collider col = GetComponent<Collider>();
        if (col != null && !col.isTrigger)
            col.isTrigger = true;

        if (dialogueUI == null)
            dialogueUI = FindObjectOfType<DialogueUI>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        if (_playerInside) return;
        _playerInside = true;

        // Already completed → don't trigger again
        if (_hasTriggered && triggerOnlyOnce) return;

        // Currently typing → ignore
        if (lockWhilePlaying && dialogueUI != null && dialogueUI.IsPlaying) return;

        _hasTriggered = true;
        dialogueUI?.Play(message);
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        _playerInside = false;
    }

    // Allows you to reset it from other scripts if needed
    public void ResetTrigger()
    {
        _hasTriggered = false;
        _playerInside = false;
    }
}