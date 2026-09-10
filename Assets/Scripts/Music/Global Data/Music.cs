using UnityEngine;

[CreateAssetMenu(fileName = "New Music Track", menuName = "Music System/Music Track")]
public class MusicTrack : ScriptableObject
{
    [Header("Audio")]
    public AudioClip audioClip;

    [Header("Musical Timing")]
    public float bpm = 120f;

    [Tooltip("Time in seconds before the first beat occurs.")]
    public float beatOffset = 0f;

    [Tooltip("Beats contained in one bar.")]
    public int beatsPerBar = 4;
}