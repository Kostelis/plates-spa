namespace Plates.Engine;

/// <summary>A coach's settings for one exercise under double progression.</summary>
public sealed class DoubleProgressionParameters
{
    private const int RepLimit = 50;

    public DoubleProgressionParameters(
        int sets,
        int minReps,
        int maxReps,
        Weight startingWeight,
        Weight increment,
        decimal deloadPercentage = 0.10m,
        int failureLimit = 2)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(sets, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(minReps, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(minReps, maxReps);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(maxReps, RepLimit);
        if (increment.Kg <= 0 || increment.Kg % TargetRounding.PlateStepKg != 0)
        {
            throw new ArgumentOutOfRangeException(nameof(increment), increment, "Increment must be a positive multiple of 2.5 kg.");
        }

        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(deloadPercentage, 0m);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(deloadPercentage, 1m);
        ArgumentOutOfRangeException.ThrowIfLessThan(failureLimit, 1);

        Sets = sets;
        MinReps = minReps;
        MaxReps = maxReps;
        StartingWeight = startingWeight;
        Increment = increment;
        DeloadPercentage = deloadPercentage;
        FailureLimit = failureLimit;
    }

    public int Sets { get; }

    public int MinReps { get; }

    public int MaxReps { get; }

    public Weight StartingWeight { get; }

    public Weight Increment { get; }

    public decimal DeloadPercentage { get; }

    public int FailureLimit { get; }
}
