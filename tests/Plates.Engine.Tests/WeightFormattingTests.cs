using System.Globalization;

namespace Plates.Engine.Tests;

public class WeightFormattingTests
{
    private static readonly CultureInfo Lithuanian = CultureInfo.GetCultureInfo("lt-LT");
    private static readonly CultureInfo English = CultureInfo.GetCultureInfo("en-US");

    [Fact]
    public void FormatsLithuanianDecimalComma()
    {
        Assert.Equal("62,5 kg", Weight.FromKg(62.5m).ToString("G", Lithuanian));
    }

    [Fact]
    public void FormatsEnglishDecimalPoint()
    {
        Assert.Equal("62.5 kg", Weight.FromKg(62.5m).ToString("G", English));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("g")]
    public void TreatsNullEmptyAndLowercaseAsGeneral(string? format)
    {
        Assert.Equal("62,5 kg", Weight.FromKg(62.5m).ToString(format, Lithuanian));
    }

    [Theory]
    [InlineData("60.00", "60 kg")]
    [InlineData("42.750", "42,75 kg")]
    [InlineData("0", "0 kg")]
    public void TrimsTrailingZeros(string kg, string expected)
    {
        var weight = Weight.FromKg(decimal.Parse(kg, CultureInfo.InvariantCulture));

        Assert.Equal(expected, weight.ToString("G", Lithuanian));
    }

    [Theory]
    [InlineData("N")]
    [InlineData("n")]
    public void FormatsNumberOnly(string format)
    {
        Assert.Equal("62,5", Weight.FromKg(62.50m).ToString(format, Lithuanian));
    }

    [Theory]
    [InlineData("LB")]
    [InlineData("lb")]
    public void FormatsPoundsWithOneDecimal(string format)
    {
        Assert.Equal("137,8 lb", Weight.FromKg(62.5m).ToString(format, Lithuanian));
    }

    [Fact]
    public void FormatsPoundsInEnglish()
    {
        Assert.Equal("100.0 lb", Weight.FromKg(45.359237m).ToString("LB", English));
    }

    [Fact]
    public void RoundsPoundsHalfAwayFromZero()
    {
        // 0.25 lb exactly; banker's rounding would give 0,2.
        Assert.Equal("0,3 lb", Weight.FromKg(0.1133980925m).ToString("LB", Lithuanian));
    }

    [Theory]
    [InlineData("X")]
    [InlineData("KG")]
    public void RejectsUnknownFormat(string format)
    {
        Assert.Throws<FormatException>(() => Weight.FromKg(60m).ToString(format, Lithuanian));
    }

    [Fact]
    public void NullProviderUsesCurrentCulture()
    {
        var previous = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = Lithuanian;
            Assert.Equal("62,5 kg", Weight.FromKg(62.5m).ToString("G", null));
            Assert.Equal("62,5 kg", Weight.FromKg(62.5m).ToString());

            CultureInfo.CurrentCulture = English;
            Assert.Equal("62.5 kg", Weight.FromKg(62.5m).ToString("G", null));
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }

    [Fact]
    public void FormatsInInterpolatedString()
    {
        var target = Weight.FromKg(62.5m);

        Assert.Equal("Target 62,5 kg (137,8 lb)", string.Create(Lithuanian, $"Target {target} ({target:LB})"));
    }
}
