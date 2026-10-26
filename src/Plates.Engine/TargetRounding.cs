namespace Plates.Engine;

/// <summary>Rule 6: every Target is rounded down to a weight that two 1,25 kg plates can load.</summary>
public static class TargetRounding
{
    public const decimal PlateStepKg = 2.5m;

    public static Weight RoundDown(Weight target) => Weight.FromKg(Math.Floor(target.Kg / PlateStepKg) * PlateStepKg);
}
