using UnityEngine;

public class ClubSpotlights : MonoBehaviour
{
    [Header("Spotlights")]
    public Light[] spotlights;

    [Header("Movement Settings")]
    [Tooltip("Max horizontal swing (left/right) from base direction, in degrees")]
    public float maxYaw = 45f;

    [Tooltip("Max vertical tilt (forward/back) from base direction, in degrees")]
    public float maxPitch = 30f;

    [Tooltip("How fast the light moves to a new target")]
    public float moveSpeed = 3f;

    [Tooltip("How often each light picks a new target (seconds)")]
    public float changeInterval = 1.5f;

    [Tooltip("Random offset so lights don't all move in sync")]
    public float timingVariance = 0.5f;

    [Header("Intensity Effects (optional)")]
    public bool pulseIntensity = true;
    public float minIntensity = 1f;
    public float maxIntensity = 8f;
    public float pulseSpeed = 2f;

    [Header("Color Effects (optional)")]
    public bool changeColors = true;
    public Color[] colors = new Color[]
    {
        new Color(1f, 0f, 0.3f),
        new Color(0f, 0.5f, 1f),
        new Color(0.5f, 0f, 1f),
        new Color(0f, 1f, 0.5f),
        new Color(1f, 0.6f, 0f),
    };

    // Internal per-spotlight data
    private Quaternion[] _targetRotations;
    private float[] _timers;
    private float[] _nextChangeTimes;
    [Header("Winner Theme Colors")]
    [Tooltip("Color when Player 1 is winning")]
    public Color player1Color = new Color(1f, 0.15f, 0.15f); // Red

    [Tooltip("Color when Player 2 is winning")]
    public Color player2Color = new Color(0.2f, 0.4f, 1f);   // Blue

    [Tooltip("Color when tied")]
    public Color tiedColor = new Color(0.15f, 1f, 0.4f);     // Green

    [Tooltip("If true, the winner theme overrides the random color picker")]
    public bool useWinnerTheme = false;

    private Color? _forcedColor = null;

    void Start()
    {
        if (spotlights == null || spotlights.Length == 0)
        {
            Debug.LogWarning("ClubSpotlights: No spotlights assigned!");
            return;
        }

        int count = spotlights.Length;
        _targetRotations = new Quaternion[count];
        _timers = new float[count];
        _nextChangeTimes = new float[count];

        for (int i = 0; i < count; i++)
        {
            if (spotlights[i] == null) continue;

            _targetRotations[i] = spotlights[i].transform.rotation;
            _nextChangeTimes[i] = changeInterval + Random.Range(-timingVariance, timingVariance);
            _timers[i] = Random.Range(0f, 10f);
        }
    }

    void Update()
    {
        if (spotlights == null) return;

        for (int i = 0; i < spotlights.Length; i++)
        {
            Light light = spotlights[i];
            if (light == null) continue;

            _nextChangeTimes[i] -= Time.deltaTime;
            if (_nextChangeTimes[i] <= 0f)
            {
                PickNewTarget(i);
                _nextChangeTimes[i] = changeInterval + Random.Range(-timingVariance, timingVariance);
            }

            light.transform.rotation = Quaternion.Slerp(
                light.transform.rotation,
                _targetRotations[i],
                Time.deltaTime * moveSpeed
            );
            if (_forcedColor.HasValue)
                light.color = _forcedColor.Value;

            if (pulseIntensity)
            {
                _timers[i] += Time.deltaTime * pulseSpeed;
                float noise = Mathf.PerlinNoise(_timers[i], i * 10f);
                light.intensity = Mathf.Lerp(minIntensity, maxIntensity, noise);
            }
        }
    }

    void PickNewTarget(int index)
    {
        Light light = spotlights[index];

        // Get the current euler angles and only randomize the Y (yaw)
        // and offset X (tilt) within safe limits. Z stays at 0.
        Vector3 euler = light.transform.eulerAngles;

        // Randomize yaw fully around (left/right swing)
        float newYaw = Random.Range(-maxYaw, maxYaw);

        // For pitch: since the light points DOWN at ~90°, "forward tilt"
        // means going below 90° (e.g. 90 -> 60 tilts the beam forward),
        // and going above 90° (e.g. 90 -> 120) tilts it backwards.
        // We clamp within the safe range so the beam stays on the floor.
        float newPitch = Random.Range(90f - maxPitch, 90f + maxPitch);

        // Build the target rotation from scratch, avoiding drift/accumulation.
        // Note: we ignore the current yaw so it moves cleanly around the base.
        _targetRotations[index] = Quaternion.Euler(newPitch, newYaw, 0f);

        if (changeColors && colors != null && colors.Length > 0)
        {
            if (!_forcedColor.HasValue)
                light.color = colors[Random.Range(0, colors.Length)];
        }
    }
    public void SetThemeColor(Color? color)
    {
        _forcedColor = color;

        if (color.HasValue && spotlights != null)
        {
            for (int i = 0; i < spotlights.Length; i++)
            {
                if (spotlights[i] != null)
                    spotlights[i].color = color.Value;
            }
        }
    }
}