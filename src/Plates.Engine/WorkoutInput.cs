namespace Plates.Engine;

/// <summary>A logged workout and the program it is checked against. Exercises the client skipped are left out of <see cref="Exercises"/>.</summary>
public sealed record WorkoutInput(IReadOnlyList<ProgramExercise> Program, DateTimeOffset Date, IReadOnlyList<ExerciseLog> Exercises);
