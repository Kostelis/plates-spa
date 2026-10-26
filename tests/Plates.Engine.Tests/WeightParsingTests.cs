using System.Globalization;

namespace Plates.Engine.Tests;

public class WeightParsingTests
{
    private static readonly CultureInfo Lithuanian = CultureInfo.GetCultureInfo("lt-LT");
    private static readonly CultureInfo English = CultureInfo.GetCultureInfo("en-US");

    [Theory]
    [InlineData("62,5 kg")]
    [InlineData("62,5kg")]
    [InlineData("62,5")]
    [InlineData("62,5 KG")]
    [InlineData("  62,5 Kg  ")]
    public void ParsesLithuanianWithOptionalKgSuffix(string text)
    {
        var parsed = Weight.TryParse(text, Lithuanian, out var weight);

        Assert.True(parsed);
        Assert.Equal(Weight.FromKg(62.5m), weight);
    }

    [Fact]
    public void ParsesEnglishDecimalPoint()
    {
        var parsed = Weight.TryParse("62.5 kg", English, out var weight);

        Assert.True(parsed);
        Assert.Equal(Weight.FromKg(62.5m), weight);
    }

    [Fact]
    public void ParsesWhatItFormats()
    {
        var target = Weight.FromKg(142.5m);

        var parsed = Weight.TryParse(target.ToString("G", Lithuanian), Lithuanian, out var weight);

        Assert.True(parsed);
        Assert.Equal(target, weight);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("kg")]
    [InlineData("-5 kg")]
    [InlineData("62,5 lb")]
    [InlineData("sixty kg")]
    [InlineData("62,5 kg kg")]
    public void RejectsInvalidText(string? text)
    {
        var parsed = Weight.TryParse(text, Lithuanian, out var weight);

        Assert.False(parsed);
        Assert.Equal(default, weight);
    }

    [Theory]
    [InlineData("62,5")]
    [InlineData("4,5 kg")]
    [InlineData("1,000 kg")]
    public void RejectsThousandsSeparator(string text)
    {
        var parsed = Weight.TryParse(text, English, out var weight);

        Assert.False(parsed);
        Assert.Equal(default, weight);
    }
}
