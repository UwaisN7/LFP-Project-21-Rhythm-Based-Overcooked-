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

    [Header("Combo Decay (DMC Style)")]
    [SerializeField] private float timeBeforeDecayStarts = 1.5f;
    [SerializeField] private int actionPointsLostPerSecond = 30;

    private Dictionary<int, float> lastActionTimes = new Dictionary<int, float>();

    private Dictionary<int, float> decayAccumulators = new Dictionary<int, float>(); // NEW
    public event System.Action<int, string> OnActionLogged; // Ultrakill feed event

    private PlayerScore GetOrCreate(int playerId)
    {
        if (!scores.TryGetValue(playerId, out var score))
        {
            score = new PlayerScore();
            scores[playerId] = score;
        }
        return score;
    }
    private void Update()
    {
        foreach (var kvp in scores)
        {
            int pId = kvp.Key;
            if (!lastActionTimes.ContainsKey(pId)) continue;

            // If the player hasn't done anything in a while, start draining their combo
            if (Time.time - lastActionTimes[pId] > timeBeforeDecayStarts)
            {
                if (kvp.Value.ActionPoints > 0)
                {
                    // Ensure the accumulator exists for this player
                    if (!decayAccumulators.ContainsKey(pId)) decayAccumulators[pId] = 0f;

                    // Accumulate the fractional decay for this frame
                    decayAccumulators[pId] += actionPointsLostPerSecond * Time.deltaTime;

                    // If we've built up at least 1 full point of decay, subtract it
                    int pointsToDrop = Mathf.FloorToInt(decayAccumulators[pId]);

                    if (pointsToDrop > 0)
                    {
                        kvp.Value.ActionPoints -= pointsToDrop;
                        decayAccumulators[pId] -= pointsToDrop; // Remove the whole numbers, keep the remainder

                        if (kvp.Value.ActionPoints < 0) kvp.Value.ActionPoints = 0;

                        OnScoreChanged?.Invoke(pId, kvp.Value, GetComboMultiplier(pId));
                    }
                }
                else
                {
                    // Reset the accumulator once they hit rock bottom
                    decayAccumulators[pId] = 0f;
                }
            }
        }
    }

    public void LogAction(int pId, string actionText)
    {
        OnActionLogged?.Invoke(pId, actionText);
    }
    public int GetComboMultiplier(int playerId)
    {
        var score = GetOrCreate(playerId);
        return Mathf.Max(1, (score.ActionPoints / pointsPerMultiplierTier) + 1);
    }

    public void AddActionPoints(int playerId, int amount)
    {
        lastActionTimes[playerId] = Time.time;
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
        lastActionTimes[playerId] = Time.time;
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