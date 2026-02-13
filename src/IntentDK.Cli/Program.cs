using IntentDK.Cli.AiTools;
using IntentDK.Core.Services;

namespace IntentDK.Cli;

class Program
{
    static int Main(string[] args)
    {
        if (args.Length == 0)
        {
            ShowHelp();
            return 0;
        }

        var command = args[0].ToLowerInvariant();

        return command switch
        {
            "init" => HandleInit(args.Skip(1).ToArray()),
            "new" => HandleNew(args.Skip(1).ToArray()),
            "list" or "ls" => HandleList(args.Skip(1).ToArray()),
            "help" or "--help" or "-h" => ShowHelp(),
            "version" or "--version" or "-v" => ShowVersion(),
            _ => UnknownCommand(command)
        };
    }

    static int ShowHelp()
    {
        Console.WriteLine(@"
Intent-Driven Development Kit (IntentDK) CLI

Usage: intent <command> [options]

Commands:
  init [path]        Initialize IntentDK in a project
  new [type]         Create a new intent file
  list               List all intent files

Options for 'init':
  --tool, -t <name>  AI tool to configure (cursor, copilot, cody, continue, aider, windsurf, claude)
  --all, -a          Install for all AI tools
  --force, -f        Overwrite existing files

Options for 'new':
  <type>             Intent type: feature, bugfix, refactor, security
  --name, -n <name>  Custom name for the intent file
  --hint <text>      Hint for the goal

Examples:
  intent init .                    Initialize with interactive selection
  intent init . --tool cursor      Initialize for Cursor
  intent init . --all              Initialize for all AI tools
  intent new feature               Create feature intent
  intent new bugfix                Create bugfix intent
  intent list                      List all intents

For more info: https://github.com/your-org/Intent-driven-development-kit
");
        return 0;
    }

    static int ShowVersion()
    {
        Console.WriteLine("IntentDK CLI v1.0.0");
        return 0;
    }

    static int UnknownCommand(string command)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Unknown command: {command}");
        Console.ResetColor();
        Console.WriteLine("Run 'intent help' for usage information.");
        return 1;
    }

    static int HandleInit(string[] args)
    {
        var path = ".";
        string? tool = null;
        var all = false;
        var force = false;

        for (int i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            
            if (arg == "--tool" || arg == "-t")
            {
                if (i + 1 < args.Length)
                    tool = args[++i];
            }
            else if (arg == "--all" || arg == "-a")
            {
                all = true;
            }
            else if (arg == "--force" || arg == "-f")
            {
                force = true;
            }
            else if (!arg.StartsWith("-"))
            {
                path = arg;
            }
        }

        return InitCommand.Execute(path, tool, all, force);
    }

    static int HandleNew(string[] args)
    {
        string? type = null;
        string? name = null;
        string? hint = null;
        var path = ".";

        for (int i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            
            if (arg == "--name" || arg == "-n")
            {
                if (i + 1 < args.Length)
                    name = args[++i];
            }
            else if (arg == "--hint" || arg == "-h")
            {
                if (i + 1 < args.Length)
                    hint = args[++i];
            }
            else if (arg == "--path" || arg == "-p")
            {
                if (i + 1 < args.Length)
                    path = args[++i];
            }
            else if (!arg.StartsWith("-") && type == null)
            {
                type = arg;
            }
        }

        return NewCommand.Execute(type, name, hint, path);
    }

    static int HandleList(string[] args)
    {
        var path = ".";

        for (int i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            
            if (arg == "--path" || arg == "-p")
            {
                if (i + 1 < args.Length)
                    path = args[++i];
            }
            else if (!arg.StartsWith("-"))
            {
                path = arg;
            }
        }

        return ListCommand.Execute(path);
    }
}

static class InitCommand
{
    public static int Execute(string path, string? tool, bool all, bool force)
    {
        var projectPath = Path.GetFullPath(path);

        if (!Directory.Exists(projectPath))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error: Directory not found: {projectPath}");
            Console.ResetColor();
            return 1;
        }

        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔══════════════════════════════════════════════════════════╗");
        Console.WriteLine("║        Intent-Driven Development Kit (IntentDK)          ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════╝");
        Console.ResetColor();
        Console.WriteLine();
        Console.WriteLine($"Initializing in: {projectPath}");
        Console.WriteLine();

        IReadOnlyList<AiToolConfig> toolsToInstall;

        if (all)
        {
            toolsToInstall = AiToolRegistry.All;
        }
        else if (!string.IsNullOrEmpty(tool))
        {
            var config = AiToolRegistry.GetById(tool);
            if (config == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Unknown tool: {tool}");
                Console.WriteLine();
                Console.ResetColor();
                ShowAvailableTools();
                return 1;
            }
            toolsToInstall = new[] { config };
        }
        else
        {
            // Interactive selection
            var selected = PromptForToolSelection();
            if (selected == null || selected.Count == 0)
            {
                Console.WriteLine("No tools selected. Exiting.");
                return 0;
            }
            toolsToInstall = selected;
        }

        // Create .intent directory
        var intentDir = Path.Combine(projectPath, ".intent");
        if (!Directory.Exists(intentDir))
        {
            Directory.CreateDirectory(intentDir);
            Console.WriteLine($"✓ Created .intent/ directory");
        }

        // Create .gitignore for .intent if needed
        var intentGitignore = Path.Combine(intentDir, ".gitignore");
        if (!File.Exists(intentGitignore))
        {
            File.WriteAllText(intentGitignore, @"# Uncomment to ignore intent files
# *.intent.yaml
# *.plan.yaml
# *.tasks.yaml
");
            Console.WriteLine($"✓ Created .intent/.gitignore");
        }

        // Install configurations for each tool
        foreach (var toolConfig in toolsToInstall)
        {
            InstallToolConfig(projectPath, toolConfig, force);
        }

        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("═══════════════════════════════════════════════════════════");
        Console.WriteLine("  IntentDK initialized successfully!");
        Console.WriteLine("═══════════════════════════════════════════════════════════");
        Console.ResetColor();
        Console.WriteLine();
        Console.WriteLine("Next steps:");
        Console.WriteLine("  1. Open your project in your AI-powered editor");
        Console.WriteLine("  2. Type /intent in the chat to create a new intent");
        Console.WriteLine("  3. Edit the YAML file with your goal, scope, and constraints");
        Console.WriteLine("  4. Use /intent.plan, /intent.tasks, /intent.implement");
        Console.WriteLine();
        Console.WriteLine("Commands:");
        Console.WriteLine("  /intent              Create new intent file");
        Console.WriteLine("  /intent.plan         Generate implementation plan");
        Console.WriteLine("  /intent.tasks        Break down into tasks");
        Console.WriteLine("  /intent.implement    Implement step by step");
        Console.WriteLine("  /intent.verify       Verify implementation");
        Console.WriteLine();

        return 0;
    }

    private static void InstallToolConfig(string projectPath, AiToolConfig tool, bool force)
    {
        var rulesDir = Path.Combine(projectPath, tool.RulesPath);
        var rulesFile = Path.Combine(rulesDir, tool.RulesFileName);

        if (File.Exists(rulesFile) && !force)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"⚠ Skipping {tool.Name}: {tool.RulesFileName} already exists (use --force to overwrite)");
            Console.ResetColor();
            return;
        }

        // Create directory if needed
        if (!Directory.Exists(rulesDir))
        {
            Directory.CreateDirectory(rulesDir);
        }

        // Write rules content
        var content = RuleTemplates.GetRuleContent(tool);
        File.WriteAllText(rulesFile, content);

        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write($"✓ ");
        Console.ResetColor();
        Console.WriteLine($"Installed {tool.Name} configuration: {Path.Combine(tool.RulesPath, tool.RulesFileName)}");

        if (!string.IsNullOrEmpty(tool.SetupInstructions))
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"  → {tool.SetupInstructions}");
            Console.ResetColor();
        }
    }

    private static List<AiToolConfig>? PromptForToolSelection()
    {
        Console.WriteLine("Which AI coding assistant(s) do you use?");
        Console.WriteLine();

        var tools = AiToolRegistry.All;
        for (int i = 0; i < tools.Count; i++)
        {
            Console.WriteLine($"  [{i + 1}] {tools[i].Name,-25} - {tools[i].Description}");
        }
        Console.WriteLine();
        Console.WriteLine($"  [A] All tools");
        Console.WriteLine($"  [Q] Quit");
        Console.WriteLine();

        Console.Write("Enter your choice (e.g., 1,2 or A): ");
        var input = Console.ReadLine()?.Trim();

        if (string.IsNullOrEmpty(input) || input.Equals("Q", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        if (input.Equals("A", StringComparison.OrdinalIgnoreCase))
        {
            return tools.ToList();
        }

        var selected = new List<AiToolConfig>();
        var parts = input.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var part in parts)
        {
            if (int.TryParse(part.Trim(), out int index) && index >= 1 && index <= tools.Count)
            {
                var toolItem = tools[index - 1];
                if (!selected.Contains(toolItem))
                {
                    selected.Add(toolItem);
                }
            }
            else
            {
                var toolItem = AiToolRegistry.GetById(part.Trim());
                if (toolItem != null && !selected.Contains(toolItem))
                {
                    selected.Add(toolItem);
                }
            }
        }

        return selected;
    }

    private static void ShowAvailableTools()
    {
        Console.WriteLine("Available AI tools:");
        foreach (var tool in AiToolRegistry.All)
        {
            Console.WriteLine($"  {tool.Id,-12} - {tool.Name} ({tool.Description})");
        }
    }
}

static class NewCommand
{
    public static int Execute(string? type, string? name, string? hint, string path)
    {
        var projectPath = Path.GetFullPath(path);
        var fileService = new IntentFileService();

        var templateType = type?.ToLowerInvariant() switch
        {
            "feature" => IntentTemplateType.Feature,
            "bugfix" or "bug" or "fix" => IntentTemplateType.BugFix,
            "refactor" => IntentTemplateType.Refactor,
            "security" or "sec" => IntentTemplateType.Security,
            _ => IntentTemplateType.Basic
        };

        try
        {
            var filePath = fileService.CreateIntentFileInProject(projectPath, name, templateType, hint);
            
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"✓ Created intent file: {Path.GetRelativePath(projectPath, filePath)}");
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine("Next steps:");
            Console.WriteLine("  1. Edit the file to define your intent");
            Console.WriteLine("  2. Use /intent.plan to generate a plan");
            Console.WriteLine("  3. Use /intent.implement to start implementation");
            return 0;
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error creating intent file: {ex.Message}");
            Console.ResetColor();
            return 1;
        }
    }
}

static class ListCommand
{
    public static int Execute(string path)
    {
        var projectPath = Path.GetFullPath(path);
        var fileService = new IntentFileService();

        var intents = fileService.GetAllIntents(projectPath).ToList();

        if (intents.Count == 0)
        {
            Console.WriteLine("No intent files found.");
            Console.WriteLine();
            Console.WriteLine("Create one with: intent new");
            return 0;
        }

        Console.WriteLine();
        Console.WriteLine($"Intent files in {projectPath}:");
        Console.WriteLine();
        Console.WriteLine("  File                                    Goal                                      Status");
        Console.WriteLine("  " + new string('-', 100));

        foreach (var info in intents)
        {
            var goal = info.Intent?.Goal ?? "(could not parse)";
            if (goal.Length > 40)
                goal = goal.Substring(0, 37) + "...";

            var status = new List<string>();
            if (info.HasPlan) status.Add("plan");
            if (info.HasTasks) status.Add("tasks");
            var statusStr = status.Count > 0 ? string.Join(", ", status) : "-";

            Console.Write("  ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($"{info.FileName,-40}");
            Console.ResetColor();
            Console.Write($"  {goal,-40}");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"  {statusStr}");
            Console.ResetColor();
        }

        Console.WriteLine();
        return 0;
    }
}
