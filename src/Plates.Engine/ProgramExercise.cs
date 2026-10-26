using System.Text.Json;

namespace Plates.Engine;

/// <summary>An exercise in a program: which progression scheme decides it, and that scheme's parameters as JSON.</summary>
public sealed record ProgramExercise(Guid ExerciseId, string SchemeKey, JsonElement Parameters);
