using System.Collections.ObjectModel;

namespace Plates.Engine;

/// <summary>Decides the next Target for an exercise with the progression scheme its program names.</summary>
public sealed class ProgressionEngine(IEnumerable<IProgressionScheme> schemes, TimeProvider? time = null)
{
    private readonly TimeProvider _time = time ?? TimeProvider.System;

    private Dictionary<string, IProgressionScheme>? _schemes;

    /// <summary>Validates the workout (rule 1) first; when it is valid, decides every logged exercise. Skipped exercises get no outcome.</summary>
    /// <remarks>The caller passes a state for every program exercise (created on assignment); a program lists each exercise once.</remarks>
    public WorkoutResult Progress(WorkoutInput input, IReadOnlyDictionary<Guid, ExerciseState> states)
    {
        if (!new WorkoutValidator(PlannedSets, _time).TryValidate(input, out var errors))
        {
            return new WorkoutResult(errors, ReadOnlyDictionary<Guid, ProgressionOutcome>.Empty);
        }

        var program = input.Program.ToDictionary(exercise => exercise.ExerciseId);
        var outcomes = input.Exercises.ToDictionary(
            log => log.ExerciseId,
            log => Progress(program[log.ExerciseId], states[log.ExerciseId], log));

        return new WorkoutResult(errors, outcomes);
    }

    /// <summary>Decides one exercise without rule 1; callers outside the engine go through the workout overload.</summary>
    internal ProgressionOutcome Progress(ProgramExercise exercise, ExerciseState state, ExerciseLog log)
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
