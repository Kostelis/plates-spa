using System.Text.Json;

namespace Plates.Engine;

/// <summary>The engine's decision for one exercise: the next Target, why, and the state to store for the next workout.</summary>
public sealed record ProgressionOutcome(
    Weight Target,
    DecisionCode Code,
    string Reason,
    int FailureCount,
    Weight? Record,
    JsonElement SchemeState);
