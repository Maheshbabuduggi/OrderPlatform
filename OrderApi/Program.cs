extern alias azid;
using AzureIdentity = azid::Azure.Identity;
using Azure.Messaging.EventHubs.Producer;
using Microsoft.EntityFrameworkCore;
using OrderApi.Configuration;
using OrderApi.Data;
using OrderApi.Messaging;
using OrderApi.Services;

var builder = WebApplication.CreateBuilder(args);

// ---- 1. Bind strongly-typed Azure options from appsettings.json ----
builder.Services.Configure<AzureOptions>(builder.Configuration.GetSection(AzureOptions.SectionName));
var azureOptions = builder.Configuration.GetSection(AzureOptions.SectionName).Get<AzureOptions>()
    ?? throw new InvalidOperationException("Missing 'Azure' configuration section.");

// ---- 2. Add Azure Key Vault as a configuration provider ----
// DefaultAzureCredential resolves to: environment vars -> Managed Identity (in Azure) -> Azure CLI login (local dev).
//var credential = new Azure.Identity.DefaultAzureCredential();
var credential = new AzureIdentity.DefaultAzureCredential();

builder.Configuration.AddAzureKeyVault(
    new Uri(azureOptions.KeyVaultUri),
    credential);

// ---- 3. Read the SQL connection string that Key Vault just injected into configuration ----
var sqlConnectionString = builder.Configuration[azureOptions.SqlConnectionStringSecretName]
    ?? throw new InvalidOperationException(
        $"Secret '{azureOptions.SqlConnectionStringSecretName}' not found in Key Vault.");

// ---- 4. EF Core / Azure SQL ----
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(sqlConnectionString, sql => sql.EnableRetryOnFailure(3)));

// ---- 5. Event Hub producer client (singleton — the SDK client is thread-safe and expensive to create) ----
builder.Services.AddSingleton(_ => new EventHubProducerClient(
    azureOptions.EventHubFullyQualifiedNamespace,
    azureOptions.EventHubName,
    credential));

// ---- 6. Application services (DI) ----
builder.Services.AddScoped<IEventPublisher, EventHubPublisher>();
builder.Services.AddScoped<IOrderService, OrderService>();

// ---- 7. ASP.NET Core plumbing ----
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Order Platform API", Version = "v1" });
});

var app = builder.Build();

// ---- 8. Apply pending EF Core migrations automatically on startup ----
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();