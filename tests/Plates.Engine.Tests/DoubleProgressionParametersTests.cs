using System.Text.Json;

namespace Plates.Engine.Tests;

public class DoubleProgressionParametersTests
{
    [Fact]
    public void DefaultsDeloadPercentageAndFailureLimit()
    {
        var parameters = new DoubleProgressionParameters(
            sets: 3, minReps: 5, maxReps: 8, startingWeight: Weight.FromKg(60m), increment: Weight.FromKg(5m));

        Assert.Equal(0.10m, parameters.DeloadPercentage);
        Assert.Equal(2, parameters.FailureLimit);
    }

    [Fact]
    public void JsonWithoutDeloadPercentageAndFailureLimitUsesTheDefaults()
    {
        // Without the defaults both would bind as 0 and the constructor would throw.
        var engine = DoubleProgression.NewEngine();
        var json = JsonDocument.Parse(
            """{ "sets": 3, "minReps": 5, "maxReps": 8, "startingWeight": 60, "increment": 5 }""").RootElement;

        Assert.Equal(3, engine.PlannedSets(new ProgramExercise(DoubleProgression.ExerciseId, DoubleProgressionScheme.SchemeKey, json)));
    }

    [Theory]
    [InlineData(3, 5, 8, 2.5, 0.10, 2)]
    [InlineData(1, 1, 1, 7.5, 0.5, 1)]
    [InlineData(5, 50, 50, 10, 0.99, 3)]
    public void AcceptsValidParameters(int sets, int minReps, int maxReps, decimal incrementKg, decimal deloadPercentage, int failureLimit)
    {
        var parameters = new DoubleProgressionParameters(
            sets: sets, minReps: minReps, maxReps: maxReps, startingWeight: Weight.FromKg(60m), increment: Weight.FromKg(incrementKg),
            deloadPercentage: deloadPercentage, failureLimit: failureLimit);

        Assert.Equal(sets, parameters.Sets);
    }

    [Theory]
    [InlineData(3, 5, 8, 2, 0.10, 2)] // Increment not a multiple of 2,5 kg
    [InlineData(3, 5, 8, 0, 0.10, 2)] // Increment 0
    [InlineData(0, 5, 8, 5, 0.10, 2)] // no sets
    [InlineData(3, 0, 8, 5, 0.10, 2)] // min below 1
    [InlineData(3, 9, 8, 5, 0.10, 2)] // min above max
    [InlineData(3, 5, 51, 5, 0.10, 2)] // max above 50
    [InlineData(3, 5, 8, 5, 0, 2)] // Deload percentage 0
    [InlineData(3, 5, 8, 5, 1, 2)] // Deload percentage 1
    [InlineData(3, 5, 8, 5, 0.10, 0)] // Failure limit below 1
    public void RejectsInvalidParameters(int sets, int minReps, int maxReps, decimal incrementKg, decimal deloadPercentage, int failureLimit)
    {
        Assert.ThrowsAny<ArgumentException>(() => new DoubleProgressionParameters(
            sets: sets, minReps: minReps, maxReps: maxReps, startingWeight: Weight.FromKg(60m), increment: Weight.FromKg(incrementKg),
            deloadPercentage: deloadPercentage, failureLimit: failureLimit));
    }
}
