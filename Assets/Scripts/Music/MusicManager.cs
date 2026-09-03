using UnityEngine;

public class MusicManager : MonoBehaviour
{
    [Header("Music")]
    [SerializeField] private MusicTrack musicTrack;
    [SerializeField] private AudioSource audioSource;

    [Header("System")]
    [SerializeField] private MusicClock musicClock;

    private void Start()
    {
        StartMusic();
    }

    public void StartMusic()
    {
        if (musicTrack == null)
        {
            Debug.LogError("MusicManager: No MusicTrack assigned.");
            return;
        }

        if (audioSource == null)
        {
            Debug.LogError("MusicManager: No AudioSource assigned.");
            return;
        }

        if (musicClock == null)
        {
            Debug.LogError("MusicManager: No MusicClock assigned.");
            return;
        }

        audioSource.clip = musicTrack.audioClip;

        /*
         * Give Unity a tiny amount of preparation time.
         * This creates a future DSP timestamp that both
         * the AudioSource and MusicClock can agree on.
         */
        double startTime = AudioSettings.dspTime + 2;

        audioSource.PlayScheduled(startTime);

        musicClock.Initialise(musicTrack, startTime);

        Debug.Log("Music scheduled to start.");
    }
}