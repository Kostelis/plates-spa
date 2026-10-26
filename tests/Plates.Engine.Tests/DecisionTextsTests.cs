namespace Plates.Engine.Tests;

public class DecisionTextsTests
{
    [Theory]
    [InlineData(DecisionCode.Increase, "Visose serijose pasiektas pakartojimų maksimumas – svoris didinamas.")]
    [InlineData(DecisionCode.Limit, "Pasiekta 500 kg riba – automatinė progresija sustabdyta, sprendžia treneris.")]
    [InlineData(DecisionCode.Hold, "Ne visose serijose pasiektas maksimumas – svoris nekeičiamas.")]
    [InlineData(DecisionCode.Failure, "Ne visose serijose pasiektas minimumas – svoris nekeičiamas.")]
    [InlineData(DecisionCode.Deload, "Pasiekta nesėkmių riba – svoris mažinamas.")]
    public void GivesTheAgreedLithuanianReason(DecisionCode code, string reason)
    {
        Assert.Equal(reason, DecisionTexts.For(code));
    }

    [Fact]
    public void EveryDecisionCodeHasAReason()
    {
        Assert.All(Enum.GetValues<DecisionCode>(), code => Assert.NotEmpty(DecisionTexts.For(code)));
    }
}
