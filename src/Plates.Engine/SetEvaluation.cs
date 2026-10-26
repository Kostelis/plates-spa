namespace Plates.Engine;

/// <summary>What a workout's sets reached in double progression.</summary>
[Flags]
public enum SetEvaluation
{
    None = 0,
    AllSetsDone = 1,
    MinReached = 2,
    MaxReached = 4,
}
