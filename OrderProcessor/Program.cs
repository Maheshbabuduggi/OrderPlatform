using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Azure.Functions.Worker.OpenTelemetry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using OpenTelemetry;


var builder = FunctionsApplication.CreateBuilder(args);

// bind the "Azure" section (change section name to match your config)
builder.Services.Configure<AzureOptions>(builder.Configuration.GetSection("Azure"));

builder.ConfigureFunctionsWebApplication();

if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING")))
{
    builder.Services.AddOpenTelemetry()
        .UseFunctionsWorkerDefaults()
        .UseAzureMonitorExporter();
}

builder.Build().Run();

public class AzureOptions
{
    public string? ApplicationInsightsConnectionString { get; set; }
    // add other properties you need
}
