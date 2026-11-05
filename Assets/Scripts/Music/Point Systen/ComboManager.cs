using TMPro;
using UnityEngine;
using System.Collections.Generic;

public class ComboManager : MonoBehaviour
{
    [SerializeField] private PointManager pointManager;
    [SerializeField] private int playerId = 0;

    [Header("UI Refs")]
    [SerializeField] private TMP_Text styleRankLabel; // DMC Rank (D, C, B, A, S)
    [SerializeField] private TMP_Text actionFeedLabel; // Ultrakill Feed

    [Header("Ultrakill Feed Settings")]
    [SerializeField] private int maxFeedLines = 5;
    [SerializeField] private float feedClearTime = 3f;

    private List<string> activeFeed = new List<string>();
    private float lastFeedTime;

    private void OnEnable()
    {
        if (pointManager != null)
        {
            pointManager.OnScoreChanged += HandleScoreChanged;
            pointManager.OnActionLogged += HandleActionLogged;
        }
    }

    private void OnDisable()
    {
        if (pointManager != null)
        {
            pointManager.OnScoreChanged -= HandleScoreChanged;
            pointManager.OnActionLogged -= HandleActionLogged;
        }
    }

    private void Update()
    {
        // Clear the feed if it's been idle too long
        if (activeFeed.Count > 0 && Time.time - lastFeedTime > feedClearTime)
        {
            activeFeed.Clear();
            UpdateFeedUI();
        }
    }

    private void HandleScoreChanged(int changedPlayerId, PointManager.PlayerScore score, int multiplier)
    {
        if (changedPlayerId != playerId) return;

        // DMC Style Rank Switcher
        string rank = "";
        switch (multiplier)
        {
            case 1: rank = "<color=#888888>D - Disgusting</color>"; break;
            case 2: rank = "<color=#55FF55>C - Cooking</color>"; break;
            case 3: rank = "<color=#5555FF>B - Brilliant</color>"; break;
            case 4: rank = "<color=#FFFF55>A - Amazing</color>"; break;
            case 5: rank = "<color=#FF8800>S - Spicy</color>"; break;
            case 6: rank = "<color=#FF0000>SS - Super Spicy</color>"; break;
            default:
                if (multiplier >= 7) rank = "<color=#FF00FF>SSS - SMOKIN' SEXY SOUS-CHEF!</color>";
                break;
        }

        styleRankLabel.text = rank + $"\n(x{multiplier})";
    }

    private void HandleActionLogged(int changedPlayerId, string actionText)
    {
        if (changedPlayerId != playerId) return;

        activeFeed.Add(actionText);
        if (activeFeed.Count > maxFeedLines)
        {
            activeFeed.RemoveAt(0); // Pop oldest action
        }

        lastFeedTime = Time.time;
        UpdateFeedUI();
    }

    private void UpdateFeedUI()
    {
        if (actionFeedLabel != null)
        {
            actionFeedLabel.text = string.Join("\n", activeFeed);
        }
    }
}