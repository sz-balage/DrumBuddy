namespace DrumBuddy.Client.Models;

public record EvaluationBox(
    int MeasureIndex,
    int StartRgIndex,
    int EndRgIndex,
    EvaluationState State
);