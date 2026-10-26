namespace Plates.Engine.Tests;

public class WeightEqualityTests
{
    [Fact]
    public void EqualsSameKgWrittenWithTrailingZero()
    {
        var sixty = Weight.FromKg(60m);
        var sixtyPointZero = Weight.FromKg(60.0m);

        Assert.True(sixty.Equals(sixtyPointZero));
        Assert.True(sixty == sixtyPointZero);
        Assert.False(sixty != sixtyPointZero);
        Assert.Equal(sixty.GetHashCode(), sixtyPointZero.GetHashCode());
    }

    [Fact]
    public void DiffersFromOtherKg()
    {
        var sixty = Weight.FromKg(60m);
        var sixtyTwoAndHalf = Weight.FromKg(62.5m);

        Assert.False(sixty.Equals(sixtyTwoAndHalf));
        Assert.False(sixty == sixtyTwoAndHalf);
        Assert.True(sixty != sixtyTwoAndHalf);
    }

    [Fact]
    public void EqualsBoxedWeightOnly()
    {
        var sixty = Weight.FromKg(60m);

        Assert.True(sixty.Equals((object)Weight.FromKg(60.0m)));
        Assert.False(sixty.Equals((object)60m));
        Assert.False(sixty.Equals(null));
    }

    [Fact]
    public void FindsEqualWeightInHashSet()
    {
        var weights = new HashSet<Weight> { Weight.FromKg(60m) };

        Assert.Contains(Weight.FromKg(60.00m), weights);
    }
}
