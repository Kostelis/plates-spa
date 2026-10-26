namespace Plates.Engine;

/// <summary>Rule 7: the Record is the best estimated one-rep max, rounded to the nearest 0,1 kg.</summary>
public sealed class RecordCalculator(OneRepMaxFormula? formula = null)
{
    private const int MaxRecordReps = 12;

    public static readonly OneRepMaxFormula Epley = (weightKg, reps) => weightKg * (1 + reps / 30m);

    private readonly OneRepMaxFormula _formula = formula ?? Epley;

    /// <summary>Returns the new Record, or <paramref name="record"/> when no set beats it. Sets above 12 reps never count.</summary>
    public Weight? Next(Weight? record, IEnumerable<LoggedSet> sets)
    {
        var estimates = sets.Where(set => set.Reps <= MaxRecordReps).Select(Estimate).ToList();
        if (estimates.Count == 0)
        {
            return record;
        }

        // No Record yet: record?.Kg is null, the comparison is false, and the estimate becomes the Record.
        var best = estimates.Max();
        return record?.Kg >= best ? record : Weight.FromKg(best);
    }

    private decimal Estimate(LoggedSet set)
    {
        var (weight, reps) = set;
        var oneRepMax = reps == 1 ? weight.Kg : _formula(weight.Kg, reps);
        return Math.Round(oneRepMax, 1, MidpointRounding.AwayFromZero);
    }
}
