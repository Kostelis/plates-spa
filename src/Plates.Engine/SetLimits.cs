namespace Plates.Engine;

/// <summary>
/// The rule 1 limits of one logged set. Programs stay within them, and rule 3 holds every Target at <see cref="MaxWeight"/>.
/// Records may exceed it.
/// </summary>
public static class SetLimits
{
    public const int MinReps = 1;
    public const int MaxReps = 50;
    public const decimal MaxKg = 500m;
    public const decimal WeightStepKg = 0.5m;

    public static readonly Weight MaxWeight = Weight.FromKg(MaxKg);
}
