using System;


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
