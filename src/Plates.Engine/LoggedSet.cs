namespace Plates.Engine;

/// <summary>One logged set: the weight lifted and the reps done.</summary>
public readonly struct LoggedSet(Weight weight, int reps)
{
    public Weight Weight { get; } = weight;

    public int Reps { get; } = reps;

    public void Deconstruct(out Weight weight, out int reps)
    {
        weight = Weight;
        reps = Reps;
    }
}
