namespace BootstrapBlazor.Copilot.MCPServer.Models;

public class Component
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<ComponentFile> Files { get; set; } = new List<ComponentFile>();
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
    public FileCategory Category { get; set; } = FileCategory.Source;
}

public class FileContent
{
    public string ComponentName { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
}

public enum FileCategory
{
    Source,
    Example
}