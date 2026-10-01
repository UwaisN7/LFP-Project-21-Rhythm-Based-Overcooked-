using System;
using System.Collections;
using TMPro;
using UnityEngine;

public enum GameState
{
    PreGame,
    Playing,
    GameEnd
}

public class GameManager : MonoBehaviour
{
    [SerializeField] private PointManager pointManager;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private RectTransform countdownParent;

    [SerializeField] private float roundDuration = 90f;
    [SerializeField] private float countdownStepDuration = 1f;
    [SerializeField] private float riseDistance = 150f;
    [SerializeField] private float countdownFontSize = 200f;

    public GameState CurrentState { get; private set; }
    public float TimeRemaining { get; private set; }
    public float RoundDuration => roundDuration;

    public event Action<GameState> OnStateChanged;

    private Coroutine countdownRoutine;

    private void Awake()
    {
        if (pointManager == null)
            pointManager = FindAnyObjectByType<PointManager>();
    }

    private void Start()
    {
        SetState(GameState.PreGame);
    }

    private void Update()
    {
        if (CurrentState != GameState.Playing) return;

        TimeRemaining = Mathf.Max(0f, TimeRemaining - Time.deltaTime);
        UpdateTimerText();

        if (TimeRemaining <= 0f)
            SetState(GameState.GameEnd);
    }

    private void SetState(GameState newState)
    {
        if (countdownRoutine != null)
        {
            StopCoroutine(countdownRoutine);
            countdownRoutine = null;
        }

        CurrentState = newState;

        switch (newState)
        {
            case GameState.PreGame:
                TimeRemaining = roundDuration;
                UpdateTimerText();
                if (resultText != null) resultText.text = "";
                countdownRoutine = StartCoroutine(CountdownRoutine());
                break;

            case GameState.GameEnd:
                ShowResult();
                break;
        }

        OnStateChanged?.Invoke(newState);
    }

    private IEnumerator CountdownRoutine()
    {
        string[] steps = { "3", "2", "1", "GO!" };

        foreach (string step in steps)
            yield return RiseAndFade(step);

        countdownRoutine = null;
        SetState(GameState.Playing);
    }

    private IEnumerator RiseAndFade(string content)
    {
        GameObject go = new GameObject("Countdown_" + content, typeof(RectTransform));
        go.transform.SetParent(countdownParent, false);

        TextMeshProUGUI text = go.AddComponent<TextMeshProUGUI>();
        text.text = content;
        text.alignment = TextAlignmentOptions.Center;
        text.fontSize = countdownFontSize;
        text.color = Color.white;
        text.raycastTarget = false;

        RectTransform rect = text.rectTransform;
        rect.sizeDelta = new Vector2(600f, 300f);

        Vector2 start = Vector2.zero;
        Vector2 end = new Vector2(0f, riseDistance);

        float elapsed = 0f;
        while (elapsed < countdownStepDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / countdownStepDuration);
            rect.anchoredPosition = Vector2.Lerp(start, end, t);
            text.alpha = 1f - t;
            yield return null;
        }

        Destroy(go);
    }

    private void UpdateTimerText()
    {
        if (timerText == null) return;

        int total = Mathf.CeilToInt(TimeRemaining);
        timerText.text = $"{total / 60}:{total % 60:00}";
    }

    private void ShowResult()
    {
        if (resultText == null || pointManager == null) return;

        float red = pointManager.player1FinalScore;
        float blue = pointManager.player2FinalScore;

        if (red > blue)
        {
            resultText.text = "Red Player Wins!";
            resultText.color = Color.red;
        }
        else if (blue > red)
        {
            resultText.text = "Blue Player Wins!";
            resultText.color = Color.blue;
        }
        else
        {
            resultText.text = "It's a Draw!";
            resultText.color = Color.white;
        }
    }

    public void RestartRound()
    {
        SetState(GameState.PreGame);
    }


}