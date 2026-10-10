using UnityEngine;

public class WinnerWatcher : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PointManager pointManager;
    [SerializeField] private MusicManager musicManager;
    [SerializeField] private ClubSpotlights clubSpotlights;

    [Header("Settings")]
    [Tooltip("How often to check who's winning (seconds). Small value = responsive.")]
    [SerializeField] private float checkInterval = 0.5f;

    [Tooltip("Score difference required before declaring a leader (avoids jitter at ties)")]
    [SerializeField] private float tieThreshold = 1f;

    [Tooltip("Update spotlight colors based on winner?")]
    [SerializeField] private bool driveSpotlightColors = true;

    [Tooltip("Update music based on winner?")]
    [SerializeField] private bool driveMusic = true;

    // 0 = tied, 1 = P1 winning, 2 = P2 winning
    private int _lastState = -1;
    private float _timer;

    void Start()
    {
        // Force an immediate check on start
        _timer = checkInterval;
    }

    void Update()
    {
        _timer -= Time.deltaTime;
        if (_timer > 0f) return;
        _timer = checkInterval;

        if (pointManager == null) return;

        int newState = EvaluateWinner();

        if (newState == _lastState) return; // nothing changed, bail

        _lastState = newState;
        ApplyState(newState);
    }

    private int EvaluateWinner()
    {
        float p1 = pointManager.player1FinalScore;
        float p2 = pointManager.player2FinalScore;

        float diff = p1 - p2;

        if (Mathf.Abs(diff) <= tieThreshold) return 0; // tied
        return diff > 0f ? 1 : 2;                       // 1 = P1, 2 = P2
    }

    private void ApplyState(int state)
    {
        Debug.Log($"[WinnerWatcher] State changed -> {StateName(state)}");

        if (driveSpotlightColors && clubSpotlights != null)
        {
            Color c;
            switch (state)
            {
                case 1: c = clubSpotlights.player1Color; break;
                case 2: c = clubSpotlights.player2Color; break;
                default: c = clubSpotlights.tiedColor; break;
            }
            clubSpotlights.SetThemeColor(c);
        }

        if (driveMusic && musicManager != null)
        {
            musicManager.PlayWinnerTheme(state);
        }
    }

    private string StateName(int state)
    {
        switch (state)
        {
            case 1: return "PLAYER 1 WINNING (Red)";
            case 2: return "PLAYER 2 WINNING (Blue)";
            default: return "TIED (Green)";
        }
    }
}