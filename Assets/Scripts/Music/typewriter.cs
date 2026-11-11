using System.Collections;
using UnityEngine;
using TMPro;

public class DialogueUI : MonoBehaviour
{
    [Header("References")]
    public TextMeshProUGUI dialogueText;

    [Header("Typewriter Settings")]
    [Tooltip("Seconds per character")]
    public float charDelay = 0.04f;

    [Tooltip("Pause after punctuation (., !, ?)")]
    public float punctuationPause = 0.25f;

    [Header("Display Settings")]
    [Tooltip("How long the text stays after finishing before it can be replaced. Set 0 to keep forever.")]
    public float holdAfterFinish = 0f;

    public bool IsPlaying { get; private set; }

    private Coroutine _typingRoutine;

    void Awake()
    {
        if (dialogueText == null)
            dialogueText = GetComponent<TextMeshProUGUI>();

        if (dialogueText != null)
            dialogueText.text = "";
    }

    /// <summary>
    /// Plays a dialogue line with the typewriter effect.
    /// If another line is already playing, it will finish first (queue-safe).
    /// </summary>
    public void Play(string message)
    {
        if (_typingRoutine != null)
            StopCoroutine(_typingRoutine);

        _typingRoutine = StartCoroutine(TypeRoutine(message));
    }

    private IEnumerator TypeRoutine(string message)
    {
        IsPlaying = true;
        dialogueText.text = "";

        foreach (char c in message)
        {
            dialogueText.text += c;

            float wait = charDelay;
            if (c == '.' || c == '!' || c == '?')
                wait += punctuationPause;

            yield return new WaitForSeconds(wait);
        }

        if (holdAfterFinish > 0f)
            yield return new WaitForSeconds(holdAfterFinish);

        IsPlaying = false;
        _typingRoutine = null;
    }

    /// <summary>Instantly finish the current line (skip typewriter).</summary>
    public void SkipToEnd()
    {
        if (_typingRoutine != null)
        {
            StopCoroutine(_typingRoutine);
            _typingRoutine = null;
        }
        IsPlaying = false;
    }

    public void Clear()
    {
        SkipToEnd();
        dialogueText.text = "";
    }
}