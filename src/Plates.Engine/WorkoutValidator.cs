using System.Globalization;

namespace Plates.Engine;

/// <summary>
/// Rule 1: checks a logged workout as a whole before any scheme runs and collects every error.
/// The planned set count comes from each exercise's scheme, so the validator knows no scheme.
/// </summary>
public sealed class WorkoutValidator(Func<ProgramExercise, int> plannedSets, TimeProvider time)
{
    public const int MaxReps = 50;

    private const int MinReps = 1;
    private const decimal WeightStepKg = 0.5m;

    private static readonly CultureInfo Lithuanian = CultureInfo.GetCultureInfo("lt-LT");

    public bool TryValidate(WorkoutInput input, out IReadOnlyList<ValidationError> errors)
    {
        var found = new List<ValidationError>();
        if (IsInTheFuture(input.Date))
        {
            found.Add(new ValidationError(null, null, "Treniruotės data negali būti ateityje"));
        }

        var programIds = input.Program.Select(exercise => exercise.ExerciseId).ToHashSet();
        var loggedIds = new HashSet<Guid>();
        foreach (var log in input.Exercises)
        {
            if (!loggedIds.Add(log.ExerciseId))
            {
                found.Add(new ValidationError(log.ExerciseId, null, "Pratimas treniruotėje įrašytas kelis kartus"));
                continue;
            }

            ValidateSets(log, found);

            var exerciseError = log switch
            {
                { Sets.Count: 0 } => "Pratime neatlikta nė viena serija",
                _ when !programIds.Contains(log.ExerciseId) => "Pratimo nėra programoje",
                _ when PlannedSets(input, log.ExerciseId) is var planned && log.Sets.Count > planned =>
                    $"Pratime atlikta {log.Sets.Count} {SetsNoun(log.Sets.Count)}, programoje numatyt{(SetsNoun(planned) == "serijos" ? "os" : "a")} {planned}",
                _ => null,
            };

            if (exerciseError is not null)
            {
                found.Add(new ValidationError(log.ExerciseId, null, exerciseError));
            }
        }

        errors = found;
        return found.Count == 0;
    }

    private static void ValidateSets(ExerciseLog log, List<ValidationError> errors)
    {
        for (var number = 1; number <= log.Sets.Count; number++)
        {
            var set = log.Sets[number - 1];
            var setName = $"{number}-{(number % 10, number % 100) switch { (3, not 13) => "iosios", _ => "osios" }} serijos";

            if (set is { Reps: < MinReps or > MaxReps })
            {
                errors.Add(new ValidationError(log.ExerciseId, number, $"{setName} pakartojimų skaičius {set.Reps} turi būti nuo {MinReps} iki {MaxReps}"));
            }

            if (set is { Weight.Kg: > LoadLimit.MaxKg })
            {
                errors.Add(new ValidationError(log.ExerciseId, number, string.Create(Lithuanian, $"{setName} svoris {set.Weight} viršija {LoadLimit.Max} ribą")));
            }
            else if (set.Weight.Kg % WeightStepKg is not 0m)
            {
                errors.Add(new ValidationError(log.ExerciseId, number, string.Create(Lithuanian, $"{setName} svoris {set.Weight} turi būti nurodytas {WeightStepKg} kg tikslumu")));
            }
        }
    }

    /// <summary>The noun agrees with the number: 1 serija, 4 serijos, 11 serijų.</summary>
    private static string SetsNoun(int count) => (count % 10, count % 100) switch
    {
        (1, not 11) => "serija",
        (>= 2 and <= 9, not (>= 12 and <= 19)) => "serijos",
        _ => "serijų",
    };

    private int PlannedSets(WorkoutInput input, Guid exerciseId) =>
        plannedSets(input.Program.First(exercise => exercise.ExerciseId == exerciseId));

    /// <summary>Compares calendar dates in the client's own offset, so a few seconds of clock skew are no error.</summary>
    private bool IsInTheFuture(DateTimeOffset date) => date.Date > time.GetUtcNow().ToOffset(date.Offset).Date;
}
