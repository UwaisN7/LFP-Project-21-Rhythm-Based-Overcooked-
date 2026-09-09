using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Central authority for action points and combo meters.
/// Each player's RhythmEvaluator reports results here.
/// Keyed by playerId so this is already ready for player 2
/// when you get to session 2 - no changes needed here.
/// </summary>
public class PointManager : MonoBehaviour
{
    [System.Serializable]
    public class PlayerScore
    {
        public int ActionPoints;
        public int ComboMeter;
    }

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
        score.ActionPoints += amount;

        // Every successful hit grows the combo; anything that costs points resets it.
        if (amount > 0)
            score.ComboMeter++;
        else
            score.ComboMeter = 0;

        OnScoreChanged?.Invoke(playerId, score);
    }

    public void ReportStrike(int playerId)
    {
        var score = GetOrCreate(playerId);
        score.ComboMeter = 0;
        OnScoreChanged?.Invoke(playerId, score);
    }

    public PlayerScore GetScore(int playerId) => GetOrCreate(playerId);
}
