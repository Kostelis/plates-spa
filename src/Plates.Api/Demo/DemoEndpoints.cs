using System.Globalization;
using System.Text.Json;
using Plates.Engine;

namespace Plates.Api.Demo;

/// <summary>
/// The interim demo endpoint: the page sends the program, its current states and a workout, and gets the engine's outcomes back.
/// Nothing is stored; step 4 replaces it with the real workout API.
/// </summary>
public static class DemoEndpoints
{
    private static readonly CultureInfo Lithuanian = CultureInfo.GetCultureInfo("lt-LT");

    public static void MapDemoEndpoints(this IEndpointRouteBuilder app) =>
        app.MapPost("/demo/progress", Progress);

    private static IResult Progress(DemoProgressRequest request, ProgressionEngine engine)
    {
        var program = request.Program
            .Select(exercise => new ProgramExercise(exercise.ExerciseId, exercise.SchemeKey, exercise.Parameters))
            .ToList();
        var logs = request.Exercises
            .Select(log => ExerciseLog.Of(log.ExerciseId, [.. log.Sets.Select(set => new LoggedSet(Weight.FromKg(set.Weight), set.Reps))]))
            .ToList();

        // An exercise without a state was just assigned: no Record, no failures.
        var states = program.ToDictionary(exercise => exercise.ExerciseId, _ => new ExerciseState(null, null));
        foreach (var state in request.States)
        {
            states[state.ExerciseId] = new ExerciseState(state.Record is { } kg ? Weight.FromKg(kg) : null, state.SchemeState);
        }

        var result = engine.Progress(new WorkoutInput(program, request.Date, logs), states);
        if (result.Errors.Count > 0)
        {
            return Results.BadRequest(new DemoErrorResponse(result.Errors));
        }

        var outcomes = result.Outcomes
            .Select(pair => new DemoOutcome(
                pair.Key,
                pair.Value.Target.Kg,
                pair.Value.Target.ToString("G", Lithuanian),
                pair.Value.Code.ToString(),
                pair.Value.Reason,
                pair.Value.FailureCount,
                pair.Value.Record?.Kg,
                pair.Value.Record?.ToString("G", Lithuanian),
                pair.Value.SchemeState))
            .ToList();

        return Results.Ok(new DemoProgressResponse(outcomes));
    }
}

public sealed record DemoProgressRequest(
    IReadOnlyList<DemoProgramExercise> Program,
    IReadOnlyList<DemoExerciseState> States,
    DateTimeOffset Date,
    IReadOnlyList<DemoExerciseLog> Exercises);

/// <summary>Name is for display only; the engine works with the ID.</summary>
public sealed record DemoProgramExercise(Guid ExerciseId, string Name, string SchemeKey, JsonElement Parameters);

public sealed record DemoExerciseState(Guid ExerciseId, decimal? Record, JsonElement? SchemeState);

public sealed record DemoExerciseLog(Guid ExerciseId, IReadOnlyList<DemoSet> Sets);

public sealed record DemoSet(decimal Weight, int Reps);

public sealed record DemoProgressResponse(IReadOnlyList<DemoOutcome> Outcomes);

/// <summary>Weights in kg as numbers, plus texts formatted by <see cref="Weight"/> („62,5 kg“).</summary>
public sealed record DemoOutcome(
    Guid ExerciseId,
    decimal Target,
    string TargetText,
    string Code,
    string Reason,
    int FailureCount,
    decimal? Record,
    string? RecordText,
    JsonElement SchemeState);

public sealed record DemoErrorResponse(IReadOnlyList<ValidationError> Errors);
