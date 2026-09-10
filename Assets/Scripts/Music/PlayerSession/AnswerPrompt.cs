using System;

/// <summary>
/// One required input, locked to a specific beat in the song.
/// AnswerMaker creates a queue of these; RhythmEvaluator judges them
/// against player input.
/// </summary>
[Serializable]
public class AnswerPrompt
{
    public enum Result { Early,Pending, OnBeat, Late, Wrong }

    public RhythmDirection RequiredDirection;
    public double TargetSongTime;  
    public int TargetBeat;        
    public Result CurrentResult = Result.Pending;

    public AnswerPrompt(RhythmDirection direction, double targetSongTime, int targetBeat)
    {
        RequiredDirection = direction;
        TargetSongTime = targetSongTime;
        TargetBeat = targetBeat;
    }
}
