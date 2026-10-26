namespace Plates.Engine;

/// <summary>Decides the next Target for an exercise with the progression scheme its program names.</summary>
public sealed class ProgressionEngine(IEnumerable<IProgressionScheme> schemes)
{
    private Dictionary<string, IProgressionScheme>? _schemes;

    public ProgressionOutcome Progress(ProgramExercise exercise, ExerciseState state, ExerciseLog log) =>
        Scheme(exercise.SchemeKey).Progress(exercise.Parameters, state, log);

    public int PlannedSets(ProgramExercise exercise) => Scheme(exercise.SchemeKey).PlannedSets(exercise.Parameters);

    private IProgressionScheme Scheme(string key)
    {
        _schemes ??= schemes.ToDictionary(scheme => scheme.Key);

        return _schemes.TryGetValue(key, out var scheme)
            ? scheme
            : throw new ArgumentException($"Unknown progression scheme '{key}'.", nameof(key));
    }
}
