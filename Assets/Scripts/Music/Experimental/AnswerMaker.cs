using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Builds the queue of AnswerPrompts for an ingredient.
/// The ingredient hands this a fixed pattern (e.g. Up, Up, Left, Right)
/// and this schedules one prompt per upcoming beat, starting a couple
/// of beats in the future so the player has reaction time.
/// </summary>
public class AnswerMaker : MonoBehaviour
{
    [Tooltip("How many beats from 'now' the first prompt should land on. Gives the player reaction time.")]
    [SerializeField] private int leadInBeats = 2;

    public Queue<AnswerPrompt> GenerateSequence(RhythmDirection[] pattern, MusicClock clock)
    {
        var prompts = new Queue<AnswerPrompt>();

        if (clock == null || pattern == null || pattern.Length == 0)
        {
            Debug.LogWarning("AnswerMaker: missing clock or empty pattern.");
            return prompts;
        }

        int startBeat = clock.CurrentBeat + leadInBeats;

        for (int i = 0; i < pattern.Length; i++)
        {
            int beatIndex = startBeat + i;
            double targetTime = clock.BeatToSongTime(beatIndex);

            prompts.Enqueue(new AnswerPrompt(pattern[i], targetTime, beatIndex));
        }

        return prompts;
    }
}
