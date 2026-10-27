namespace Plates.Engine;

/// <summary>The rule 1 weight limit. A logged set may not go above it, and rule 3 holds every Target at it. Records may exceed it.</summary>
public static class LoadLimit
{
    public const decimal MaxKg = 500m;

    public static readonly Weight Max = Weight.FromKg(MaxKg);
}
