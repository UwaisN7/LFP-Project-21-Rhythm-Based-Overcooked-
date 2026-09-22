using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

    [Header("Feedback")]
    [Tooltip("Prefab with RhythmFeedbackText + TextMeshPro. Spawned at hitLine on resolve.")]
    [SerializeField] private TextMeshPro feedbackTextPrefab;
    [SerializeField] private string onBeatMessage = "PERFECT!";
    [SerializeField] private string wrongMessage = "WRONG!";
    [SerializeField] private string lateMessage = "MISS!";
    [SerializeField] private string earlyMessage = "TOO EARLY!";
    [SerializeField] private Color onBeatColor = new Color(0.3f, 1f, 0.4f);
    [SerializeField] private Color wrongColor = new Color(1f, 0.3f, 0.3f);
    [SerializeField] private Color lateColor = new Color(0.7f, 0.7f, 0.7f);
    [SerializeField] private Color earlyColor = new Color(1f, 0.8f, 0.3f);
    [Header("Feedback Animation")]
    [SerializeField] private float feedbackRiseSpeed = 1f;
    [SerializeField] private float feedbackLifetime = 0.9f;
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
        //Debug.Log($"[LaneUI] Prompt resolved: {prompt.CurrentResult}");
        RhythmArrow match = activeArrows.Find(a => a != null && a.Prompt == prompt);
        if (match != null)
        {
            match.Resolve(prompt.CurrentResult);
            activeArrows.Remove(match);
        }

        // 2) Spawn the floating feedback text.
        SpawnFeedback(prompt.CurrentResult);
    }

    private void SpawnFeedback(AnswerPrompt.Result result)
    {
        if (feedbackTextPrefab == null || hitLine == null)
        {
            Debug.LogWarning($"{nameof(RhythmLaneUI)}: feedback text or hitLine missing.", this);
            return;
        }

        string message;
        Color color;

        switch (result)
        {
            case AnswerPrompt.Result.OnBeat:
                message = onBeatMessage;
                color = onBeatColor;
                break;

            case AnswerPrompt.Result.Wrong:
                message = wrongMessage;
                color = wrongColor;
                break;

            case AnswerPrompt.Result.Late:
                message = lateMessage;
                color = lateColor;
                break;

            case AnswerPrompt.Result.Early:
                message = earlyMessage;
                color = earlyColor;
                break;

            default:
                return;
        }

        // Spawn the 3D TextMesh directly.
        TextMeshPro popup = Instantiate(feedbackTextPrefab);

        popup.text = message;
        popup.color = color;

        // Spawn it at the hit line.
        popup.transform.position = laneParent.position;

        StartCoroutine(RiseAndFade(popup));
    }
    private IEnumerator RiseAndFade(TextMeshPro popup)
    {
        float elapsed = 0f;
        Color startColor = popup.color;

        while (elapsed < feedbackLifetime)
        {
            elapsed += Time.deltaTime;

            popup.transform.position += Vector3.up * feedbackRiseSpeed * Time.deltaTime;

            float alpha = Mathf.Lerp(1f, 0f, elapsed / feedbackLifetime);

            popup.color = new Color(
                startColor.r,
                startColor.g,
                startColor.b,
                alpha
            );

            yield return null;
        }

        Destroy(popup.gameObject);
    }
}