# BootstrapBlazor.Copilot.MCPServer

[中文 README](README_zh_CN.md) | **English README**

A Model Context Protocol (MCP) server that provides AI assistants (like GitHub Copilot, Claude) with access to BootstrapBlazor component documentation and source code.

## Project Overview

BootstrapBlazor.Copilot.MCPServer is an MCP server specifically designed for the BootstrapBlazor framework that can:

- 📚 Provide complete documentation for BootstrapBlazor components
- 🔍 Allow AI assistants to search and browse component source code
- 📖 Offer component usage examples and best practices
- 🔄 Automatically sync the latest BootstrapBlazor source code
- 🛠️ Support integration with AI assistants via HTTP transport

## Architecture

The project is built with .NET 9.0 and ASP.NET Core, including the following main components:

### Project Structure

```
BootstrapBlazor.Copilot.MCPServer/
├── BootstrapBlazor.Copilot.MCPServer/           # Main service project
│   ├── Services/                                # Core services
│   │   ├── GitRepositoryManager.cs             # Git repository management
│   │   └── ComponentDocumentationService.cs    # Component documentation service
│   ├── Tools/                                   # MCP tool definitions
│   │   └── ComponentsTool.cs                   # Component query tools
│   ├── Models/                                  # Data models
│   │   └── Component.cs                        # Component-related models
│   └── Program.cs                              # Application entry point
├── BootstrapBlazor.Copilot.MCPServer.AppHost/  # Aspire application host
└── BootstrapBlazor.Copilot.MCPServer.ServiceDefaults/  # Shared service configuration
```

### Core Features

1. **Git Repository Management** (`GitRepositoryManager`)
   - Automatically clone and update BootstrapBlazor official repository
   - Check if local repository is up to date
   - Background service for periodic code synchronization

2. **Component Documentation Service** (`ComponentDocumentationService`)
   - Scan and index all BootstrapBlazor components
   - Provide access to component source code and example files
   - Intelligent path resolution supporting multiple file organization structures

3. **MCP Tools** (`ComponentsTool`)
   - `ListComponents`: List all available components
   - `GetComponentFiles`: Get component file list
   - `GetFileContent`: Get specific file content
   - `Echo`: Test tool

## Technology Stack

- **.NET 9.0**: Latest .NET framework
- **ASP.NET Core**: Web application framework
- **Model Context Protocol**: AI assistant integration protocol
- **LibGit2Sharp**: Git operations library
- **.NET Aspire**: Cloud-native application development platform
- **OpenTelemetry**: Observability and monitoring

## Installation and Configuration

### Prerequisites

- .NET 9.0 SDK
- Git
- Visual Studio 2022 or VS Code

### Quick Start

1. **Clone the project**
   ```bash
   git clone https://github.com/your-repo/BootstrapBlazor.Copilot.MCPServer.git
   cd BootstrapBlazor.Copilot.MCPServer
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Run the service**
   ```bash
   # Using Aspire (recommended)
   dotnet run --project BootstrapBlazor.Copilot.MCPServer.AppHost

   # Or run the main project directly
   dotnet run --project BootstrapBlazor.Copilot.MCPServer
   ```

4. **Verify the service**
   The service runs on `http://localhost:3001` by default and supports the following endpoints:
   - `/health` - Health check
   - `/alive` - Liveness check
   - MCP protocol endpoints

### Configuration Options

Configure the following options in `appsettings.json`:

```json
{
  "McpToolConfig": {
    "ServerUrl": "http://localhost:3001"
  },
  "GitRepository": {
    "LocalPath": "C:\\temp\\BootstrapBlazorRepo"  // Optional: custom local repository path
  }
}
```

## MCP Tool Usage

### 1. List All Components

```bash
# MCP call example
ListComponents()
```

Returns names and descriptions of all BootstrapBlazor components.

### 2. Get Component File List

```bash
GetComponentFiles("Button")
```

Returns a list of source code files and example files for the specified component.

### 3. Get File Content

```bash
GetFileContent({
  "ComponentName": "Button",
  "FileName": "Button.razor.cs",
  "Category": "Source"  // Source or Example
})
```

Returns the complete content of the specified file.

## AI Assistant Integration

This service is designed to integrate with AI assistants that support the MCP protocol:

### GitHub Copilot Integration

1. Add MCP server in GitHub Copilot settings
2. Configure server address: `https://localhost:3001`
3. Restart GitHub Copilot to apply configuration

### Claude Desktop Integration

Add to Claude Desktop configuration file:

```json
{
  "mcpServers": {
    "bootstrapblazor": {
      "command": "dotnet",
      "args": ["run", "--project", "path/to/BootstrapBlazor.Copilot.MCPServer"],
      "env": {
        "ASPNETCORE_ENVIRONMENT": "Production"
      }
    }
  }
}
```

## Development Guide

### Adding New MCP Tools

1. Create a new tool class in the `Tools` directory
2. Mark the class with `[McpServerToolType]`
3. Mark tool methods with `[McpServerTool]`
4. Register the service in `Program.cs`

Example:

```csharp
[McpServerToolType]
public class NewTool
{
    [McpServerTool, Description("New tool description")]
    public string NewMethod(string parameter)
    {
        return "Result";
    }
}
```

### Extending Component Analysis

You can add new component analysis logic in `ComponentDocumentationService`:

- Parse component properties
- Extract usage examples
- Generate component relationship diagrams

## Monitoring and Operations

### Health Checks

- `/health` - Complete health check
- `/alive` - Basic liveness check

### Logging

The project uses the .NET standard logging framework, supporting:
- Console output
- File logging (requires configuration)
- Structured logging
- OpenTelemetry integration

### Performance Monitoring

Provided through .NET Aspire and OpenTelemetry:
- Request tracing
- Performance metrics
- Distributed tracing
- Error monitoring

## Troubleshooting

### Common Issues

1. **Git repository clone failure**
   - Check network connectivity
   - Verify Git is properly installed
   - Check firewall settings

2. **MCP connection failure**
   - Verify server address configuration
   - Check if port is in use
   - Confirm MCP client configuration is correct

3. **Component files not found**
   - Confirm BootstrapBlazor repository is properly cloned
   - Check local repository path configuration
   - Verify component name spelling

### Debug Mode

Enable detailed logging in development environment:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "BootstrapBlazor.Copilot.MCPServer": "Trace"
    }
  }
}
```

## Contributing

1. Fork this project
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Create a Pull Request

## License

This project is licensed under the Apache License 2.0. See the [LICENSE](LICENSE.txt) file for details.

## Related Links

- [BootstrapBlazor Official Website](https://www.blazor.zone/)
- [BootstrapBlazor GitHub Repository](https://github.com/dotnetcore/BootstrapBlazor)
- [Model Context Protocol Specification](https://modelcontextprotocol.io/)
- [.NET Aspire Documentation](https://learn.microsoft.com/en-us/dotnet/aspire/)

## Version History

### v1.1.0
- Update the component model, add example file and documentation file attributes
- Correct the file search logic to look for example code only in the Samples directory
- Update the MCP server version number and the way to access configuration items
- Adjust the ServerUrl configuration in the development environment to use HTTP protocol

### v1.0.0
- Initial release
- Basic MCP tool implementation
- Git repository auto-sync
- Component documentation service

---

**Note**: This project is currently in development, and APIs may change. Thorough testing is recommended before production use.
