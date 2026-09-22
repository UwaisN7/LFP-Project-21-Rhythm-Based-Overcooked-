using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PointManager : MonoBehaviour
{
    [System.Serializable]
    public class PlayerScore
    {
        public int ActionPoints;

        public int ComboMeter => ActionPoints;

    }

    [Header("Debug")]
    [SerializeField] private bool debugLogging = true;

    [SerializeField] private int pointsPerMultiplierTier = 100;

    [SerializeField] private bool clampActionPointsAtZero = true;

    [Header("Strike Penalties")]
    [SerializeField] private int strikeActionPointPenalty = 50;

    [Header("Test (Q Key)")]
    [SerializeField] private int testOrderDifficulty = 100;
    [SerializeField] private int testTimeLeft = 50;
    [SerializeField] private int testPlayerId = 0;

    public float player1FinalScore;
    public float player2FinalScore;

    private Dictionary<int, PlayerScore> scores = new Dictionary<int, PlayerScore>();

    public event System.Action<int, PlayerScore, int> OnScoreChanged;

    private PlayerScore GetOrCreate(int playerId)
    {
        if (!scores.TryGetValue(playerId, out var score))
        {
            score = new PlayerScore();
            scores[playerId] = score;
        }
        return score;
    }

    public int GetComboMultiplier(int playerId)
    {
        var score = GetOrCreate(playerId);
        return Mathf.Max(1, (score.ActionPoints / pointsPerMultiplierTier) + 1);
    }

    public void AddActionPoints(int playerId, int amount)
    {
        var score = GetOrCreate(playerId);
        int previousAP = score.ActionPoints;
        int previousMultiplier = GetComboMultiplier(playerId);

        score.ActionPoints += amount;

        if (clampActionPointsAtZero && score.ActionPoints < 0)
            score.ActionPoints = 0;

        int newMultiplier = GetComboMultiplier(playerId);

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
                $"Multiplier: {newMultiplier}x{tierChange}\n" 

            );
        }

        OnScoreChanged?.Invoke(playerId, score, newMultiplier);
    }

    public void ReportStrike(int playerId)
    {
        var score = GetOrCreate(playerId);
        int previousAP = score.ActionPoints;
        int previousMultiplier = GetComboMultiplier(playerId);

        score.ActionPoints -= strikeActionPointPenalty;

        if (clampActionPointsAtZero && score.ActionPoints < 0)
            score.ActionPoints = 0;

        int newMultiplier = GetComboMultiplier(playerId);

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
                $"Multiplier: {newMultiplier}x{tierChange}\n" 
                
            );
        }

        OnScoreChanged?.Invoke(playerId, score, newMultiplier);
    }

    public PlayerScore GetScore(int playerId) => GetOrCreate(playerId);

    public float CalculateFinalScore(int playerId, int orderDifficulty, int timeLeft)
    {
        int multiplier = GetComboMultiplier(playerId);
        float finalScore = (orderDifficulty + timeLeft) * multiplier;
        return finalScore;
    }

    public void ApplyFinalScore(int playerId, int orderDifficulty, int timeLeft)
    {
        float final = CalculateFinalScore(playerId, orderDifficulty, timeLeft);
        if (playerId == 0) player1FinalScore = final;
        else if (playerId == 1) player2FinalScore = final;

        Debug.Log($"[PointManager] Player {playerId} FINAL SCORE: {final} " +
                  $"(Diff {orderDifficulty} + Time {timeLeft}) * {GetComboMultiplier(playerId)}x = {final}");
    }
    public void AddFinalScore(int playerId, float amount)
    {
        if (playerId == 0) player1FinalScore += amount;
        else if (playerId == 1) player2FinalScore += amount;

        if (debugLogging)
        {
            float currentTotal = playerId == 0 ? player1FinalScore : player2FinalScore;
            Debug.Log($"[PointManager] Player {playerId} final score +{amount} | Total: {currentTotal}");
        }
    }
}