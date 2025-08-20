using BootstrapBlazor.Copilot.MCPServer.Models;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace BootstrapBlazor.Copilot.MCPServer.Services;

public class ComponentDocumentationService
{
    private readonly ILogger<ComponentDocumentationService> _logger;
    private readonly GitRepositoryManager _gitRepositoryManager;
    private readonly string _componentsPath;
    private readonly string _docsPath;
    private readonly string _samplesPath;

    public ComponentDocumentationService(
        ILogger<ComponentDocumentationService> logger,
        GitRepositoryManager gitRepositoryManager)
    {
        _logger = logger;
        _gitRepositoryManager = gitRepositoryManager;
        
        var repoPath = _gitRepositoryManager.GetRepositoryPath();
        // Corrected paths based on the actual repository structure
        _componentsPath = Path.Combine(repoPath, "src", "BootstrapBlazor", "Components");
        _docsPath = Path.Combine(repoPath, "src", "BootstrapBlazor.Server", "Components");
        _samplesPath = Path.Combine(repoPath, "src", "BootstrapBlazor.Server", "Components", "Samples");
    }

    public List<Component> GetComponents()
    {
        var components = new List<Component>();
        
        try
        {
            if (!Directory.Exists(_componentsPath))
            {
                _logger.LogWarning("Components directory not found at {ComponentsPath}", _componentsPath);
                return components;
            }

            // Get all component directories
            var componentDirectories = Directory.GetDirectories(_componentsPath);
            
            foreach (var directory in componentDirectories)
            {
                var directoryName = new DirectoryInfo(directory).Name;
                
                // Skip non-component directories
                if (directoryName.StartsWith(".") || directoryName.Equals("Locales") || 
                    directoryName.Equals("BaseComponents"))
                {
                    continue;
                }
                
                // Get all files in the component directory without extension filtering
                var componentFiles = Directory.GetFiles(directory);
                
                if (!componentFiles.Any())
                {
                    continue;
                }

                var component = new Component { Name = directoryName };
                
                // Add source files information
                foreach (var file in componentFiles)
                {
                    var fileName = Path.GetFileName(file);
                    var fileType = Path.GetExtension(file).TrimStart('.').ToLower();
                    
                    component.Files.Add(new ComponentFile 
                    { 
                        Name = fileName,
                        Type = fileType,
                        Path = file.Replace(_gitRepositoryManager.GetRepositoryPath(), "").TrimStart('\\', '/')
                    });
                }
                
                // Try to extract description from component files
                foreach (var file in componentFiles.Where(f => Path.GetExtension(f).ToLower() is ".cs" or ".razor"))
                {
                    var content = File.ReadAllText(file);
                    
                    // Look for description in XML comments
                    var descMatch = Regex.Match(content, @"<summary>\s*(.*?)\s*</summary>", RegexOptions.Singleline);
                    if (descMatch.Success)
                    {
                        component.Description = descMatch.Groups[1].Value.Trim();
                        break;
                    }
                }
                
                // If no description found, try to infer from class or filename
                if (string.IsNullOrEmpty(component.Description))
                {
                    component.Description = $"{directoryName} component";
                }
                
                // 修正：只在 Samples 目录下查找 example code
                var pluralComponentName = directoryName.EndsWith("s") ? directoryName : $"{directoryName}s";
                var sampleRazor = Path.Combine(_samplesPath, $"{pluralComponentName}.razor");
                var sampleCodeBehind = Path.Combine(_samplesPath, $"{pluralComponentName}.razor.cs");

                if (File.Exists(sampleRazor))
                {
                    component.ExampleFiles.Add(new ComponentFile
                    {
                        Name = Path.GetFileName(sampleRazor),
                        Type = "razor",
                        Path = sampleRazor.Replace(_gitRepositoryManager.GetRepositoryPath(), "").TrimStart('\\', '/')
                    });
                }
                if (File.Exists(sampleCodeBehind))
                {
                    component.ExampleFiles.Add(new ComponentFile
                    {
                        Name = Path.GetFileName(sampleCodeBehind),
                        Type = "cs",
                        Path = sampleCodeBehind.Replace(_gitRepositoryManager.GetRepositoryPath(), "").TrimStart('\\', '/')
                    });
                }

                // Look for documentation files in the docs path
                var docsDir = Path.Combine(_docsPath, directoryName);
                if (Directory.Exists(docsDir))
                {
                    var docFiles = Directory.GetFiles(docsDir)
                        .Where(f => Path.GetExtension(f).ToLower() == ".md")
                        .ToList();
                    
                    foreach (var file in docFiles)
                    {
                        var fileName = Path.GetFileName(file);
                        var fileType = Path.GetExtension(file).TrimStart('.').ToLower();
                        var relativePath = file.Replace(_gitRepositoryManager.GetRepositoryPath(), "").TrimStart('\\', '/');
                        
                        component.DocumentationFiles.Add(new ComponentFile 
                        { 
                            Name = fileName,
                            Type = fileType,
                            Path = relativePath
                        });
                    }
                }
                
                components.Add(component);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting components list");
        }
        
        return components;
    }

    public ComponentFiles GetComponentFiles(string componentName)
    {
        var componentFiles = new ComponentFiles { ComponentName = componentName };
        
        try
        {
            // Find component source files
            var componentDir = Path.Combine(_componentsPath, componentName);
            if (Directory.Exists(componentDir))
            {
                // Get all files in the component directory
                var sourceFiles = Directory.GetFiles(componentDir);
                
                foreach (var file in sourceFiles)
                {
                    var fileName = Path.GetFileName(file);
                    var fileType = Path.GetExtension(file).TrimStart('.').ToLower();
                    var relativePath = file.Replace(_gitRepositoryManager.GetRepositoryPath(), "").TrimStart('\\', '/');
                    
                    componentFiles.SourceFiles.Add(new ComponentFileInfo 
                    {
                        FileName = fileName,
                        FileType = fileType,
                        Path = relativePath
                    });
                }
            }
            else
            {
                _logger.LogWarning("Component directory not found: {ComponentDir}", componentDir);
            }
            
            // 修正：只在 Samples 目录下查找 example code
            var pluralComponentName = componentName.EndsWith("s") ? componentName : $"{componentName}s";
            var sampleRazor = Path.Combine(_samplesPath, $"{pluralComponentName}.razor");
            var sampleCodeBehind = Path.Combine(_samplesPath, $"{pluralComponentName}.razor.cs");

            if (File.Exists(sampleRazor))
            {
                componentFiles.ExampleFiles.Add(new ComponentFileInfo
                {
                    FileName = Path.GetFileName(sampleRazor),
                    FileType = "razor",
                    Path = sampleRazor.Replace(_gitRepositoryManager.GetRepositoryPath(), "").TrimStart('\\', '/')
                });
            }
            if (File.Exists(sampleCodeBehind))
            {
                componentFiles.ExampleFiles.Add(new ComponentFileInfo
                {
                    FileName = Path.GetFileName(sampleCodeBehind),
                    FileType = "cs",
                    Path = sampleCodeBehind.Replace(_gitRepositoryManager.GetRepositoryPath(), "").TrimStart('\\', '/')
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting component files for {ComponentName}", componentName);
        }
        
        return componentFiles;
    }

    /// <summary>
    /// Gets the content of a specific file for a component.
    /// </summary>
    /// <param name="componentName">The name of the component</param>
    /// <param name="fileName">The name of the file to retrieve</param>
    /// <param name="category">Specifies whether to search in source files (FileCategory.Source) or example/sample files (FileCategory.Example)</param>
    /// <returns>The file content and metadata</returns>
    public FileContent GetFileContent(string componentName, string fileName, FileCategory category)
    {
        var result = new FileContent 
        { 
            ComponentName = componentName,
            FileName = fileName
        };
        
        try
        {
            string filePath = null;
            
            if (category == FileCategory.Source)
            {
                filePath = Path.Combine(_componentsPath, componentName, fileName);
            }
            else
            {
                // 修正：只在 Samples 目录下查找 example code
                var pluralComponentName = componentName.EndsWith("s") ? componentName : $"{componentName}s";
                var sampleRazor = Path.Combine(_samplesPath, $"{pluralComponentName}.razor");
                var sampleCodeBehind = Path.Combine(_samplesPath, $"{pluralComponentName}.razor.cs");

                if (string.Equals(fileName, $"{pluralComponentName}.razor", StringComparison.OrdinalIgnoreCase) && File.Exists(sampleRazor))
                {
                    filePath = sampleRazor;
                }
                else if (string.Equals(fileName, $"{pluralComponentName}.razor.cs", StringComparison.OrdinalIgnoreCase) && File.Exists(sampleCodeBehind))
                {
                    filePath = sampleCodeBehind;
                }
            }
            
            if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
            {
                result.Content = File.ReadAllText(filePath);
                result.FileType = Path.GetExtension(filePath).TrimStart('.').ToLower();
            }
            else
            {
                _logger.LogWarning("File not found: {FilePath} for component {ComponentName}", 
                    fileName, componentName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting file content for {FileName} of component {ComponentName}", 
                fileName, componentName);
        }
        
        return result;
    }

    public ComponentDocumentation GetComponentDocumentation(string componentName)
    {
        var documentation = new ComponentDocumentation();
        
        try
        {
            // Find component source code
            var componentDir = Path.Combine(_componentsPath, componentName);
            if (Directory.Exists(componentDir))
            {
                // Get all files in the component directory without extension filtering
                var sourceFiles = Directory.GetFiles(componentDir);
                
                foreach (var file in sourceFiles)
                {
                    var fileName = Path.GetFileName(file);
                    var fileType = Path.GetExtension(file).TrimStart('.').ToLower();
                    var content = File.ReadAllText(file);
                    
                    documentation.SourceFiles.Add(new DocumentFile 
                    {
                        FileName = fileName,
                        FileType = fileType,
                        Content = content
                    });
                }
            }
            else
            {
                _logger.LogWarning("Component directory not found: {ComponentDir}", componentDir);
            }
            
            // 修正：只在 Samples 目录下查找 example code
            var pluralComponentName = componentName.EndsWith("s") ? componentName : $"{componentName}s";
            var sampleRazor = Path.Combine(_samplesPath, $"{pluralComponentName}.razor");
            var sampleCodeBehind = Path.Combine(_samplesPath, $"{pluralComponentName}.razor.cs");

            if (File.Exists(sampleRazor))
            {
                documentation.ExampleFiles.Add(new DocumentFile
                {
                    FileName = Path.GetFileName(sampleRazor),
                    FileType = "razor",
                    Content = File.ReadAllText(sampleRazor)
                });
            }
            if (File.Exists(sampleCodeBehind))
            {
                documentation.ExampleFiles.Add(new DocumentFile
                {
                    FileName = Path.GetFileName(sampleCodeBehind),
                    FileType = "cs",
                    Content = File.ReadAllText(sampleCodeBehind)
                });
            }

            if (!documentation.ExampleFiles.Any())
            {
                _logger.LogWarning("No example files found for component: {ComponentName}", componentName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting component documentation for {ComponentName}", componentName);
        }
        
        return documentation;
    }
}