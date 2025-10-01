using DrumBuddy.Core.Models;
using DrumBuddy.Models;

namespace DrumBuddy.Services;

public static class MeasureEvaluator
{
    public static EvaluationBox EvaluateGroup(
        int measureIndex,
        int groupIndex,
        RythmicGroup overlayGroup,
        RythmicGroup recordedGroup)
    {
        var state = overlayGroup.Equals(recordedGroup)
            ? EvaluationState.Correct
            : EvaluationState.Incorrect;

        return new EvaluationBox(measureIndex, groupIndex, groupIndex, state);
    }
}