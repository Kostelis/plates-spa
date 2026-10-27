using Plates.Api.Demo;
using Plates.Engine;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<IProgressionScheme, DoubleProgressionScheme>();
builder.Services.AddSingleton<ProgressionEngine>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapDemoEndpoints();

app.Run();
