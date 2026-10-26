using System.Text.Json;

namespace Plates.Engine;

/// <summary>A client's state for one exercise before a workout. A null scheme state means the program was just assigned.</summary>
public sealed record ExerciseState(Weight? Record, JsonElement? SchemeState);
