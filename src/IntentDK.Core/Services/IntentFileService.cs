using IntentDK.Core.Models;
using IntentDK.Core.Parsing;
using IntentDK.Core.Templates;

namespace IntentDK.Core.Services;

/// <summary>
/// Service for managing intent files in a project.
/// </summary>
public class IntentFileService
{
    private readonly IntentParser _parser;
    
    /// <summary>
    /// Default directory for intent files.
    /// </summary>
    public const string DefaultIntentDirectory = ".intent";
    
    /// <summary>
    /// File extension for intent files.
    /// </summary>
    public const string IntentFileExtension = ".intent.yaml";
    
    /// <summary>
    /// File extension for plan files.
    /// </summary>
    public const string PlanFileExtension = ".plan.yaml";
    
    /// <summary>
    /// File extension for tasks files.
    /// </summary>
    public const string TasksFileExtension = ".tasks.yaml";

    public IntentFileService()
    {
        _parser = new IntentParser();
    }

    /// <summary>
    /// Creates a new intent file with a template.
    /// </summary>
    /// <param name="directory">Directory to create the file in.</param>
    /// <param name="name">Name for the intent (used in filename).</param>
    /// <param name="template">Template type to use.</param>
    /// <param name="hint">Optional hint for the goal.</param>
    /// <returns>Path to the created file.</returns>
    public string CreateIntentFile(
        string directory, 
        string? name = null, 
        IntentTemplateType template = IntentTemplateType.Basic,
        string? hint = null)
    {
        var fileName = GenerateFileName(name);
        var filePath = Path.Combine(directory, fileName);
        
        var content = template switch
        {
            IntentTemplateType.Feature => IntentTemplates.GetFeatureTemplate(hint),
            IntentTemplateType.BugFix => IntentTemplates.GetBugFixTemplate(hint),
            IntentTemplateType.Refactor => IntentTemplates.GetRefactorTemplate(hint),
            IntentTemplateType.Security => IntentTemplates.GetSecurityTemplate(hint),
            _ => IntentTemplates.GetBasicTemplate(hint)
        };

        // Ensure directory exists
        Directory.CreateDirectory(directory);
        
        File.WriteAllText(filePath, content);
        
        return filePath;
    }

    /// <summary>
    /// Creates a new intent file in the default .intent directory.
    /// </summary>
    public string CreateIntentFileInProject(
        string projectRoot,
        string? name = null,
        IntentTemplateType template = IntentTemplateType.Basic,
        string? hint = null)
    {
        var intentDir = Path.Combine(projectRoot, DefaultIntentDirectory);
        return CreateIntentFile(intentDir, name, template, hint);
    }

    /// <summary>
    /// Reads and parses an intent from a file.
    /// </summary>
    public ParseResult<Intent> ReadIntent(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return ParseResult<Intent>.Failure(new List<string> 
            { 
                $"Intent file not found: {filePath}" 
            });
        }

        var content = File.ReadAllText(filePath);
        return _parser.ParseAndValidate(content);
    }

    /// <summary>
    /// Finds the most recent intent file in a directory.
    /// </summary>
    public string? FindLatestIntentFile(string directory)
    {
        if (!Directory.Exists(directory))
            return null;

        return Directory.GetFiles(directory, $"*{IntentFileExtension}")
            .OrderByDescending(f => File.GetLastWriteTime(f))
            .FirstOrDefault();
    }

    /// <summary>
    /// Finds an intent file in the project.
    /// Searches in .intent directory and project root.
    /// </summary>
    public string? FindIntentFile(string projectRoot, string? name = null)
    {
        var searchPaths = new[]
        {
            Path.Combine(projectRoot, DefaultIntentDirectory),
            projectRoot
        };

        foreach (var searchPath in searchPaths)
        {
            if (!Directory.Exists(searchPath))
                continue;

            // If name is specified, look for that specific file
            if (!string.IsNullOrEmpty(name))
            {
                var specificFile = Path.Combine(searchPath, $"{name}{IntentFileExtension}");
                if (File.Exists(specificFile))
                    return specificFile;
            }
            
            // Otherwise find the latest
            var latest = FindLatestIntentFile(searchPath);
            if (latest != null)
                return latest;
        }

        // Also check for intent.yaml in root
        var rootIntent = Path.Combine(projectRoot, "intent.yaml");
        if (File.Exists(rootIntent))
            return rootIntent;

        return null;
    }

    /// <summary>
    /// Creates a plan file for an intent.
    /// </summary>
    public string CreatePlanFile(string intentFilePath, string planContent)
    {
        var planPath = GetAssociatedFilePath(intentFilePath, PlanFileExtension);
        File.WriteAllText(planPath, planContent);
        return planPath;
    }

    /// <summary>
    /// Creates a tasks file for an intent.
    /// </summary>
    public string CreateTasksFile(string intentFilePath, string tasksContent)
    {
        var tasksPath = GetAssociatedFilePath(intentFilePath, TasksFileExtension);
        File.WriteAllText(tasksPath, tasksContent);
        return tasksPath;
    }

    /// <summary>
    /// Gets all intent files in a project.
    /// </summary>
    public IEnumerable<IntentFileInfo> GetAllIntents(string projectRoot)
    {
        var results = new List<IntentFileInfo>();
        
        var searchPaths = new[]
        {
            Path.Combine(projectRoot, DefaultIntentDirectory),
            projectRoot
        };

        foreach (var searchPath in searchPaths)
        {
            if (!Directory.Exists(searchPath))
                continue;

            foreach (var file in Directory.GetFiles(searchPath, $"*{IntentFileExtension}"))
            {
                var parseResult = ReadIntent(file);
                results.Add(new IntentFileInfo
                {
                    FilePath = file,
                    FileName = Path.GetFileName(file),
                    Intent = parseResult.IsSuccess ? parseResult.Value : null,
                    HasPlan = File.Exists(GetAssociatedFilePath(file, PlanFileExtension)),
                    HasTasks = File.Exists(GetAssociatedFilePath(file, TasksFileExtension)),
                    LastModified = File.GetLastWriteTime(file)
                });
            }
        }

        return results.OrderByDescending(i => i.LastModified);
    }

    /// <summary>
    /// Gets the path for an associated file (plan, tasks).
    /// </summary>
    public string GetAssociatedFilePath(string intentFilePath, string extension)
    {
        var baseName = Path.GetFileName(intentFilePath)
            .Replace(IntentFileExtension, "")
            .Replace(".yaml", "");
        
        var directory = Path.GetDirectoryName(intentFilePath) ?? ".";
        return Path.Combine(directory, $"{baseName}{extension}");
    }

    private string GenerateFileName(string? name)
    {
        if (!string.IsNullOrEmpty(name))
        {
            // Sanitize the name
            var sanitized = string.Join("_", name.Split(Path.GetInvalidFileNameChars()));
            return $"{sanitized}{IntentFileExtension}";
        }

        // Generate timestamp-based name
        var timestamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
        return $"intent-{timestamp}{IntentFileExtension}";
    }
}

/// <summary>
/// Template types for intent files.
/// </summary>
public enum IntentTemplateType
{
    Basic,
    Feature,
    BugFix,
    Refactor,
    Security
}

/// <summary>
/// Information about an intent file.
/// </summary>
public class IntentFileInfo
{
    public string FilePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public Intent? Intent { get; set; }
    public bool HasPlan { get; set; }
    public bool HasTasks { get; set; }
    public DateTime LastModified { get; set; }
}
