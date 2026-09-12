using System.Collections.Generic;
using UnityEngine;

public class RhythmLaneUI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private MusicClock musicClock;
    [SerializeField] private RhythmEvaluator evaluator;

    [Tooltip("Optional parent for spawned arrows. If null, arrows are spawned at the scene root.")]
    [SerializeField] private Transform laneParent;

    [Header("Layout (empty transforms)")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform hitLine;
    [SerializeField] private float travelTimeSeconds = 1.5f;

    [Header("Direction prefabs (GameObjects with RhythmArrow)")]
    [SerializeField] private GameObject upPrefab;
    [SerializeField] private GameObject downPrefab;
    [SerializeField] private GameObject leftPrefab;
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

        if (musicClock == null || spawnPoint == null || hitLine == null)
        {
            Debug.LogError($"{nameof(RhythmLaneUI)}: missing musicClock / spawnPoint / hitLine.", this);
            return;
        }

        Vector3 spawnPos = GetPos(spawnPoint);
        Vector3 hitPos = GetPos(hitLine);

        foreach (AnswerPrompt prompt in prompts)
        {
            GameObject prefab = PrefabFor(prompt.RequiredDirection);
            if (prefab == null)
            {
                Debug.LogError($"{nameof(RhythmLaneUI)}: no prefab for direction {prompt.RequiredDirection}.", this);
                continue;
            }

            GameObject instance = Instantiate(prefab, laneParent);
            if (!instance.TryGetComponent(out RhythmArrow arrow))
            {
                Debug.LogError($"{nameof(RhythmLaneUI)}: prefab '{prefab.name}' has no RhythmArrow component.", prefab);
                Destroy(instance);
                continue;
            }

            double spawnTime = prompt.TargetSongTime - travelTimeSeconds;
            arrow.Setup(prompt, musicClock, spawnTime, spawnPos, hitPos);

            activeArrows.Add(arrow);
        }
    }

    private Vector3 GetPos(Transform t)
    {
        if (t is RectTransform rt)
            return new Vector3(rt.anchoredPosition.x, rt.anchoredPosition.y, 0f);

        return t.position;
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