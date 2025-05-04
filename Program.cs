using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddOpenTelemetry(x => x.AddOtlpExporter(a => 
{
    a.Endpoint = new Uri("http://localhost:5341/ingest/otlp/v1/logs");
    a.Protocol = OtlpExportProtocol.HttpProtobuf;
    a.Headers = "X-Seq-ApiKey=bQ8iaI9kcKoQJGivHv4q";
}));

// Add services to the container.]
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weather", (string city, int days, ILogger<Program> logger) =>
{
    var forecast =  Enumerable.Range(1, days).Select(index =>
        new WeatherForecast
        (
            city,
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    logger.LogInformation("Weather forecast for {City} for {Days} days", city, days);
    logger.LogInformation("Weather forecast: {Forecast}", forecast);
    return forecast;
})
.WithName("GetWeatherForecast");

app.Run(); 

record WeatherForecast(string City, DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
