using System.ComponentModel;
using BootstrapBlazor.Copilot.MCPServer.Models;
using BootstrapBlazor.Copilot.MCPServer.Services;
using ModelContextProtocol.Server;

namespace BootstrapBlazor.Copilot.MCPServer.Tools;

[McpServerToolType]
public class ComponentsTool
{
    private readonly ComponentDocumentationService _documentationService;

    public ComponentsTool(ComponentDocumentationService documentationService)
    {
        _documentationService = documentationService;
    }

    [McpServerTool, Description("Lists all BootstrapBlazor components with their names and descriptions")]
    public List<Models.Component> ListComponents()
    {
        return _documentationService.GetComponents();
    }

    [McpServerTool, Description("Gets just the file names for a specific component without content")]
    public ComponentFiles GetComponentFiles(string componentName)
    {
        return _documentationService.GetComponentFiles(componentName);
    }

    [McpServerTool, Description("Gets the content of a specific file for a component")]
    public FileContent GetFileContent(FileContentRequest request)
    {
        return _documentationService.GetFileContent(request.ComponentName, request.FileName, request.Category);
    }

    //[McpServerTool, Description("Gets source code and example documentation for a specific component (returns all content at once, prefer using GetComponentFiles and GetFileContent for large components)")]
    public ComponentDocumentation GetDocument(string componentName)
    {
        return _documentationService.GetComponentDocumentation(componentName);
    }
}