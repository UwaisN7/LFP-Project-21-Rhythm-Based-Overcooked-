using UnityEngine;

/// <summary>
/// A single arrow that travels from a spawn point to a hit line and
/// arrives exactly on its target beat. Purely visual - the
/// RhythmEvaluator is the source of truth for hit/miss.
///
/// Tints itself via a per-instance material copy. Supports both
/// Renderer (MeshRenderer/SkinnedMeshRenderer) and SpriteRenderer,
/// and falls back through several common color property names.
/// </summary>
public class RhythmArrow : MonoBehaviour
{
    [Header("Visual")]
    [Tooltip("Source material. If left null, the arrow uses whatever material is already on its renderer.")]
    [SerializeField] private Material visual;

    [Tooltip("Optional override property name. Leave blank to auto-detect (_BaseColor, _Color, _TintColor).")]
    [SerializeField] private string colorPropertyOverride = "";

    [Header("Result colors (applied to the material)")]
    [SerializeField] private Color onBeatColor = Color.green;
    [SerializeField] private Color wrongColor = Color.red;
    [SerializeField] private Color lateColor = Color.gray;

    [Header("Cleanup")]
    [SerializeField] private float destroyDelay = 0.15f;

    private Renderer rend;
    private SpriteRenderer spriteRend;
    private Material runtimeMat;
    private int colorPropId = -1;

    private Transform tr;
    private RectTransform rect;

    private MusicClock musicClock;
    private double spawnSongTime;
    private double targetSongTime;
    private Vector3 startPos;
    private Vector3 endPos;
    private bool resolved;

    public AnswerPrompt Prompt { get; private set; }

    public void Setup(AnswerPrompt prompt, MusicClock clock, double spawnTime, Vector3 spawnPos, Vector3 hitPos)
    {
        Prompt = prompt;
        musicClock = clock;
        spawnSongTime = spawnTime;
        targetSongTime = prompt.TargetSongTime;
        startPos = spawnPos;
        endPos = hitPos;

        tr = transform;
        rect = tr as RectTransform;

        CacheRendererAndMaterial();
        ApplyPosition(0f);
    }

    private void CacheRendererAndMaterial()
    {
        // Try both in case the prefab uses a mesh renderer or a sprite renderer.
        rend = GetComponent<Renderer>();
        spriteRend = GetComponent<SpriteRenderer>();

        if (rend != null)
        {
            // If a source material was provided, assign it first.
            if (visual != null)
                rend.sharedMaterial = visual;

            // .material instantiates a per-object copy.
            runtimeMat = rend.material;
        }
        else if (spriteRend != null)
        {
            if (visual != null)
                spriteRend.sharedMaterial = visual;

            runtimeMat = spriteRend.material;
        }
        else
        {
            Debug.LogWarning($"{nameof(RhythmArrow)} on '{name}': no Renderer or SpriteRenderer found. Color changes will do nothing.", this);
            return;
        }

        if (runtimeMat == null)
        {
            Debug.LogWarning($"{nameof(RhythmArrow)} on '{name}': runtime material is null.", this);
            return;
        }

        colorPropId = ResolveColorProperty(runtimeMat);
        if (colorPropId == -1)
        {
            Debug.LogWarning(
                $"{nameof(RhythmArrow)} on '{name}': could not find a color property on material '{runtimeMat.shader.name}'. " +
                $"Tried: _BaseColor, _Color, _TintColor. Set 'colorPropertyOverride' to the correct name.", this);
        }
    }

    private int ResolveColorProperty(Material mat)
    {
        if (!string.IsNullOrEmpty(colorPropertyOverride))
            return Shader.PropertyToID(colorPropertyOverride);

        if (mat.HasProperty("_BaseColor")) return Shader.PropertyToID("_BaseColor"); // URP/HDRP Lit
        if (mat.HasProperty("_Color")) return Shader.PropertyToID("_Color");     // Built-in Standard, Sprites/Default
        if (mat.HasProperty("_TintColor")) return Shader.PropertyToID("_TintColor"); // Particles

        return -1;
    }

    private void Update()
    {
        if (musicClock == null) return;

        double duration = targetSongTime - spawnSongTime;
        double t = duration > 0.0 ? (musicClock.SongTime - spawnSongTime) / duration : 1.0;
        ApplyPosition(Mathf.Clamp01((float)t));
    }

    private void ApplyPosition(float t)
    {
        Vector3 pos = Vector3.Lerp(startPos, endPos, t);

        if (rect != null)
        {
            rect.anchoredPosition = new Vector2(pos.x, rect.anchoredPosition.y);
        }
        else if (tr != null)
        {
            tr.position = pos;
        }
    }

    public void ShowResult(AnswerPrompt.Result result)
    {
        if (resolved) return;
        resolved = true;

        if (result == AnswerPrompt.Result.Early)
        {
            Destroy(gameObject);
            return;
        }

        if (runtimeMat != null && colorPropId != -1)
        {
            Color c = runtimeMat.GetColor(colorPropId);
            switch (result)
            {
                case AnswerPrompt.Result.OnBeat: c = onBeatColor; break;
                case AnswerPrompt.Result.Wrong: c = wrongColor; break;
                case AnswerPrompt.Result.Late: c = lateColor; break;
            }

            // Preserve alpha in case the source color had transparency.
            c.a = runtimeMat.GetColor(colorPropId).a;
            runtimeMat.SetColor(colorPropId, c);
        }

        Destroy(gameObject, destroyDelay);
    }
}