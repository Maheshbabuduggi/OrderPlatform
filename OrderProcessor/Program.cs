extern alias azid1;
using AzureIdentity = azid1::Azure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OrderProcessor.Data;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureAppConfiguration((context, config) =>
    {
        var keyVaultUri = Environment.GetEnvironmentVariable("KeyVaultUri");
        if (!string.IsNullOrWhiteSpace(keyVaultUri))
        {
            config.AddAzureKeyVault(new Uri(keyVaultUri), new AzureIdentity.DefaultAzureCredential());
        }
    })
    .ConfigureServices((context, services) =>
    {
        var configuration = context.Configuration;

        var sqlConnectionString = configuration["SqlConnectionString"]
            ?? throw new InvalidOperationException("SqlConnectionString not found in configuration/Key Vault.");

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(sqlConnectionString, sql => sql.EnableRetryOnFailure(3)));

        services.AddApplicationInsightsTelemetryWorkerService();
       // services.ConfigureFunctionsApplicationInsights();
    })
    .Build();

host.Run();