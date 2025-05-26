using BootstrapBlazor.Copilot.MCPServer.Services;
using ModelContextProtocol.Server;
using System.ComponentModel;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddMcpServer(options =>
{
    options.ServerInfo = new()
    {
        Name = "BootstrapBlazor Document & Source Code MCP Server",
        Version = "1.0.0"
    };
})
    .WithHttpTransport()
    .WithToolsFromAssembly();

// Add services to the container.
builder.Services.AddOpenApi();

// Register custom services
builder.Services.AddSingleton<GitRepositoryManager>();
builder.Services.AddSingleton<ComponentDocumentationService>();

// Add Git Repository Update Hosted Service
builder.Services.AddHostedService<GitRepositoryUpdateService>();

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

//app.UseHttpsRedirection();

app.MapMcp();

app.Run(builder.Configuration["McpToolConfig::ServerUrl"]);

[McpServerToolType]
public static class EchoTool
{
    [McpServerTool, Description("Echoes the message back to the client.")]
    public static string Echo(string message) => $"hello {message}";
}

// Background service to check and update Git repository on startup
public class GitRepositoryUpdateService : BackgroundService
{
    private readonly GitRepositoryManager _gitRepositoryManager;
    private readonly ILogger<GitRepositoryUpdateService> _logger;

    public GitRepositoryUpdateService(
        GitRepositoryManager gitRepositoryManager,
        ILogger<GitRepositoryUpdateService> logger)
    {
        _gitRepositoryManager = gitRepositoryManager;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            _logger.LogInformation("Checking if Git repository is up to date");
            
            if (!_gitRepositoryManager.IsRepositoryUpToDate())
            {
                _logger.LogInformation("Repository is not up to date. Updating...");
                await _gitRepositoryManager.UpdateRepositoryAsync();
            }
            else
            {
                _logger.LogInformation("Repository is up to date");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating Git repository");
        }
    }
}