using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Judges a queue of AnswerPrompts against player input.
/// One Evaluator = one player's active rhythm session.
///
/// Scoring rules (as spec'd):
///  - Press before the hit window opens -> ignored, no penalty, prompt stays active.
///  - Press with the CORRECT direction inside the hit window -> On Beat, +25 points.
///  - Press with the WRONG direction inside the hit window -> Wrong, -50 points + strike.
///  - No valid press before the window closes -> Late, strike (no points lost).
///  - 2 strikes -> ingredient burns (OnIngredientBurned fires).
/// </summary>
public class RhythmEvaluator : MonoBehaviour
{
    [Header("Timing")]
    [Tooltip("How close to the beat (seconds, either side) counts as 'on beat'.")]
    [SerializeField] private double hitWindowSeconds = 0.12;

    [Header("Refs")]
    [SerializeField] private MusicClock musicClock;
    [SerializeField] private RhythmInputManager inputManager;
    [SerializeField] private PointManager pointManager;
    [SerializeField] private int playerId = 0;

    public event System.Action<AnswerPrompt> OnPromptResolved;
    public event System.Action OnIngredientBurned;

    private Queue<AnswerPrompt> activePrompts = new Queue<AnswerPrompt>();
    private int strikes = 0;

    private void OnEnable()
    {
        if (inputManager != null)
            inputManager.OnDirectionPressed += HandleInput;
    }

    private void OnDisable()
    {
        if (inputManager != null)
            inputManager.OnDirectionPressed -= HandleInput;
    }

    public void BeginSequence(Queue<AnswerPrompt> prompts)
    {
        activePrompts = prompts;
        strikes = 0;
    }

    private void Update()
    {
        if (activePrompts.Count == 0 || musicClock == null) return;

        AnswerPrompt current = activePrompts.Peek();

        // Window closed with nothing hit -> Late strike.
        if (musicClock.SongTime > current.TargetSongTime + hitWindowSeconds)
        {
            Resolve(current, AnswerPrompt.Result.Late);
        }
    }

    private void HandleInput(RhythmDirection direction, double songTimeOfPress)
    {
        if (activePrompts.Count == 0) return;

        AnswerPrompt current = activePrompts.Peek();
        double diff = songTimeOfPress - current.TargetSongTime;

        // Too early to judge yet -- ignore entirely, no penalty, prompt stays active.
        if (diff < -hitWindowSeconds)
            return;

        if (direction != current.RequiredDirection)
        {
            Resolve(current, AnswerPrompt.Result.Wrong);
            return;
        }

        Resolve(current, AnswerPrompt.Result.OnBeat);
    }

    private void Resolve(AnswerPrompt prompt, AnswerPrompt.Result result)
    {
        prompt.CurrentResult = result;
        activePrompts.Dequeue();

        switch (result)
        {
            case AnswerPrompt.Result.OnBeat:
                pointManager?.AddActionPoints(playerId, 25);
                Debug.Log("Added Points");
                break;

            case AnswerPrompt.Result.Wrong:
                pointManager?.AddActionPoints(playerId, -50);
                Debug.Log("Losing Points");
                RegisterStrike();
                break;

            case AnswerPrompt.Result.Late:
                Debug.Log("Losing Points");
                RegisterStrike();
                break;
        }

        OnPromptResolved?.Invoke(prompt);
    }

    private void RegisterStrike()
    {
        strikes++;
        pointManager?.ReportStrike(playerId);

        if (strikes >= 2)
        {
            OnIngredientBurned?.Invoke();
        }
    }
}
