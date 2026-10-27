using System.Globalization;

namespace Plates.Engine;

/// <summary>
/// Rule 1: checks a logged workout as a whole before any scheme runs and collects every error.
/// The planned set count comes from each exercise's scheme, so the validator knows no scheme.
/// </summary>
public sealed class WorkoutValidator(Func<ProgramExercise, int> plannedSets, TimeProvider time)
{
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

            var inProgram = programIds.Contains(log.ExerciseId);
            if (!inProgram)
            {
                found.Add(new ValidationError(log.ExerciseId, null, "Pratimo nėra programoje"));
            }

            var exerciseError = log switch
            {
                { Sets.Count: 0 } => "Pratime neatlikta nė viena serija",
                _ when inProgram && PlannedSets(input, log.ExerciseId) is var planned && log.Sets.Count > planned =>
                    $"Pratime atlikta {log.Sets.Count} {SetCount(log.Sets.Count).Noun}, programoje {SetCount(planned).Planned} {planned}",
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
            var setName = SetName(number);

            if (set is { Reps: < SetLimits.MinReps or > SetLimits.MaxReps })
            {
                errors.Add(new ValidationError(log.ExerciseId, number, $"{setName} pakartojimų skaičius {set.Reps} turi būti nuo {SetLimits.MinReps} iki {SetLimits.MaxReps}"));
            }

            if (set is { Weight.Kg: > SetLimits.MaxKg })
            {
                errors.Add(new ValidationError(log.ExerciseId, number, string.Create(Lithuanian, $"{setName} svoris {set.Weight} viršija {SetLimits.MaxWeight} ribą")));
            }
            else if (set.Weight.Kg % SetLimits.WeightStepKg is not 0m)
            {
                errors.Add(new ValidationError(log.ExerciseId, number, string.Create(Lithuanian, $"{setName} svoris {set.Weight} turi būti nurodytas {SetLimits.WeightStepKg} kg tikslumu")));
            }
        }
    }

    /// <summary>The ordinal agrees with the number: 1-osios, 3-iosios, 13-osios serijos.</summary>
    private static string SetName(int number) => (number % 10, number % 100) switch
    {
        (3, not 13) => $"{number}-iosios serijos",
        _ => $"{number}-osios serijos",
    };

    /// <summary>The words agree with the number: 1 serija / numatyta, 4 serijos / numatytos, 11 serijų / numatyta.</summary>
    private static (string Noun, string Planned) SetCount(int count) => (count % 10, count % 100) switch
    {
        (1, not 11) => ("serija", "numatyta"),
        (>= 2 and <= 9, not (>= 12 and <= 19)) => ("serijos", "numatytos"),
        _ => ("serijų", "numatyta"),
    };

    private int PlannedSets(WorkoutInput input, Guid exerciseId) =>
        plannedSets(input.Program.First(exercise => exercise.ExerciseId == exerciseId));

    /// <summary>Compares calendar dates in the client's own offset, so a few seconds of clock skew are no error.</summary>
    private bool IsInTheFuture(DateTimeOffset date) => date.Date > time.GetUtcNow().ToOffset(date.Offset).Date;
}
