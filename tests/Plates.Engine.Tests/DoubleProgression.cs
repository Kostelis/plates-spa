using System.Text.Json;

namespace Plates.Engine.Tests;

/// <summary>Builds engine input for double-progression tests.</summary>
internal static class DoubleProgression
{
    public static readonly Guid ExerciseId = Guid.Parse("8d1f6c2e-3b4a-4f5e-9a7b-1c2d3e4f5a6b");

    /// <summary>The fixed "now" of every test: 27 October 2026, 18:00 UTC.</summary>
    public static readonly DateTimeOffset Now = new(2026, 10, 27, 18, 0, 0, TimeSpan.Zero);

    public static ProgressionEngine NewEngine() => new([new DoubleProgressionScheme()], new FixedTimeProvider(Now));

    /// <summary>Section 3 squat: 3 × 5–8, Increment 5 kg.</summary>
    public static ProgramExercise Squat() => Exercise(new DoubleProgressionParameters(
        sets: 3, minReps: 5, maxReps: 8, startingWeight: Weight.FromKg(60m), increment: Weight.FromKg(5m)));

    /// <summary>Section 3 bench press: 3 × 6–10, Increment 2,5 kg.</summary>
    public static ProgramExercise Bench() => Exercise(new DoubleProgressionParameters(
        sets: 3, minReps: 6, maxReps: 10, startingWeight: Weight.FromKg(47.5m), increment: Weight.FromKg(2.5m)));

    public static ProgramExercise Exercise(DoubleProgressionParameters parameters) =>
        new(ExerciseId, DoubleProgressionScheme.SchemeKey, JsonSerializer.SerializeToElement(parameters, JsonSerializerOptions.Web));

    public static ExerciseState State(decimal? recordKg, int failureCount = 0) => new(
        recordKg is { } kg ? Weight.FromKg(kg) : null,
        JsonSerializer.SerializeToElement(new DoubleProgressionState { FailureCount = failureCount }, JsonSerializerOptions.Web));

    public static ExerciseLog Log(params LoggedSet[] sets) => ExerciseLog.Of(ExerciseId, sets);

    /// <summary>A workout logged today against a program that holds only the squat.</summary>
    public static WorkoutInput Workout(params ExerciseLog[] logs) => Workout(Now, logs);

    public static WorkoutInput Workout(DateTimeOffset date, params ExerciseLog[] logs) => new([Squat()], date, logs);

    public static WorkoutInput Workout(ProgramExercise exercise, params ExerciseLog[] logs) => new([exercise], Now, logs);

    public static LoggedSet Set(decimal kg, int reps) => new(Weight.FromKg(kg), reps);
}
