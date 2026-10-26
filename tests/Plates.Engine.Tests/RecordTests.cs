using static Plates.Engine.Tests.DoubleProgression;

namespace Plates.Engine.Tests;

public class RecordTests
{
    private static ProgramExercise Exercise(int minReps, int maxReps) => DoubleProgression.Exercise(new DoubleProgressionParameters(
        sets: 3, minReps: minReps, maxReps: maxReps, startingWeight: Weight.FromKg(40m), increment: Weight.FromKg(2.5m)));

    [Fact]
    public void WithoutARecordTheEstimateBecomesTheRecord()
    {
        var outcome = NewEngine().Progress(Squat(), State(recordKg: null), Log(Set(60m, 8), Set(60m, 8), Set(60m, 6)));

        Assert.Equal(Weight.FromKg(76.0m), outcome.Record);
    }

    [Fact]
    public void SetsAbove12RepsAreSkipped()
    {
        // 50 × 15 would estimate 75,0; only the 12-rep sets count: 50 × 12 → 70,0.
        var outcome = NewEngine().Progress(Exercise(minReps: 8, maxReps: 15), State(recordKg: null), Log(Set(50m, 15), Set(50m, 12), Set(50m, 12)));

        Assert.Equal(Weight.FromKg(70.0m), outcome.Record);
    }

    [Fact]
    public void WithoutAQualifyingSetTheRecordIsUnchanged()
    {
        var outcome = NewEngine().Progress(Exercise(minReps: 8, maxReps: 15), State(recordKg: 50.0m), Log(Set(50m, 13), Set(50m, 15), Set(50m, 15)));

        Assert.Equal(Weight.FromKg(50.0m), outcome.Record);
    }

    [Fact]
    public void WithoutAQualifyingSetAndNoRecordThereIsStillNoRecord()
    {
        var outcome = NewEngine().Progress(Exercise(minReps: 8, maxReps: 15), State(recordKg: null), Log(Set(50m, 13), Set(50m, 15), Set(50m, 15)));

        Assert.Null(outcome.Record);
    }

    [Fact]
    public void ASingleRepSetCountsAsItsWeight()
    {
        // Epley would give 103,3.
        var outcome = NewEngine().Progress(Exercise(minReps: 1, maxReps: 3), State(recordKg: null), Log(Set(100m, 1), Set(100m, 1), Set(100m, 1)));

        Assert.Equal(Weight.FromKg(100.0m), outcome.Record);
    }

    [Fact]
    public void ALowerEstimateKeepsTheRecord()
    {
        var outcome = NewEngine().Progress(Squat(), State(recordKg: 80.0m), Log(Set(60m, 8), Set(60m, 8), Set(60m, 8)));

        Assert.Equal(Weight.FromKg(80.0m), outcome.Record);
    }

    [Theory]
    [InlineData(47.5, 7, 58.6)] // 58,583 → up
    [InlineData(62.5, 5, 72.9)] // 72,917 → down
    [InlineData(65.5, 3, 72.1)] // 72,05 → midpoint away from zero
    public void RecordIsRoundedToTheNearestTenthKg(decimal kg, int reps, decimal recordKg)
    {
        var outcome = NewEngine().Progress(Exercise(minReps: 3, maxReps: 10), State(recordKg: null), Log(Set(kg, reps), Set(kg, reps), Set(kg, reps)));

        Assert.Equal(Weight.FromKg(recordKg), outcome.Record);
    }

    [Fact]
    public void UsesAnInjectedOneRepMaxFormula()
    {
        var engine = new ProgressionEngine([new DoubleProgressionScheme(new RecordCalculator((weightKg, reps) => weightKg + reps))]);

        var outcome = engine.Progress(Squat(), State(recordKg: null), Log(Set(60m, 8), Set(60m, 8), Set(60m, 6)));

        Assert.Equal(Weight.FromKg(68.0m), outcome.Record);
    }
}
