using System.Text.Json;

namespace Plates.Engine;

/// <summary>
/// The shared pipeline of every scheme: deserialize → <see cref="Decide"/> → rounding (rule 6) → records (rule 7) → serialize state.
/// A new scheme defines its parameter and state types and overrides <see cref="Decide"/>, <see cref="CountPlannedSets"/> and <see cref="FailureCount"/>.
/// </summary>
public abstract class ProgressionSchemeBase<TParameters, TState>(RecordCalculator? records) : IProgressionScheme
    where TState : new()
{
    private readonly RecordCalculator _records = records ?? new RecordCalculator();

    public abstract string Key { get; }

    public int PlannedSets(JsonElement parameters) => CountPlannedSets(Read<TParameters>(parameters));

    public ProgressionOutcome Progress(JsonElement parameters, ExerciseState state, ExerciseLog log)
    {
        var schemeState = state.SchemeState is { } json ? Read<TState>(json) : new TState();
        var decision = Decide(Read<TParameters>(parameters), schemeState, log);

        return new ProgressionOutcome(
            TargetRounding.RoundDown(decision.Target),
            decision.Code,
            DecisionTexts.For(decision.Code),
            FailureCount(decision.State),
            _records.Next(state.Record, log.Sets),
            JsonSerializer.SerializeToElement(decision.State, JsonSerializerOptions.Web));
    }

    protected abstract int CountPlannedSets(TParameters parameters);

    /// <summary>The Failure counter the outcome reports; the state is its only owner.</summary>
    protected abstract int FailureCount(TState state);

    /// <summary>Decides the next Target before rounding. Caps such as the 500 kg limit belong here.</summary>
    protected abstract SchemeDecision<TState> Decide(TParameters parameters, TState state, ExerciseLog log);

    private static T Read<T>(JsonElement json) =>
        json.Deserialize<T>(JsonSerializerOptions.Web) ?? throw new JsonException($"Expected {typeof(T).Name}, got null.");
}
