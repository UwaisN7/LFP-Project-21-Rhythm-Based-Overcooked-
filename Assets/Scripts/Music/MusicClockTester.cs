using UnityEngine;

public class MusicClockDebugger : MonoBehaviour
{
    [SerializeField] private MusicClock musicClock;

    private int lastBeat = -1;

    private void Update()
    {
        if (musicClock == null)
            return;

        if (musicClock.CurrentBeat != lastBeat)
        {
            lastBeat = musicClock.CurrentBeat;

            Debug.Log(
                $"Beat: {musicClock.CurrentBeat} | " +
                $"Bar: {musicClock.CurrentBar} | " +
                $"Song Time: {musicClock.SongTime:F2}"
            );
        }
    }
}
