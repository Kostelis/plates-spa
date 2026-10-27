using static Plates.Engine.Tests.DoubleProgression;

namespace Plates.Engine.Tests;

public class FailureDeloadLimitTests
{
    [Fact]
    public void Scenario3_TargetAbove500KgIsHeldAtTheLimit()
    {
        var outcome = NewEngine().Progress(Squat(), State(recordKg: 600.0m), Log(Set(500m, 8), Set(500m, 8), Set(500m, 8)));

        Assert.Equal(Weight.FromKg(500m), outcome.Target);
        Assert.Equal(DecisionCode.Limit, outcome.Code);
        Assert.Equal("Pasiekta 500 kg riba – automatinė progresija sustabdyta, sprendžia treneris.", outcome.Reason);
        Assert.Equal(0, outcome.FailureCount);
        Assert.Equal(Weight.FromKg(633.3m), outcome.Record);
    }

    [Fact]
    public void Scenario4_ReachingTheFailureLimitDeloads()
    {
        var outcome = NewEngine().Progress(Bench(), State(recordKg: 60.0m, failureCount: 1), Log(Set(47.5m, 7), Set(47.5m, 5), Set(47.5m, 4)));

        Assert.Equal(Weight.FromKg(42.5m), outcome.Target);
        Assert.Equal(DecisionCode.Deload, outcome.Code);
        Assert.Equal("Pasiekta nesėkmių riba – svoris mažinamas.", outcome.Reason);
        Assert.Equal(0, outcome.FailureCount);
        Assert.Equal(Weight.FromKg(60.0m), outcome.Record);
    }

    [Fact]
    public void AFirstFailureKeepsTheBaseWeight()
    {
        var outcome = NewEngine().Progress(Squat(), State(recordKg: 76.0m), Log(Set(60m, 8), Set(60m, 8), Set(60m, 4)));

        Assert.Equal(Weight.FromKg(60m), outcome.Target);
        Assert.Equal(DecisionCode.Failure, outcome.Code);
        Assert.Equal("Ne visose serijose pasiektas minimumas – svoris nekeičiamas.", outcome.Reason);
        Assert.Equal(1, outcome.FailureCount);
        Assert.Equal(Weight.FromKg(76.0m), outcome.Record);
    }

    [Fact]
    public void FewerSetsThanPlannedIsAFailure()
    {
        // Both sets reach the maximum, but the program plans 3.
        var outcome = NewEngine().Progress(Squat(), State(recordKg: null), Log(Set(60m, 8), Set(60m, 8)));

        Assert.Equal(Weight.FromKg(60m), outcome.Target);
        Assert.Equal(DecisionCode.Failure, outcome.Code);
        Assert.Equal(1, outcome.FailureCount);
    }

    [Fact]
    public void BaseWeight498KgPlusIncrementIsCappedAt500Kg()
    {
        var outcome = NewEngine().Progress(Squat(), State(recordKg: null), Log(Set(498m, 8), Set(498m, 8), Set(498m, 8)));

        Assert.Equal(Weight.FromKg(500m), outcome.Target);
        Assert.Equal(DecisionCode.Limit, outcome.Code);
    }

    [Fact]
    public void LimitResetsTheFailureCounter()
    {
        var outcome = NewEngine().Progress(Squat(), State(recordKg: null, failureCount: 1), Log(Set(500m, 8), Set(500m, 8), Set(500m, 8)));

        Assert.Equal(0, outcome.FailureCount);
    }

    [Fact]
    public void TheFailureCounterIsCarriedToTheNextWorkout()
    {
        var engine = NewEngine();
        var failed = Log(Set(60m, 8), Set(60m, 8), Set(60m, 4));

        var first = engine.Progress(Squat(), State(recordKg: null), failed);
        var second = engine.Progress(Squat(), new ExerciseState(first.Record, first.SchemeState), failed);

        // 60 − 6 = 54 → 52,5.
        Assert.Equal(Weight.FromKg(52.5m), second.Target);
        Assert.Equal(DecisionCode.Deload, second.Code);
        Assert.Equal(0, second.FailureCount);
    }

    [Fact]
    public void BelowAHigherFailureLimitTheCounterKeepsGrowing()
    {
        var exercise = Exercise(new DoubleProgressionParameters(
            sets: 3, minReps: 5, maxReps: 8, startingWeight: Weight.FromKg(60m), increment: Weight.FromKg(5m), failureLimit: 3));

        var outcome = NewEngine().Progress(exercise, State(recordKg: null, failureCount: 1), Log(Set(60m, 8), Set(60m, 8), Set(60m, 4)));

        Assert.Equal(DecisionCode.Failure, outcome.Code);
        Assert.Equal(2, outcome.FailureCount);
    }

    [Fact]
    public void DeloadUsesTheProgramsDeloadPercentage()
    {
        var exercise = Exercise(new DoubleProgressionParameters(
            sets: 3, minReps: 5, maxReps: 8, startingWeight: Weight.FromKg(60m), increment: Weight.FromKg(5m), deloadPercentage: 0.20m));

        var outcome = NewEngine().Progress(exercise, State(recordKg: null, failureCount: 1), Log(Set(60m, 8), Set(60m, 8), Set(60m, 4)));

        // 60 − 12 = 48 → 47,5.
        Assert.Equal(Weight.FromKg(47.5m), outcome.Target);
        Assert.Equal(DecisionCode.Deload, outcome.Code);
    }
}
