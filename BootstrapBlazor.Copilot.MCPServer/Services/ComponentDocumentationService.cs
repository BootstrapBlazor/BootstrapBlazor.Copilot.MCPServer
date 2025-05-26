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
                
                // Add files information
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
            
            // Find example files - check in three potential locations
            
            // 1. Direct component folder in Components directory
            var exampleDir = Path.Combine(_docsPath, componentName);
            var foundExamples = false;
            
            if (Directory.Exists(exampleDir))
            {
                var exampleFiles = Directory.GetFiles(exampleDir);
                
                foreach (var file in exampleFiles)
                {
                    var fileName = Path.GetFileName(file);
                    var fileType = Path.GetExtension(file).TrimStart('.').ToLower();
                    var relativePath = file.Replace(_gitRepositoryManager.GetRepositoryPath(), "").TrimStart('\\', '/');
                    
                    componentFiles.ExampleFiles.Add(new ComponentFileInfo 
                    {
                        FileName = fileName,
                        FileType = fileType,
                        Path = relativePath
                    });
                }
                foundExamples = componentFiles.ExampleFiles.Any();
            }
            
            // 2. Check for plural form in Samples directory
            if (!foundExamples)
            {
                var pluralComponentName = componentName.EndsWith("s") ? componentName : $"{componentName}s";
                var samplePath = Path.Combine(_samplesPath, pluralComponentName);
                
                if (Directory.Exists(samplePath))
                {
                    var sampleFiles = Directory.GetFiles(samplePath);
                    
                    foreach (var file in sampleFiles)
                    {
                        var fileName = Path.GetFileName(file);
                        var fileType = Path.GetExtension(file).TrimStart('.').ToLower();
                        var relativePath = file.Replace(_gitRepositoryManager.GetRepositoryPath(), "").TrimStart('\\', '/');
                        
                        componentFiles.ExampleFiles.Add(new ComponentFileInfo 
                        {
                            FileName = fileName,
                            FileType = fileType,
                            Path = relativePath
                        });
                    }
                    foundExamples = componentFiles.ExampleFiles.Any();
                }
            }
            
            // 3. Check for subdirectories in Samples directory
            if (!foundExamples)
            {
                // Look for nested directories in Samples
                foreach (var sampleSubDir in Directory.GetDirectories(_samplesPath))
                {
                    var dirName = new DirectoryInfo(sampleSubDir).Name;
                    
                    if (dirName.Equals(componentName, StringComparison.OrdinalIgnoreCase) ||
                        dirName.StartsWith(componentName, StringComparison.OrdinalIgnoreCase))
                    {
                        var nestedFiles = Directory.GetFiles(sampleSubDir, "*.*", SearchOption.AllDirectories);
                        
                        foreach (var file in nestedFiles)
                        {
                            var fileName = Path.GetFileName(file);
                            var fileType = Path.GetExtension(file).TrimStart('.').ToLower();
                            var relativePath = file.Replace(_gitRepositoryManager.GetRepositoryPath(), "").TrimStart('\\', '/');
                            
                            componentFiles.ExampleFiles.Add(new ComponentFileInfo 
                            {
                                FileName = fileName,
                                FileType = fileType,
                                Path = relativePath
                            });
                        }
                        break;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting component files for {ComponentName}", componentName);
        }
        
        return componentFiles;
    }

    public FileContent GetFileContent(string componentName, string fileName, FileCategory category)
    {
        var result = new FileContent 
        { 
            ComponentName = componentName,
            FileName = fileName
        };
        
        try
        {
            string filePath;
            
            if (category == FileCategory.Source)
            {
                filePath = Path.Combine(_componentsPath, componentName, fileName);
            }
            else
            {
                // First check direct component folder in Components directory
                filePath = Path.Combine(_docsPath, componentName, fileName);
                
                // If not found, check plural form in Samples directory
                if (!File.Exists(filePath))
                {
                    var pluralComponentName = componentName.EndsWith("s") ? componentName : $"{componentName}s";
                    filePath = Path.Combine(_samplesPath, pluralComponentName, fileName);
                }
                
                // If still not found, search in subdirectories of Samples
                if (!File.Exists(filePath))
                {
                    foreach (var sampleSubDir in Directory.GetDirectories(_samplesPath))
                    {
                        var dirName = new DirectoryInfo(sampleSubDir).Name;
                        
                        if (dirName.Equals(componentName, StringComparison.OrdinalIgnoreCase) ||
                            dirName.StartsWith(componentName, StringComparison.OrdinalIgnoreCase))
                        {
                            var matchingFiles = Directory.GetFiles(sampleSubDir, fileName, SearchOption.AllDirectories);
                            if (matchingFiles.Any())
                            {
                                filePath = matchingFiles.First();
                                break;
                            }
                        }
                    }
                }
            }
            
            if (File.Exists(filePath))
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
            
            // Find example code - check in three potential locations
            
            // 1. Direct component folder in Components directory
            var exampleDir = Path.Combine(_docsPath, componentName);
            var foundExamples = false;
            
            if (Directory.Exists(exampleDir))
            {
                var exampleFiles = Directory.GetFiles(exampleDir);
                
                foreach (var file in exampleFiles)
                {
                    var fileName = Path.GetFileName(file);
                    var fileType = Path.GetExtension(file).TrimStart('.').ToLower();
                    var content = File.ReadAllText(file);
                    
                    documentation.ExampleFiles.Add(new DocumentFile 
                    {
                        FileName = fileName,
                        FileType = fileType,
                        Content = content
                    });
                }
                foundExamples = documentation.ExampleFiles.Any();
            }
            
            // 2. Check for plural form in Samples directory
            if (!foundExamples)
            {
                var pluralComponentName = componentName.EndsWith("s") ? componentName : $"{componentName}s";
                var samplePath = Path.Combine(_samplesPath, pluralComponentName);
                
                if (Directory.Exists(samplePath))
                {
                    var sampleFiles = Directory.GetFiles(samplePath);
                    
                    foreach (var file in sampleFiles)
                    {
                        var fileName = Path.GetFileName(file);
                        var fileType = Path.GetExtension(file).TrimStart('.').ToLower();
                        var content = File.ReadAllText(file);
                        
                        documentation.ExampleFiles.Add(new DocumentFile 
                        {
                            FileName = fileName,
                            FileType = fileType,
                            Content = content
                        });
                    }
                    foundExamples = documentation.ExampleFiles.Any();
                }
            }
            
            // 3. Check for subdirectories in Samples directory
            if (!foundExamples)
            {
                // Look for nested directories in Samples
                foreach (var sampleSubDir in Directory.GetDirectories(_samplesPath))
                {
                    var dirName = new DirectoryInfo(sampleSubDir).Name;
                    
                    if (dirName.Equals(componentName, StringComparison.OrdinalIgnoreCase) ||
                        dirName.StartsWith(componentName, StringComparison.OrdinalIgnoreCase))
                    {
                        var nestedFiles = Directory.GetFiles(sampleSubDir, "*.*", SearchOption.AllDirectories);
                        
                        foreach (var file in nestedFiles)
                        {
                            var fileName = Path.GetFileName(file);
                            var fileType = Path.GetExtension(file).TrimStart('.').ToLower();
                            var content = File.ReadAllText(file);
                            
                            documentation.ExampleFiles.Add(new DocumentFile 
                            {
                                FileName = fileName,
                                FileType = fileType,
                                Content = content
                            });
                        }
                        break;
                    }
                }
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