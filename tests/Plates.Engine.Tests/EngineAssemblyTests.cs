using System.Reflection;

namespace Plates.Engine.Tests;

public class EngineAssemblyTests
{
    [Fact]
    public void ReferencesNoWebOrDataAssemblies()
    {
        var engine = Assembly.Load("Plates.Engine");

        var forbidden = engine.GetReferencedAssemblies()
            .Select(reference => reference.Name ?? "")
            .Where(name => name.StartsWith("Microsoft.AspNetCore")
                || name.StartsWith("Microsoft.EntityFrameworkCore")
                || name.StartsWith("Npgsql"));

        Assert.Empty(forbidden);
    }
}
