namespace Plates.Engine.Tests;

public class WeightArithmeticTests
{
    [Fact]
    public void DefaultIsZeroKg()
    {
        Assert.Equal(0m, default(Weight).Kg);
    }

    [Fact]
    public void RejectsNegativeKg()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Weight.FromKg(-0.5m));
    }

    [Fact]
    public void AllowsMoreThan500Kg()
    {
        Assert.Equal(520.5m, Weight.FromKg(520.5m).Kg);
    }

    [Fact]
    public void AddsIncrementToBaseWeight()
    {
        Assert.Equal(Weight.FromKg(65m), Weight.FromKg(60m) + Weight.FromKg(5m));
    }

    [Fact]
    public void AddsPastTheLimit()
    {
        Assert.Equal(Weight.FromKg(503m), Weight.FromKg(498m) + Weight.FromKg(5m));
    }

    [Fact]
    public void SubtractsWeight()
    {
        Assert.Equal(Weight.FromKg(57.5m), Weight.FromKg(60m) - Weight.FromKg(2.5m));
        Assert.Equal(default, Weight.FromKg(60m) - Weight.FromKg(60m));
    }

    [Fact]
    public void RejectsSubtractionBelowZero()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Weight.FromKg(2.5m) - Weight.FromKg(5m));
    }

    [Fact]
    public void DeloadsByDeloadPercentage()
    {
        var baseWeight = Weight.FromKg(47.5m);

        Assert.Equal(Weight.FromKg(42.75m), baseWeight - baseWeight * 0.10m);
    }

    [Fact]
    public void RejectsMultiplicationBelowZero()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Weight.FromKg(60m) * -1m);
    }
}
