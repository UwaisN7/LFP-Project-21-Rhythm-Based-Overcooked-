using System.Collections.Generic;
using UnityEngine;
public class PointManager : MonoBehaviour
{
    [System.Serializable]
    public class PlayerScore
    {
        public int ActionPoints;
        public int ComboMeter;

        public int ComboMultiplier = 1;
    }

    [Header("Debug")]
    [Tooltip("Log every score change with full AP breakdown for all players.")]
    [SerializeField] private bool debugLogging = true;

    [Tooltip("Action points required per combo multiplier tier. Default: 100.")]
    [SerializeField] private int pointsPerMultiplierTier = 100;

    [Tooltip("If true, AP is clamped at 0 and can never go negative. " +
             "If false, AP can go negative (multiplier already floors at 1x).")]
    [SerializeField] private bool clampActionPointsAtZero = true;

    [Header("Strike Penalties")]
    [Tooltip("Action points lost when a player does a bad action (a strike). Tune this in the Inspector.")]
    [SerializeField] private int strikeActionPointPenalty = 50;

    [Tooltip("Combo meter (hit streak) lost per strike, instead of wiping it to 0. Tune this in the Inspector.")]
    [SerializeField] private int strikeComboMeterPenalty = 3;

    private Dictionary<int, PlayerScore> scores = new Dictionary<int, PlayerScore>();

    public event System.Action<int, PlayerScore> OnScoreChanged;

    private PlayerScore GetOrCreate(int playerId)
    {
        if (!scores.TryGetValue(playerId, out var score))
        {
            score = new PlayerScore();
            scores[playerId] = score;
        }
        return score;
    }

    public void AddActionPoints(int playerId, int amount)
    {
        var score = GetOrCreate(playerId);
        int previousAP = score.ActionPoints;
        int previousMultiplier = score.ComboMultiplier;

        score.ActionPoints += amount;

        if (clampActionPointsAtZero && score.ActionPoints < 0)
            score.ActionPoints = 0;

        
        if (amount > 0)
            score.ComboMeter++;

        
        int newMultiplier = score.ActionPoints / pointsPerMultiplierTier;
        if (newMultiplier < 1) newMultiplier = 1; 
        score.ComboMultiplier = newMultiplier;

        if (debugLogging)
        {
            string tierChange = "";
            if (newMultiplier > previousMultiplier)
                tierChange = $" ^ UPGRADED {previousMultiplier}x -> {newMultiplier}x";
            else if (newMultiplier < previousMultiplier)
                tierChange = $" v DOWNGRADED {previousMultiplier}x -> {newMultiplier}x";

            Debug.Log(
                $"[PointManager] Player {playerId} | " +
                $"AP: {previousAP} -> {score.ActionPoints} ({amount:+#;-#;0}) | " +
                $"Multiplier: {newMultiplier}x{tierChange} | " +
                $"ComboMeter: {score.ComboMeter}\n" +
                FormatAllPlayerScores()
            );
        }

        OnScoreChanged?.Invoke(playerId, score);
    }

    public void ReportStrike(int playerId)
    {
        var score = GetOrCreate(playerId);
        int previousAP = score.ActionPoints;
        int previousMultiplier = score.ComboMultiplier;
        int previousComboMeter = score.ComboMeter;

      
        score.ActionPoints -= strikeActionPointPenalty;

        if (clampActionPointsAtZero && score.ActionPoints < 0)
            score.ActionPoints = 0;

        // Softer penalty: drop the streak by a fixed amount instead of wiping it out.
        score.ComboMeter -= strikeComboMeterPenalty;
        if (score.ComboMeter < 0)
            score.ComboMeter = 0;

        int newMultiplier = score.ActionPoints / pointsPerMultiplierTier;
        if (newMultiplier < 1) newMultiplier = 1; 

        score.ComboMultiplier = newMultiplier;

        if (debugLogging)
        {
            string tierChange = "";
            if (newMultiplier > previousMultiplier)
                tierChange = $" ^ UPGRADED {previousMultiplier}x -> {newMultiplier}x";
            else if (newMultiplier < previousMultiplier)
                tierChange = $" v DOWNGRADED {previousMultiplier}x -> {newMultiplier}x";

            Debug.Log(
                $"[PointManager] Player {playerId} STRUCK | " +
                $"AP: {previousAP} -> {score.ActionPoints} (-{strikeActionPointPenalty}) | " +
                $"ComboMeter: {previousComboMeter} -> {score.ComboMeter} (-{strikeComboMeterPenalty}) | " +
                $"Multiplier: {newMultiplier}x{tierChange}\n" +
                FormatAllPlayerScores()
            );
        }

        OnScoreChanged?.Invoke(playerId, score);
    }

    public PlayerScore GetScore(int playerId) => GetOrCreate(playerId);

    public int GetComboMultiplier(int playerId) => GetOrCreate(playerId).ComboMultiplier;

    private string FormatAllPlayerScores()
    {
        if (scores.Count == 0) return "  (no players tracked yet)";

        var sb = new System.Text.StringBuilder();
        foreach (var kvp in scores)
        {
            string playerLabel = kvp.Key == 0 ? "Red" : kvp.Key == 1 ? "Blue" : $"P{kvp.Key}";
            sb.AppendLine($"  Player {kvp.Key} ({playerLabel}): " +
                          $"AP={kvp.Value.ActionPoints} | " +
                          $"Multiplier={kvp.Value.ComboMultiplier}x | " +
                          $"Combo={kvp.Value.ComboMeter}");
        }
        return sb.ToString().TrimEnd();
    }
}