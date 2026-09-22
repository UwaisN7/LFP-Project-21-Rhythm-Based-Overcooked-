using UnityEngine;

public class MusicClock : MonoBehaviour
{
    private MusicTrack currentTrack;

    private double songStartTime;
    private double secondsPerBeat;
    public event System.Action OnBeat;
    public event System.Action OnBar;
    public int CurrentBeat { get; private set; }
    public int CurrentBar { get; private set; }
    public double SecondsPerBeat => secondsPerBeat;
    public double SongTime
    {
        get
        {
            if (currentTrack == null)
                return 0;

            return AudioSettings.dspTime - songStartTime;
        }
    }

    public void Initialise(MusicTrack track, double startTime)
    {
        currentTrack = track;
        songStartTime = startTime;

        secondsPerBeat = 60.0 / track.bpm;

        CurrentBeat = 0;
        CurrentBar = 0;
    }

    public double BeatToSongTime(int beatIndex)
    {
        if (currentTrack == null)
            return 0;

        return currentTrack.beatOffset + (beatIndex * secondsPerBeat);
    }

    private void Update()
    {
        if (currentTrack == null)
            return;

        UpdateMusicalPosition();
    }

    private void UpdateMusicalPosition()
    {
        double adjustedTime = SongTime - currentTrack.beatOffset;

        if (adjustedTime < 0)
            return;

        int newBeat = Mathf.FloorToInt(
            (float)(adjustedTime / secondsPerBeat)
        );

        if (newBeat != CurrentBeat)
        {
            int previousBar = CurrentBar;

            CurrentBeat = newBeat;

            CurrentBar = CurrentBeat / currentTrack.beatsPerBar;

            OnBeat?.Invoke();

            if (CurrentBar != previousBar)
            {
                OnBar?.Invoke();
            }
        }
    }
}