using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Plates.Api.Tests;

public class OpenApiTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task PublishesOpenApiDocumentInDevelopment()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/openapi/v1.json", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("\"openapi\"", await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
    }
}
