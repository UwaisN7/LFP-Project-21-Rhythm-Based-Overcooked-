using TMPro;
using UnityEngine;

public class RhythmFeedbackText : MonoBehaviour
{
    [Header("Motion")]
    [SerializeField] private float riseDistance = 0.6f;
    [SerializeField] private float lifetimeSeconds = 0.7f;

    [Header("Fade")]
    [SerializeField] private AnimationCurve alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

  
    private Vector3 startPos;
    private float elapsed;


    private TMP_Text text; // base class covers both TextMeshPro and TextMeshProUGUI

    private void Awake()
    {
        text = GetComponent<TMP_Text>();
        if (text == null)
        {
            Debug.LogError($"{nameof(RhythmFeedbackText)}: no TMP_Text component found.", this);
        }
    }

    public void Setup(string message, Color color, Vector3 worldPosition)
    {
        if (text == null) text = GetComponent<TMP_Text>();

        if (text != null)
        {
            text.text = message;
            text.color = color;
        }

        transform.position = worldPosition;
        startPos = worldPosition;
        elapsed = 0f;
    }

   

private void Update()
    {
        elapsed += Time.deltaTime;

        float t = lifetimeSeconds > 0f ? Mathf.Clamp01(elapsed / lifetimeSeconds) : 1f;

        // Rise.
        transform.position = startPos + Vector3.up * (riseDistance * t);

        // Fade.
        if (text != null)
        {
            Color c = text.color;
            c.a = alphaCurve.Evaluate(t);
            text.color = c;
        }

        if (elapsed >= lifetimeSeconds)
            Destroy(gameObject);
    }
}