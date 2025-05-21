// Importamos las librerias
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Exporter;
using System.Diagnostics.Metrics;


var builder = WebApplication.CreateBuilder(args);

var metrics = new CustomMetrics(); // Creamos el objeto de nuestras CustomMetrics

builder.Services.AddOpenTelemetry() // Agregamos OpenTelemetry al builder con nuestras CustomMetrics
        .WithMetrics(metricsBuilder =>
        {
            metricsBuilder
        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("Dice-App"))
        .AddAspNetCoreInstrumentation()
        .AddMeter("CustomMetrics")
        .AddOtlpExporter(options => // Agregamos otlp como exporter y decimos la ruta al servicio
            {
                options.Endpoint = new Uri("http://localhost:4317");
                options.Protocol = OtlpExportProtocol.Grpc;
            })
        .AddView(instrumentName: "dice_results", new ExplicitBucketHistogramConfiguration { Boundaries = new double[] { 1, 2, 3, 4, 5, 6 } }); // Configuramos los Buckets para La metrica de resultados
        });

var app = builder.Build();


string HandleRollDice() // Funcion que retorna un numero del 1 al 6
{
    var result = Random.Shared.Next(1, 7);
    metrics.DiceRolls(); // Incrementamos el contador de tiradas
    metrics.DiceResults(result); // Agregamos el resultado al histograma de resultados

    return result.ToString();
}

app.MapGet("/rolldice", HandleRollDice); // Mapeamos la funcion a /rolldice

app.Run();

class CustomMetrics // Aca esta la magia
{
    private readonly Meter _meter;
    private readonly Counter<long> _diceRollsCounter;
    private readonly Histogram<int> _diceResultsHistogram;

    public CustomMetrics()
    {
        _meter = new Meter("CustomMetrics"); // Creamos un Meter con el mismo nombre que pusimos mas arriba
        _diceRollsCounter = _meter.CreateCounter<long>( // Creamos nuestro contador de tiradas
            "dice_rolls",
            unit: "1",
            description: "Counts the number of dice rolls"
        );

        _diceResultsHistogram = _meter.CreateHistogram<int>( // Creamos nuestro histograma de resultados
            "dice_results",
            unit: "1",
            description: "Records the results of dice rolls"
        );
    }

    public void DiceRolls(long value = 1) // Funcion que incrementa el contador de tiradas
    {
        _diceRollsCounter.Add(value);
    }

    public void DiceResults(int result) // Funcion que agrega un resultado al histograma de resultados
    {
        _diceResultsHistogram.Record(result);
    }
}
