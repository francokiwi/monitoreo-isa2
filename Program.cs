using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Exporter;
using System.Diagnostics.Metrics;


var builder = WebApplication.CreateBuilder(args);


var metrics = new CustomMetrics();

builder.Services.AddOpenTelemetry()
        .WithMetrics(metricsBuilder =>
        {
            metricsBuilder
        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("Dice-App"))
        .AddAspNetCoreInstrumentation()
        .AddMeter("CustomMetrics")
        .AddOtlpExporter(options =>
            {
                options.Endpoint = new Uri("http://localhost:4317");
                options.Protocol = OtlpExportProtocol.Grpc;
            })
        .AddView(instrumentName: "dice_results", new ExplicitBucketHistogramConfiguration { Boundaries = new double[] { 1, 2, 3, 4, 5, 6 } });
        });

var app = builder.Build();


string HandleRollDice(string? player)
{
    var result = Random.Shared.Next(1, 7);
    metrics.DiceRolls();
    metrics.DiceResults(result);

    return result.ToString();
}

app.MapGet("/rolldice", HandleRollDice);

app.Run();

class CustomMetrics
{
    private readonly Meter _meter;
    private readonly Counter<long> _diceRollsCounter;
    private readonly Histogram<int> _diceResultsHistogram;

    public CustomMetrics()
    {
        _meter = new Meter("CustomMetrics");
        _diceRollsCounter = _meter.CreateCounter<long>(
            "dice_rolls",
            unit: "1",
            description: "Counts the number of dice rolls"
        );

        _diceResultsHistogram = _meter.CreateHistogram<int>(
            "dice_results",
            unit: "1",
            description: "Records the results of dice rolls"
        );
    }

    public void DiceRolls(long value = 1)
    {
        _diceRollsCounter.Add(value);
    }

    public void DiceResults(int result)
    {
        _diceResultsHistogram.Record(result);
    }
}
