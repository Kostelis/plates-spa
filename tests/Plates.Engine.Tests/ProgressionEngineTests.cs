using static Plates.Engine.Tests.DoubleProgression;

namespace Plates.Engine.Tests;

public class ProgressionEngineTests
{
    [Fact]
    public void RejectsAnUnknownSchemeKey()
    {
        var exercise = Squat() with { SchemeKey = "top-set-back-off" };

        var error = Assert.Throws<ArgumentException>(() => NewEngine().Progress(exercise, State(recordKg: null), Log(Set(60m, 8))));
        Assert.Contains("top-set-back-off", error.Message);
    }

    [Fact]
    public void ReturnsTheNewSchemeState()
    {
        var outcome = NewEngine().Progress(Squat(), State(recordKg: null, failureCount: 1), Log(Set(60m, 8), Set(60m, 8), Set(60m, 8)));

        Assert.Equal(0, outcome.SchemeState.GetProperty("failureCount").GetInt32());
    }

    [Fact]
    public void AFreshStateStartsWithNoFailures()
    {
        var outcome = NewEngine().Progress(Squat(), new ExerciseState(Record: null, SchemeState: null), Log(Set(60m, 8), Set(60m, 8), Set(60m, 6)));

        Assert.Equal(DecisionCode.Hold, outcome.Code);
        Assert.Equal(0, outcome.FailureCount);
    }

    [Fact]
    public void ReadsThePlannedSetCountFromTheScheme()
    {
        Assert.Equal(3, NewEngine().PlannedSets(Squat()));
    }
}
