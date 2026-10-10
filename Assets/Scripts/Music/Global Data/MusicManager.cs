using UnityEngine;

public class MusicManager : MonoBehaviour
{
    [Header("Music")]
    [Header("Winner Themes")]
    [SerializeField] private MusicTrack player1WinningTrack;
    [SerializeField] private MusicTrack player2WinningTrack;
    [SerializeField] private MusicTrack neutralTrack;
    [SerializeField] private MusicTrack musicTrack;

    private MusicTrack _currentTrack;
    private bool _isPlayingTheme = false;
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

     public void PlayWinnerTheme(int winnerState)
    {
        if (audioSource == null || musicClock == null)
        {
            Debug.LogWarning("MusicManager: Cannot play theme, missing refs.");
            return;
        }

        MusicTrack target = null;

        switch (winnerState)
        {
            case 0: target = neutralTrack; break;
            case 1: target = player1WinningTrack; break;
            case 2: target = player2WinningTrack; break;
        }

        // No track assigned for this state — do nothing (keep current music)
        if (target == null) return;

        // Already playing this theme? Don't restart.
        if (_isPlayingTheme && _currentTrack == target) return;

        _currentTrack = target;
        _isPlayingTheme = true;

        audioSource.clip = target.audioClip;

        // Same pattern as StartMusic — schedule ahead so MusicClock agrees
        double startTime = AudioSettings.dspTime + 0.25; // short crossfade window
        audioSource.PlayScheduled(startTime);

        musicClock.Initialise(target, startTime);

        Debug.Log($"[MusicManager] Winner theme {winnerState} started: {target.name}");
    }
}