using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Visualises the current prompt queue: arrows travel from the spawn
/// point to the hit line, timed to arrive exactly on their target beat.
/// It reads the same AnswerPrompt data the Evaluator judges, and only
/// ever reflects the Evaluator's decision - it never makes hit/miss
/// calls of its own.
/// </summary>
public class RhythmLaneUI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private MusicClock musicClock;
    [SerializeField] private RhythmEvaluator evaluator;
    [SerializeField] private RhythmArrow arrowPrefab;
    [SerializeField] private RectTransform laneParent;

    [Header("Layout")]
    [Tooltip("X position (local to laneParent) where arrows spawn.")]
    [SerializeField] private float spawnX = -500f;
    [Tooltip("X position where arrows should be hit. Swap spawnX/hitX if you want them travelling right-to-left instead.")]
    [SerializeField] private float hitX = 500f;
    [Tooltip("Seconds an arrow spends travelling before it reaches the hit line.")]
    [SerializeField] private float travelTimeSeconds = 1.5f;

    [Header("Direction prefabs (GameObjects)")]
    [Tooltip("Prefab spawned for an Up prompt. Should contain a RhythmArrow component.")]
    [SerializeField] private GameObject upPrefab;
    [Tooltip("Prefab spawned for a Down prompt. Should contain a RhythmArrow component.")]
    [SerializeField] private GameObject downPrefab;
    [Tooltip("Prefab spawned for a Left prompt. Should contain a RhythmArrow component.")]
    [SerializeField] private GameObject leftPrefab;
    [Tooltip("Prefab spawned for a Right prompt. Should contain a RhythmArrow component.")]
    [SerializeField] private GameObject rightPrefab;

    private readonly List<RhythmArrow> activeArrows = new List<RhythmArrow>();

    private void OnEnable()
    {
        if (evaluator != null)
            evaluator.OnPromptResolved += HandlePromptResolved;
    }

    private void OnDisable()
    {
        if (evaluator != null)
            evaluator.OnPromptResolved -= HandlePromptResolved;
    }

    public void Display(IEnumerable<AnswerPrompt> prompts)
    {
        if (prompts == null) return;

        if (musicClock == null || laneParent == null)
        {
            Debug.LogError($"{nameof(RhythmLaneUI)}: musicClock or laneParent is not assigned in the Inspector.", this);
            return;
        }

        foreach (AnswerPrompt prompt in prompts)
        {
            GameObject prefab = PrefabFor(prompt.RequiredDirection);
            if (prefab == null)
            {
                Debug.LogError($"{nameof(RhythmLaneUI)}: no prefab assigned for direction {prompt.RequiredDirection}.", this);
                continue;
            }

            GameObject instance = Instantiate(prefab, laneParent);
            if (!instance.TryGetComponent(out RhythmArrow arrow))
            {
                Debug.LogError($"{nameof(RhythmLaneUI)}: prefab '{prefab.name}' has no RhythmArrow component.", prefab);
                Destroy(instance);
                continue;
            }

            float spawnTime = (float)(prompt.TargetSongTime - travelTimeSeconds);
            arrow.Setup(prompt, musicClock, spawnTime, spawnX, hitX);

            activeArrows.Add(arrow);
        }
    }

    private GameObject PrefabFor(RhythmDirection direction)
    {
        switch (direction)
        {
            case RhythmDirection.Up: return upPrefab;
            case RhythmDirection.Down: return downPrefab;
            case RhythmDirection.Left: return leftPrefab;
            case RhythmDirection.Right: return rightPrefab;
            default: return null;
        }
    }

    private void HandlePromptResolved(AnswerPrompt prompt)
    {
        RhythmArrow match = activeArrows.Find(a => a != null && a.Prompt == prompt);
        if (match == null) return;

        match.ShowResult(prompt.CurrentResult);
        activeArrows.Remove(match);
    }
}