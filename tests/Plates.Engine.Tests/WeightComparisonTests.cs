namespace Plates.Engine.Tests;

public class WeightComparisonTests
{
    private static readonly Weight Sixty = Weight.FromKg(60m);
    private static readonly Weight SixtyTwoAndHalf = Weight.FromKg(62.5m);

    [Fact]
    public void ComparesByKg()
    {
        Assert.True(Sixty.CompareTo(SixtyTwoAndHalf) < 0);
        Assert.True(SixtyTwoAndHalf.CompareTo(Sixty) > 0);
        Assert.Equal(0, Sixty.CompareTo(Weight.FromKg(60.0m)));
    }

    [Fact]
    public void RelationalOperatorsFollowKg()
    {
        Assert.True(Sixty < SixtyTwoAndHalf);
        Assert.False(SixtyTwoAndHalf < Sixty);
        Assert.True(SixtyTwoAndHalf > Sixty);
        Assert.False(Sixty > SixtyTwoAndHalf);
        Assert.True(Sixty <= Weight.FromKg(60.0m));
        Assert.True(Sixty >= Weight.FromKg(60.0m));
        Assert.False(SixtyTwoAndHalf <= Sixty);
        Assert.False(Sixty >= SixtyTwoAndHalf);
    }

    [Fact]
    public void TargetAboveLimitComparesGreaterThan500Kg()
    {
        var limit = Weight.FromKg(500m);

        Assert.True(Weight.FromKg(503m) > limit);
        Assert.False(Weight.FromKg(500.0m) > limit);
    }

    [Fact]
    public void SortsAndFindsHigherRecord()
    {
        var records = new List<Weight> { Weight.FromKg(74m), Weight.FromKg(58.6m), Weight.FromKg(76m) };

        records.Sort();

        Assert.Equal([Weight.FromKg(58.6m), Weight.FromKg(74m), Weight.FromKg(76m)], records);
        Assert.Equal(Weight.FromKg(76m), records.Max());
    }
}
