namespace Plates.Engine;

/// <summary>The sets logged for one exercise in a workout, in the order they were done.</summary>
public sealed class ExerciseLog
{
    private ExerciseLog(Guid exerciseId, IReadOnlyList<LoggedSet> sets)
    {
        ExerciseId = exerciseId;
        Sets = sets;
    }

    public Guid ExerciseId { get; }

    public IReadOnlyList<LoggedSet> Sets { get; }

    public static ExerciseLog Of(Guid exerciseId, params LoggedSet[] sets) => new(exerciseId, [.. sets]);
}
