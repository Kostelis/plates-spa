namespace Plates.Engine;

/// <summary>One rule 1 error with a Lithuanian message. Null ExerciseId means the whole workout; null SetNumber means the whole exercise.</summary>
public sealed record ValidationError(Guid? ExerciseId, int? SetNumber, string Message);
