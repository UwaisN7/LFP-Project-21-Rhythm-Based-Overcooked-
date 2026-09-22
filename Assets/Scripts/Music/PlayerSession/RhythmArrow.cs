using UnityEngine;

/// <summary>
/// A single arrow that travels from a spawn point to a hit line and
/// arrives exactly on its target beat. Purely visual - the
/// RhythmEvaluator is the source of truth for hit/miss.
///
/// This component does NOT handle feedback. It just moves and dies.
/// Feedback (text, particles, sound) is spawned by RhythmLaneUI.
/// </summary>
public class RhythmArrow : MonoBehaviour
{
    [Header("Cleanup")]
    [SerializeField] private float destroyDelay = 0.15f;

    private Transform tr;
    private RectTransform rect;

    private MusicClock musicClock;
    private double spawnSongTime;
    private double targetSongTime;
    private Vector3 startPos;
    private Vector3 endPos;
    private bool resolved;

    public AnswerPrompt Prompt { get; private set; }

    public void Setup(AnswerPrompt prompt, MusicClock clock, double spawnTime, Vector3 spawnPos, Vector3 hitPos)
    {
        Prompt = prompt;
        musicClock = clock;
        spawnSongTime = spawnTime;
        targetSongTime = prompt.TargetSongTime;
        startPos = spawnPos;
        endPos = hitPos;

        tr = transform;
        rect = tr as RectTransform;

        ApplyPosition(0f);
    }

    private void Update()
    {
        if (musicClock == null) return;

        double duration = targetSongTime - spawnSongTime;
        double t = duration > 0.0 ? (musicClock.SongTime - spawnSongTime) / duration : 1.0;
        ApplyPosition(Mathf.Clamp01((float)t));
    }

    private void ApplyPosition(float t)
    {
        Vector3 pos = Vector3.Lerp(startPos, endPos, t);

        if (rect != null)
        {
            rect.anchoredPosition = new Vector2(pos.x, rect.anchoredPosition.y);
        }
        else if (tr != null)
        {
            tr.position = pos;
        }
    }

    /// <summary>
    /// Called when the prompt is resolved. Destroys the arrow
    /// (after a short delay for OnBeat/Wrong/Late, immediately for Early).
    /// </summary>
    public void Resolve(AnswerPrompt.Result result)
    {
        if (resolved) return;
        resolved = true;

        if (result == AnswerPrompt.Result.Early)
        {
            Destroy(gameObject);
            return;
        }

        Destroy(gameObject, destroyDelay);
    }
}