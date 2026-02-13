namespace IntentDK.Cli.AiTools;

/// <summary>
/// Represents an AI coding assistant tool configuration.
/// </summary>
public class AiToolConfig
{
    /// <summary>
    /// Display name of the AI tool.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Short identifier.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Description of the tool.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Relative path where rules should be created.
    /// </summary>
    public string RulesPath { get; set; } = string.Empty;

    /// <summary>
    /// Filename for the rules file.
    /// </summary>
    public string RulesFileName { get; set; } = string.Empty;

    /// <summary>
    /// Whether this tool uses markdown format.
    /// </summary>
    public bool UsesMarkdown { get; set; } = true;

    /// <summary>
    /// Additional setup instructions.
    /// </summary>
    public string? SetupInstructions { get; set; }
}

/// <summary>
/// Registry of supported AI tools.
/// </summary>
public static class AiToolRegistry
{
    public static readonly AiToolConfig Cursor = new()
    {
        Id = "cursor",
        Name = "Cursor",
        Description = "Cursor AI-powered code editor",
        RulesPath = ".cursor/rules",
        RulesFileName = "intent-command.mdc",
        UsesMarkdown = true,
        SetupInstructions = "Rules will be automatically loaded when you open the project in Cursor."
    };

    public static readonly AiToolConfig GitHubCopilot = new()
    {
        Id = "copilot",
        Name = "GitHub Copilot",
        Description = "GitHub Copilot AI assistant",
        RulesPath = ".github",
        RulesFileName = "copilot-instructions.md",
        UsesMarkdown = true,
        SetupInstructions = "Instructions will be used by Copilot Chat when working in this repository."
    };

    public static readonly AiToolConfig Cody = new()
    {
        Id = "cody",
        Name = "Sourcegraph Cody",
        Description = "Sourcegraph Cody AI assistant",
        RulesPath = ".sourcegraph",
        RulesFileName = "cody.json",
        UsesMarkdown = false,
        SetupInstructions = "Cody will use these instructions when working in this repository."
    };

    public static readonly AiToolConfig Continue = new()
    {
        Id = "continue",
        Name = "Continue",
        Description = "Continue open-source AI assistant",
        RulesPath = ".continue",
        RulesFileName = "rules.md",
        UsesMarkdown = true,
        SetupInstructions = "Continue will load these rules when you open the project."
    };

    public static readonly AiToolConfig Aider = new()
    {
        Id = "aider",
        Name = "Aider",
        Description = "Aider AI pair programming",
        RulesPath = ".",
        RulesFileName = ".aider.conf.yml",
        UsesMarkdown = false,
        SetupInstructions = "Aider will use conventions from this file."
    };

    public static readonly AiToolConfig Windsurf = new()
    {
        Id = "windsurf",
        Name = "Windsurf (Codeium)",
        Description = "Windsurf AI-powered IDE by Codeium",
        RulesPath = ".windsurf/rules",
        RulesFileName = "intent-command.md",
        UsesMarkdown = true,
        SetupInstructions = "Rules will be loaded when you open the project in Windsurf."
    };

    public static readonly AiToolConfig Claude = new()
    {
        Id = "claude",
        Name = "Claude (AGENTS.md)",
        Description = "Claude with AGENTS.md project instructions",
        RulesPath = ".",
        RulesFileName = "AGENTS.md",
        UsesMarkdown = true,
        SetupInstructions = "Claude will read AGENTS.md for project-specific instructions."
    };

    public static readonly AiToolConfig Generic = new()
    {
        Id = "generic",
        Name = "Generic (README)",
        Description = "Generic instructions in project README",
        RulesPath = ".",
        RulesFileName = "AI_INSTRUCTIONS.md",
        UsesMarkdown = true,
        SetupInstructions = "Copy these instructions to your AI assistant as needed."
    };

    /// <summary>
    /// Gets all supported AI tools.
    /// </summary>
    public static IReadOnlyList<AiToolConfig> All => new[]
    {
        Cursor,
        GitHubCopilot,
        Cody,
        Continue,
        Aider,
        Windsurf,
        Claude,
        Generic
    };

    /// <summary>
    /// Gets a tool by ID.
    /// </summary>
    public static AiToolConfig? GetById(string id)
    {
        return All.FirstOrDefault(t => 
            t.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
    }
}
