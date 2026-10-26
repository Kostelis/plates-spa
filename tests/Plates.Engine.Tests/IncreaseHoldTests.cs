using static Plates.Engine.Tests.DoubleProgression;

namespace Plates.Engine.Tests;

public class IncreaseHoldTests
{
    [Fact]
    public void Scenario1_AllSetsAtMaxIncreasesByIncrement()
    {
        var outcome = NewEngine().Progress(Squat(), State(recordKg: 72.0m), Log(Set(60m, 8), Set(60m, 8), Set(60m, 8)));

        Assert.Equal(Weight.FromKg(65m), outcome.Target);
        Assert.Equal(DecisionCode.Increase, outcome.Code);
        Assert.Equal("Visose serijose pasiektas pakartojimų maksimumas – svoris didinamas.", outcome.Reason);
        Assert.Equal(0, outcome.FailureCount);
        Assert.Equal(Weight.FromKg(76.0m), outcome.Record);
    }

    [Fact]
    public void Scenario2_MinButNotMaxInEverySetHoldsBaseWeight()
    {
        var outcome = NewEngine().Progress(Squat(), State(recordKg: 76.0m), Log(Set(60m, 8), Set(60m, 8), Set(60m, 6)));

        Assert.Equal(Weight.FromKg(60m), outcome.Target);
        Assert.Equal(DecisionCode.Hold, outcome.Code);
        Assert.Equal("Ne visose serijose pasiektas maksimumas – svoris nekeičiamas.", outcome.Reason);
        Assert.Equal(0, outcome.FailureCount);
        Assert.Equal(Weight.FromKg(76.0m), outcome.Record);
    }

    [Fact]
    public void HoldTargetIsRoundedDownTo2_5Kg()
    {
        var outcome = NewEngine().Progress(Squat(), State(recordKg: null), Log(Set(61m, 8), Set(61m, 8), Set(61m, 6)));

        Assert.Equal(Weight.FromKg(60m), outcome.Target);
        Assert.Equal(DecisionCode.Hold, outcome.Code);
    }

    [Fact]
    public void BaseWeightIsTheLowestLoggedWeight()
    {
        var outcome = NewEngine().Progress(Squat(), State(recordKg: null), Log(Set(62.5m, 8), Set(60m, 8), Set(62.5m, 8)));

        Assert.Equal(Weight.FromKg(65m), outcome.Target);
        Assert.Equal(DecisionCode.Increase, outcome.Code);
    }

    [Theory]
    [InlineData(8, 8, 8)]
    [InlineData(8, 8, 6)]
    public void IncreaseAndHoldResetTheFailureCounter(int firstReps, int secondReps, int thirdReps)
    {
        var outcome = NewEngine().Progress(
            Squat(), State(recordKg: null, failureCount: 1), Log(Set(60m, firstReps), Set(60m, secondReps), Set(60m, thirdReps)));

        Assert.Equal(0, outcome.FailureCount);
    }
}
