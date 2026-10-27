using static Plates.Engine.Tests.DoubleProgression;

namespace Plates.Engine.Tests;

public class WorkoutValidationTests
{
    private static readonly WorkoutValidator Validator = new(NewEngine().PlannedSets, new FixedTimeProvider(Now));

    [Fact]
    public void Scenario5_A1000KgSetIsRejected()
    {
        var valid = Validator.TryValidate(Workout(Log(Set(60m, 8), Set(1000m, 8), Set(60m, 8))), out var errors);

        Assert.False(valid);
        var error = Assert.Single(errors);
        Assert.Equal(new ValidationError(ExerciseId, 2, "2-osios serijos svoris 1000 kg viršija 500 kg ribą"), error);
    }

    [Fact]
    public void AValidWorkoutHasNoErrors()
    {
        // Boundaries of rule 1: 1 and 50 reps, 0 and 500 kg, 0,5 kg steps.
        var valid = Validator.TryValidate(Workout(Log(Set(0m, 1), Set(500m, 50), Set(62.5m, 8))), out var errors);

        Assert.True(valid);
        Assert.Empty(errors);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(51)]
    public void RepsOutside1To50AreRejected(int reps)
    {
        var valid = Validator.TryValidate(Workout(Log(Set(60m, 8), Set(60m, reps))), out var errors);

        Assert.False(valid);
        Assert.Equal(new ValidationError(ExerciseId, 2, $"2-osios serijos pakartojimų skaičius {reps} turi būti nuo 1 iki 50"), Assert.Single(errors));
    }

    [Fact]
    public void AWeightOffThe05KgStepIsRejected()
    {
        var valid = Validator.TryValidate(Workout(Log(Set(60.3m, 8))), out var errors);

        Assert.False(valid);
        Assert.Equal(new ValidationError(ExerciseId, 1, "1-osios serijos svoris 60,3 kg turi būti nurodytas 0,5 kg tikslumu"), Assert.Single(errors));
    }

    [Fact]
    public void TheThirdSetIsWrittenAs3Iosios()
    {
        Validator.TryValidate(Workout(Log(Set(60m, 8), Set(60m, 8), Set(1000m, 8))), out var errors);

        Assert.Equal("3-iosios serijos svoris 1000 kg viršija 500 kg ribą", Assert.Single(errors).Message);
    }

    [Fact]
    public void AnExerciseNotInTheProgramIsRejected()
    {
        var otherExercise = Guid.NewGuid();

        var valid = Validator.TryValidate(Workout(ExerciseLog.Of(otherExercise, Set(60m, 8))), out var errors);

        Assert.False(valid);
        Assert.Equal(new ValidationError(otherExercise, null, "Pratimo nėra programoje"), Assert.Single(errors));
    }

    [Fact]
    public void MoreSetsThanPlannedAreRejected()
    {
        var valid = Validator.TryValidate(Workout(Log(Set(60m, 8), Set(60m, 8), Set(60m, 8), Set(60m, 8))), out var errors);

        Assert.False(valid);
        Assert.Equal(new ValidationError(ExerciseId, null, "Pratime atlikta 4 serijos, programoje numatytos 3"), Assert.Single(errors));
    }

    [Fact]
    public void TheSetCountMessageAgreesWithTheNumber()
    {
        var tenSets = Exercise(new DoubleProgressionParameters(
            sets: 10, minReps: 5, maxReps: 8, startingWeight: Weight.FromKg(60m), increment: Weight.FromKg(5m)));
        var elevenSets = Enumerable.Repeat(Set(60m, 8), 11).ToArray();

        Validator.TryValidate(Workout(tenSets, Log(elevenSets)), out var errors);

        Assert.Equal("Pratime atlikta 11 serijų, programoje numatyta 10", Assert.Single(errors).Message);
    }

    [Fact]
    public void AnExerciseEntryWithoutSetsIsRejected()
    {
        // A skipped exercise is left out of the workout instead (clarified rule 9).
        var valid = Validator.TryValidate(Workout(Log()), out var errors);

        Assert.False(valid);
        Assert.Equal(new ValidationError(ExerciseId, null, "Pratime neatlikta nė viena serija"), Assert.Single(errors));
    }

    [Fact]
    public void AnEmptyEntryForAnExerciseNotInTheProgramReportsBothErrors()
    {
        var otherExercise = Guid.NewGuid();

        Validator.TryValidate(Workout(ExerciseLog.Of(otherExercise)), out var errors);

        Assert.Equal(
            [
                new ValidationError(otherExercise, null, "Pratimo nėra programoje"),
                new ValidationError(otherExercise, null, "Pratime neatlikta nė viena serija"),
            ],
            errors);
    }

    [Fact]
    public void TheSameExerciseLoggedTwiceIsRejected()
    {
        var valid = Validator.TryValidate(Workout(Log(Set(60m, 8)), Log(Set(60m, 8))), out var errors);

        Assert.False(valid);
        Assert.Equal(new ValidationError(ExerciseId, null, "Pratimas treniruotėje įrašytas kelis kartus"), Assert.Single(errors));
    }

    [Fact]
    public void AFutureDateIsRejected()
    {
        var valid = Validator.TryValidate(Workout(Now.AddDays(1), Log(Set(60m, 8))), out var errors);

        Assert.False(valid);
        Assert.Equal(new ValidationError(null, null, "Treniruotės data negali būti ateityje"), Assert.Single(errors));
    }

    [Fact]
    public void ALaterHourOfTheSameDayIsNotInTheFuture()
    {
        // The date counts, not the instant, in the client's own offset: a small clock skew is no error.
        var laterToday = new DateTimeOffset(2026, 10, 27, 23, 30, 0, TimeSpan.FromHours(2));

        Assert.True(Validator.TryValidate(Workout(laterToday, Log(Set(60m, 8))), out _));
    }

    [Fact]
    public void EveryErrorIsReturnedTogether()
    {
        var otherExercise = Guid.NewGuid();

        var valid = Validator.TryValidate(
            Workout(Now.AddDays(1), Log(Set(1000m, 0), Set(60.3m, 8)), ExerciseLog.Of(otherExercise, Set(60m, 8))),
            out var errors);

        Assert.False(valid);
        Assert.Equal(
            [
                new ValidationError(null, null, "Treniruotės data negali būti ateityje"),
                new ValidationError(ExerciseId, 1, "1-osios serijos pakartojimų skaičius 0 turi būti nuo 1 iki 50"),
                new ValidationError(ExerciseId, 1, "1-osios serijos svoris 1000 kg viršija 500 kg ribą"),
                new ValidationError(ExerciseId, 2, "2-osios serijos svoris 60,3 kg turi būti nurodytas 0,5 kg tikslumu"),
                new ValidationError(otherExercise, null, "Pratimo nėra programoje"),
            ],
            errors);
    }
}
