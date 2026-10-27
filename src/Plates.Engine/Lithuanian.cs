using System.Globalization;

namespace Plates.Engine;

/// <summary>The culture every user-facing text is formatted with, so validation errors and decision reasons write numbers alike („60,3 kg“).</summary>
internal static class Lithuanian
{
    public static readonly CultureInfo Culture = CultureInfo.GetCultureInfo("lt-LT");
}
