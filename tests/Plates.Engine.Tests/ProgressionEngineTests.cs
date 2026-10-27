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
    public void RejectsALogForAnotherExercise()
    {
        var otherExerciseLog = ExerciseLog.Of(Guid.NewGuid(), Set(60m, 8), Set(60m, 8), Set(60m, 8));

        Assert.Throws<ArgumentException>(() => NewEngine().Progress(Squat(), State(recordKg: null), otherExerciseLog));
    }

    [Fact]
    public void RejectsALogWithoutSets()
    {
        // A skipped exercise is left out of the workout; it never reaches the engine.
        Assert.Throws<ArgumentException>(() => NewEngine().Progress(Squat(), State(recordKg: null), Log()));
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

    [Fact]
    public void Scenario5_AnInvalidWorkoutReturnsErrorsAndNoOutcomes()
    {
        var states = new Dictionary<Guid, ExerciseState> { [ExerciseId] = State(recordKg: null) };

        var result = NewEngine().Progress(Workout(Log(Set(60m, 8), Set(1000m, 8), Set(60m, 8))), states);

        Assert.Equal("2-osios serijos svoris 1000 kg viršija 500 kg ribą", Assert.Single(result.Errors).Message);
        Assert.Empty(result.Outcomes);
    }

    [Fact]
    public void AValidWorkoutDecidesEveryLoggedExerciseAndSkipsTheRest()
    {
        var benchId = Guid.NewGuid();
        var input = new WorkoutInput([Squat(), Bench() with { ExerciseId = benchId }], Now, [Log(Set(60m, 8), Set(60m, 8), Set(60m, 8))]);
        var states = new Dictionary<Guid, ExerciseState> { [ExerciseId] = State(recordKg: null), [benchId] = State(recordKg: null) };

        var result = NewEngine().Progress(input, states);

        Assert.Empty(result.Errors);
        var (exerciseId, outcome) = Assert.Single(result.Outcomes);
        Assert.Equal(ExerciseId, exerciseId);
        Assert.Equal(DecisionCode.Increase, outcome.Code);
    }
}
