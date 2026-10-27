using System.Diagnostics;

namespace Plates.Engine;

/// <summary>Double progression: add reps up to the top of the range, then add the Increment; repeated failures lower the weight by the Deload percentage.</summary>
public sealed class DoubleProgressionScheme(RecordCalculator? records = null)
    : ProgressionSchemeBase<DoubleProgressionParameters, DoubleProgressionState>(records)
{
    public const string SchemeKey = "double-progression";

    /// <summary>Every set planned was done and each reached the minimum reps.</summary>
    private const SetEvaluation Passed = SetEvaluation.AllSetsDone | SetEvaluation.MinReached;

    /// <summary>Rule 3: a Target never goes above the rule 1 weight limit.</summary>
    private static readonly Weight TargetLimit = Weight.FromKg(500m);

    public override string Key => SchemeKey;

    protected override int CountPlannedSets(DoubleProgressionParameters parameters) => parameters.Sets;

    protected override int FailureCount(DoubleProgressionState state) => state.FailureCount;

    protected override SchemeDecision<DoubleProgressionState> Decide(
        DoubleProgressionParameters parameters, DoubleProgressionState state, ExerciseLog log)
    {
        var baseWeight = log.Sets.Min(set => set.Weight);
        var increased = baseWeight + parameters.Increment;
        var failures = state.FailureCount + 1;
        var evaluation = Evaluate(parameters, log.Sets);
        var failed = (evaluation & Passed) != Passed;

        return evaluation switch
        {
            _ when failed && failures >= parameters.FailureLimit =>
                Decided(baseWeight - baseWeight * parameters.DeloadPercentage, DecisionCode.Deload, failureCount: 0),
            _ when failed =>
                Decided(baseWeight, DecisionCode.Failure, failureCount: failures),
            Passed | SetEvaluation.MaxReached when increased > TargetLimit =>
                Decided(TargetLimit, DecisionCode.Limit, failureCount: 0),
            Passed | SetEvaluation.MaxReached =>
                Decided(increased, DecisionCode.Increase, failureCount: 0),
            Passed =>
                Decided(baseWeight, DecisionCode.Hold, failureCount: 0),
            _ => throw new UnreachableException($"Unexpected set evaluation {evaluation}."),
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

    private static SchemeDecision<DoubleProgressionState> Decided(Weight target, DecisionCode code, int failureCount) =>
        new(target, code, new DoubleProgressionState { FailureCount = failureCount });
}
