using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace Plates.Api.Tests;

public class DemoProgressTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private static readonly Guid SquatId = Guid.Parse("5f0c1a52-8d2e-4c1b-9b3a-0a1e2d3c4b5a");
    private static readonly Guid BenchId = Guid.Parse("b7e4d3c2-1a0f-4e9d-8c7b-6a5f4e3d2c1b");

    /// <summary>The fixed "now" of every test: 27 October 2026, 18:00 UTC.</summary>
    private static readonly DateTimeOffset Now = new(2026, 10, 27, 18, 0, 0, TimeSpan.Zero);

    /// <summary>Section 7 program: squat 3 × 5–8, +5 kg; bench press 3 × 6–10, +2,5 kg.</summary>
    private static readonly object[] Program =
    [
        new { exerciseId = SquatId, name = "Pritūpimas", schemeKey = "double-progression",
              parameters = new { sets = 3, minReps = 5, maxReps = 8, startingWeight = 60, increment = 5 } },
        new { exerciseId = BenchId, name = "Spaudimas gulint", schemeKey = "double-progression",
              parameters = new { sets = 3, minReps = 6, maxReps = 10, startingWeight = 47.5m, increment = 2.5m } },
    ];

    [Fact]
    public async Task Section7Scenario_SquatIncreasesAndBenchDeloads()
    {
        var client = NewClient();

        // The earlier workout, from a fresh state.
        var earlier = await PostOk(client, Request([], Now, Log(SquatId, (60m, 7), (60m, 6), (60m, 6)), Log(BenchId, (47.5m, 7), (47.5m, 5), (47.5m, 4))));
        Assert.Equal(60m, Outcome(earlier, SquatId).GetProperty("target").GetDecimal());
        Assert.Equal(74.0m, Outcome(earlier, SquatId).GetProperty("record").GetDecimal());
        Assert.Equal("Failure", Outcome(earlier, BenchId).GetProperty("code").GetString());
        Assert.Equal(1, Outcome(earlier, BenchId).GetProperty("failureCount").GetInt32());
        Assert.Equal(58.6m, Outcome(earlier, BenchId).GetProperty("record").GetDecimal());

        // The returned states are fed back, as the page does.
        var states = new[] { StateFrom(earlier, SquatId), StateFrom(earlier, BenchId) };
        var next = await PostOk(client, Request(states, Now, Log(SquatId, (60m, 8), (60m, 8), (60m, 8)), Log(BenchId, (47.5m, 7), (47.5m, 5), (47.5m, 4))));

        var squat = Outcome(next, SquatId);
        Assert.Equal(65m, squat.GetProperty("target").GetDecimal());
        Assert.Equal("Increase", squat.GetProperty("code").GetString());
        Assert.Equal("Visose serijose pasiektas pakartojimų maksimumas – svoris didinamas.", squat.GetProperty("reason").GetString());
        Assert.Equal(76.0m, squat.GetProperty("record").GetDecimal());

        var bench = Outcome(next, BenchId);
        Assert.Equal(42.5m, bench.GetProperty("target").GetDecimal());
        Assert.Equal("Deload", bench.GetProperty("code").GetString());
        Assert.Equal(0, bench.GetProperty("failureCount").GetInt32());
    }

    [Fact]
    public async Task FormatsTargetAndRecordInKg()
    {
        var response = await PostOk(NewClient(), Request([], Now, Log(SquatId, (60m, 7), (60m, 6), (60m, 6))));

        var squat = Outcome(response, SquatId);
        Assert.Equal("60 kg", squat.GetProperty("targetText").GetString());
        Assert.Equal("74 kg", squat.GetProperty("recordText").GetString());
    }

    [Fact]
    public async Task A1000KgSetReturnsTheLimitErrorAndNoOutcomes()
    {
        var response = await NewClient().PostAsJsonAsync("/demo/progress", Request([], Now, Log(SquatId, (60m, 8), (1000m, 8), (60m, 8))), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await ReadJson(response);
        var error = Assert.Single(body.GetProperty("errors").EnumerateArray());
        Assert.Equal("2-osios serijos svoris 1000 kg viršija 500 kg ribą", error.GetProperty("message").GetString());
        Assert.Equal(SquatId, error.GetProperty("exerciseId").GetGuid());
        Assert.Equal(2, error.GetProperty("setNumber").GetInt32());
        Assert.False(body.TryGetProperty("outcomes", out _));
    }

    [Fact]
    public async Task ASkippedExerciseGetsNoOutcome()
    {
        var response = await PostOk(NewClient(), Request([], Now, Log(SquatId, (60m, 8), (60m, 8), (60m, 8))));

        var outcome = Assert.Single(response.GetProperty("outcomes").EnumerateArray());
        Assert.Equal(SquatId, outcome.GetProperty("exerciseId").GetGuid());
    }

    [Fact]
    public async Task AWorkoutDatedTomorrowReturnsTheFutureDateError()
    {
        var response = await NewClient().PostAsJsonAsync("/demo/progress", Request([], Now.AddDays(1), Log(SquatId, (60m, 8), (60m, 8), (60m, 8))), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = Assert.Single((await ReadJson(response)).GetProperty("errors").EnumerateArray());
        Assert.Equal("Treniruotės data negali būti ateityje", error.GetProperty("message").GetString());
    }

    [Fact]
    public async Task TheRootUrlServesTheDemoPage()
    {
        var response = await NewClient().GetAsync("/", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/html", response.Content.Headers.ContentType?.MediaType);
        Assert.Contains("Registruoti treniruotę", await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
    }

    private HttpClient NewClient() => factory
        .WithWebHostBuilder(builder => builder.ConfigureTestServices(services => services.AddSingleton<TimeProvider>(new FixedTimeProvider(Now))))
        .CreateClient();

    private static object Request(object[] states, DateTimeOffset date, params object[] exercises) =>
        new { program = Program, states, date, exercises };

    private static object Log(Guid exerciseId, params (decimal Kg, int Reps)[] sets) =>
        new { exerciseId, sets = sets.Select(set => new { weight = set.Kg, reps = set.Reps }).ToArray() };

    private static object StateFrom(JsonElement response, Guid exerciseId)
    {
        var outcome = Outcome(response, exerciseId);
        return new { exerciseId, record = outcome.GetProperty("record").GetDecimal(), schemeState = outcome.GetProperty("schemeState") };
    }

    private static JsonElement Outcome(JsonElement response, Guid exerciseId) =>
        response.GetProperty("outcomes").EnumerateArray().Single(outcome => outcome.GetProperty("exerciseId").GetGuid() == exerciseId);

    private static async Task<JsonElement> PostOk(HttpClient client, object request)
    {
        var response = await client.PostAsJsonAsync("/demo/progress", request, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return await ReadJson(response);
    }

    private static async Task<JsonElement> ReadJson(HttpResponseMessage response) =>
        await response.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken);
}
