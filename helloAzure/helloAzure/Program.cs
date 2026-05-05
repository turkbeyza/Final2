using Npgsql;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var keyVaultUrl = "https://kv-final2.vault.azure.net/";

var secretClient = new SecretClient(
    new Uri(keyVaultUrl),
    new DefaultAzureCredential()
);

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.MapGet("/hello", async () =>
{
    try
    {
        
        var dbUrl = (await secretClient.GetSecretAsync("db-url")).Value.Value;
        var dbUser = (await secretClient.GetSecretAsync("db-username")).Value.Value;
        var dbPassword = (await secretClient.GetSecretAsync("db-password")).Value.Value;

        var connectionString =
            $"Server={dbUrl};Database=postgres;Port=5432;User Id={dbUser};Password={dbPassword};Ssl Mode=Require;";

        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync();

        return Results.Ok("Database connection SUCCESSFUL with ACR");
    }
    catch (Exception ex)
    {
        return Results.Problem($"Database connection FAILED: {ex.Message}");
    }
});

app.Run();