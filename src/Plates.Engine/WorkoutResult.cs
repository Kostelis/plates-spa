namespace Plates.Engine;

/// <summary>Either the rule 1 errors, or the outcome of every logged exercise by exercise ID; never both.</summary>
public sealed record WorkoutResult(IReadOnlyList<ValidationError> Errors, IReadOnlyDictionary<Guid, ProgressionOutcome> Outcomes);
