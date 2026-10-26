namespace Plates.Engine.Tests;

public class LoggedSetTests
{
    [Fact]
    public void DeconstructsIntoWeightAndReps()
    {
        var (weight, reps) = new LoggedSet(Weight.FromKg(62.5m), 8);

        Assert.Equal(Weight.FromKg(62.5m), weight);
        Assert.Equal(8, reps);
    }

    [Fact]
    public void ExerciseLogKeepsSetsInOrder()
    {
        var exerciseId = Guid.NewGuid();

        var log = ExerciseLog.Of(exerciseId, new LoggedSet(Weight.FromKg(60m), 8), new LoggedSet(Weight.FromKg(60m), 6));

        Assert.Equal(exerciseId, log.ExerciseId);
        Assert.Equal([8, 6], log.Sets.Select(set => set.Reps));
    }
}
