using TMPro;
using UnityEngine;

/// <summary>
/// Attach to a TextMeshProUGUI (or TextMeshPro) at bottom-left (red) or
/// bottom-right (blue). Shows "x{combo}" for one player.
/// Stays hidden at combo 0.
/// </summary>
public class ComboManager : MonoBehaviour
{
    [SerializeField] private PointManager pointManager;
    [SerializeField] private int playerId = 0;
    [SerializeField] private Color comboColor = Color.red; 
    [SerializeField] private TMP_Text label; 

    private void OnEnable()
    {
        if (pointManager != null)
            pointManager.OnScoreChanged += HandleScoreChanged;
    }

    private void OnDisable()
    {
        if (pointManager != null)
            pointManager.OnScoreChanged -= HandleScoreChanged;
    }

    private void Start()
    {
        label.color = comboColor;
        label.text = "0X";
    }

    private void HandleScoreChanged(int changedPlayerId, PointManager.PlayerScore score)
    {
        if (changedPlayerId != playerId) return;

        if (score.ComboMeter <= 0)
            label.text = "0X";
        else
            label.text = "x" + score.ComboMeter;
    }
}