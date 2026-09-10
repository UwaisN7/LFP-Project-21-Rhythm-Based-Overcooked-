using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A single on-screen arrow that travels across the lane and arrives
/// at the hit line exactly on its target beat. Purely visual - the
/// RhythmEvaluator is still the source of truth for hit/miss, this
/// just reflects whatever it decides.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class RhythmArrow : MonoBehaviour
{
    [Header("Result colors")]
    [SerializeField] private Material visual;         
    [SerializeField] private Color onBeatColor = Color.green;
    [SerializeField] private Color wrongColor = Color.red;
    [SerializeField] private Color lateColor = Color.gray;

    private RectTransform rect;
    private MusicClock musicClock;
    private double spawnSongTime;
    private double targetSongTime;
    private float startX;
    private float endX;
    private bool resolved;

    public AnswerPrompt Prompt { get; private set; }

    /// <summary>
    /// Called by RhythmLaneUI after instantiating the direction prefab.
    /// </summary>
    public void Setup(AnswerPrompt prompt, MusicClock clock, float spawnTime, float spawnX, float hitX)
    {
        Prompt = prompt;
        musicClock = clock;
        spawnSongTime = spawnTime;
        targetSongTime = prompt.TargetSongTime;
        startX = spawnX;
        endX = hitX;

        rect = GetComponent<RectTransform>();
        if (rect != null)
            rect.anchoredPosition = new Vector2(startX, rect.anchoredPosition.y);
    }

    private void Update()
    {
        if (musicClock == null || rect == null) return;

        double duration = targetSongTime - spawnSongTime;
        double t = duration > 0 ? (musicClock.SongTime - spawnSongTime) / duration : 1;
        float clamped = Mathf.Clamp01((float)t);

        float x = Mathf.Lerp(startX, endX, clamped);
        rect.anchoredPosition = new Vector2(x, rect.anchoredPosition.y);
    }

    public void ShowResult(AnswerPrompt.Result result)
    {
        if (resolved) return;
        resolved = true;

        if (visual != null)
        {
            switch (result)
            {
                case AnswerPrompt.Result.Early: Destroy(gameObject); break;
                case AnswerPrompt.Result.OnBeat: visual.color = onBeatColor; break;
                case AnswerPrompt.Result.Wrong: visual.color = wrongColor; break;
                case AnswerPrompt.Result.Late: visual.color = lateColor; break;
            }
        }

        Destroy(gameObject, 0.15f);
    }
}
