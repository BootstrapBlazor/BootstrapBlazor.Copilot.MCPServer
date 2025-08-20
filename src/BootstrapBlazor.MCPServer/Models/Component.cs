using System.ComponentModel;

namespace BootstrapBlazor.Copilot.MCPServer.Models;

public class Component
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<ComponentFile> Files { get; set; } = new List<ComponentFile>();
    
    /// <summary>
    /// List of example files associated with this component
    /// </summary>
    public List<ComponentFile> ExampleFiles { get; set; } = new List<ComponentFile>();
    
    /// <summary>
    /// List of documentation files associated with this component
    /// </summary>
    public List<ComponentFile> DocumentationFiles { get; set; } = new List<ComponentFile>();
}

public class ComponentFile
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
}

public class ComponentDocumentation
{
    public List<DocumentFile> SourceFiles { get; set; } = new List<DocumentFile>();
    public List<DocumentFile> ExampleFiles { get; set; } = new List<DocumentFile>();
}

public class DocumentFile
{
    public string FileName { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}

public class ComponentFiles
{
    public string ComponentName { get; set; } = string.Empty;
    public List<ComponentFileInfo> SourceFiles { get; set; } = new List<ComponentFileInfo>();
    public List<ComponentFileInfo> ExampleFiles { get; set; } = new List<ComponentFileInfo>();
}

public class ComponentFileInfo
{
    public string FileName { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
}

public class FileContentRequest
{
    public string ComponentName { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    [Description("0=Source Code, 1=Example/Sample Code")]
    public FileCategory Category { get; set; } = FileCategory.Source;
}

public class FileContent
{
    public string ComponentName { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
}

// Add XML documentation to FileCategory enum
public enum FileCategory
{
    /// <summary>
    /// Indicates that the file is a source code file located in the component's source directory
    /// </summary>
    Source,
    
    /// <summary>
    /// Indicates that the file is an example or sample file located in documentation or samples directories
    /// </summary>
    Example
}