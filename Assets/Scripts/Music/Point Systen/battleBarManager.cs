using UnityEngine;


using UnityEngine.UI;

public class battleBarManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PointManager pointManager;
    [SerializeField] private Image redFillImage;
    [SerializeField] private Image blueFillImage;
    [SerializeField] private RectTransform barContainer;

    [Header("Animation")]
    [Tooltip("How fast the bar catches up to the real score. Higher = snappier.")]
    [SerializeField] private float lerpSpeed = 8f;

    // +1 = red dominates, -1 = blue dominates, 0 = tie.
    private float displayedBalance = 0f;

    private void Start()
    {
        // Snap to correct value on start so there's no animation from center.
        displayedBalance = ComputeTargetBalance();
        ApplyBalance(displayedBalance);
    }

    private void Update()
    {
        float target = ComputeTargetBalance();
        displayedBalance = Mathf.Lerp(displayedBalance, target, Time.deltaTime * lerpSpeed);
        ApplyBalance(displayedBalance);
    }

    private float ComputeTargetBalance()
    {
        if (pointManager == null) return 0f;

        // Read the final scores directly from the inspector-tunable fields.
        float r = Mathf.Max(0f, pointManager.player1FinalScore);
        float b = Mathf.Max(0f, pointManager.player2FinalScore);

        float total = r + b;
        if (total <= 0f) return 0f; // tie at zero

        return (r - b) / total; // range -1..+1
    }

    private void ApplyBalance(float balance)
    {
        float redPercent = (1f + balance) * 0.5f;
        float bluePercent = (1f - balance) * 0.5f;

        // Drive the Image's fillAmount instead of the RectTransform width.
        redFillImage.fillAmount = redPercent;
        blueFillImage.fillAmount = bluePercent;
    }
}