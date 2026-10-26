using System.Text.Json;

namespace Plates.Engine;

/// <summary>A progression scheme, selected by <see cref="Key"/>. Parameters and state stay JSON so a new scheme needs no migration.</summary>
public interface IProgressionScheme
{
    string Key { get; }

    int PlannedSets(JsonElement parameters);

    ProgressionOutcome Progress(JsonElement parameters, ExerciseState state, ExerciseLog log);
}
