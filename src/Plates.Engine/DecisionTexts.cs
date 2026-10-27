using System.Globalization;

namespace Plates.Engine;

/// <summary>The Lithuanian reason shown with each decision code.</summary>
public static class DecisionTexts
{
    private static readonly Dictionary<DecisionCode, string> Texts;

    static DecisionTexts()
    {
        Texts = new Dictionary<DecisionCode, string>
        {
            [DecisionCode.Increase] = "Visose serijose pasiektas pakartojimų maksimumas – svoris didinamas.",
            [DecisionCode.Limit] = string.Create(CultureInfo.GetCultureInfo("lt-LT"), $"Pasiekta {SetLimits.MaxWeight} riba – automatinė progresija sustabdyta, sprendžia treneris."),
            [DecisionCode.Hold] = "Ne visose serijose pasiektas maksimumas – svoris nekeičiamas.",
            [DecisionCode.Failure] = "Ne visose serijose pasiektas minimumas – svoris nekeičiamas.",
            [DecisionCode.Deload] = "Pasiekta nesėkmių riba – svoris mažinamas.",
        };

        foreach (var code in Enum.GetValues<DecisionCode>())
        {
            if (!Texts.ContainsKey(code))
            {
                throw new InvalidOperationException($"Decision code {code} has no reason text.");
            }
        }
    }

    public static string For(DecisionCode code) => Texts[code];
}
