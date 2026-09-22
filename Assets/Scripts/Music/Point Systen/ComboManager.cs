using TMPro;
using UnityEngine;


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

    private void HandleScoreChanged(int changedPlayerId, PointManager.PlayerScore score, int multiplier)
    {
        if (changedPlayerId != playerId) return;
        label.text = "x" + multiplier;
    }
}