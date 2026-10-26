namespace Plates.Engine;

/// <summary>Decides the next Target for an exercise with the progression scheme its program names.</summary>
public sealed class ProgressionEngine(IEnumerable<IProgressionScheme> schemes)
{
    private Dictionary<string, IProgressionScheme>? _schemes;

    public ProgressionOutcome Progress(ProgramExercise exercise, ExerciseState state, ExerciseLog log)
    {
        if (log.ExerciseId != exercise.ExerciseId)
        {
            throw new ArgumentException($"The log is for exercise {log.ExerciseId}, not {exercise.ExerciseId}.", nameof(log));
        }

        // A skipped exercise is left out of the workout, so its state stays as it is (clarified rule 9).
        if (log.Sets.Count == 0)
        {
            throw new ArgumentException($"The log for exercise {log.ExerciseId} has no sets.", nameof(log));
        }

        return Scheme(exercise.SchemeKey).Progress(exercise.Parameters, state, log);
    }

    public int PlannedSets(ProgramExercise exercise) => Scheme(exercise.SchemeKey).PlannedSets(exercise.Parameters);

    private IProgressionScheme Scheme(string key)
    {
        _schemes ??= schemes.ToDictionary(scheme => scheme.Key);

        return _schemes.TryGetValue(key, out var scheme)
            ? scheme
            : throw new ArgumentException($"Unknown progression scheme '{key}'.", nameof(key));
    }
}
