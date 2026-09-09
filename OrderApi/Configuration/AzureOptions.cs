namespace OrderApi.Configuration;

public class AzureOptions
{
    public const string SectionName = "Azure";

    public string KeyVaultUri { get; set; } = string.Empty;
    public string EventHubFullyQualifiedNamespace { get; set; } = string.Empty;
    public string EventHubName { get; set; } = string.Empty;
    public string SqlConnectionStringSecretName { get; set; } = "SqlConnectionString";
}