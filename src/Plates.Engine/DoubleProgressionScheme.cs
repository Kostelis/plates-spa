namespace Plates.Engine;

/// <summary>Double progression: add reps up to the top of the range, then add the Increment.</summary>
public sealed class DoubleProgressionScheme(RecordCalculator? records = null)
    : ProgressionSchemeBase<DoubleProgressionParameters, DoubleProgressionState>(records)
{
    public const string SchemeKey = "double-progression";

    public override string Key => SchemeKey;

    protected override int CountPlannedSets(DoubleProgressionParameters parameters) => parameters.Sets;

    protected override SchemeDecision<DoubleProgressionState> Decide(
        DoubleProgressionParameters parameters, DoubleProgressionState state, ExerciseLog log)
    {
        var baseWeight = log.Sets.Min(set => set.Weight);

        return Evaluate(parameters, log.Sets) switch
        {
            SetEvaluation.AllSetsDone | SetEvaluation.MinReached | SetEvaluation.MaxReached =>
                Succeeded(baseWeight + parameters.Increment, DecisionCode.Increase),
            SetEvaluation.AllSetsDone | SetEvaluation.MinReached =>
                Succeeded(baseWeight, DecisionCode.Hold),
            var evaluation => throw new NotSupportedException($"Failed workouts ({evaluation}) are not decided yet."),
        };
    }

    private static SetEvaluation Evaluate(DoubleProgressionParameters parameters, IReadOnlyList<LoggedSet> sets)
    {
        var evaluation = SetEvaluation.None;
        if (sets.Count >= parameters.Sets)
        {
            evaluation |= SetEvaluation.AllSetsDone;
        }

        if (sets.All(set => set.Reps >= parameters.MinReps))
        {
            evaluation |= SetEvaluation.MinReached;
        }

        if (sets.All(set => set.Reps >= parameters.MaxReps))
        {
            evaluation |= SetEvaluation.MaxReached;
        }

        return evaluation;
    }

    private static SchemeDecision<DoubleProgressionState> Succeeded(Weight target, DecisionCode code) =>
        new(target, code, FailureCount: 0, new DoubleProgressionState());
}
