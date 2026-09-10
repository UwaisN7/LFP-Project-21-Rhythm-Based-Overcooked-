using UnityEngine;

public class MusicEventDebugger : MonoBehaviour
{
    [SerializeField] private MusicClock musicClock;

    private void OnEnable()
    {
        if (musicClock == null)
            return;

        musicClock.OnBeat += HandleBeat;
        musicClock.OnBar += HandleBar;
    }

    private void OnDisable()
    {
        if (musicClock == null)
            return;

        musicClock.OnBeat -= HandleBeat;
        musicClock.OnBar -= HandleBar;
    }

    private void HandleBeat()
    {
        Debug.Log($"🎵 BEAT {musicClock.CurrentBeat}");
    }

    private void HandleBar()
    {
        Debug.Log($"🥁 BAR {musicClock.CurrentBar}");
    }
}