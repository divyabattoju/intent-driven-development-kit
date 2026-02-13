using IntentDK.Core.Models;

namespace IntentDK.Core.Planning;

/// <summary>
/// Generates task breakdowns from intents.
/// </summary>
public class TaskGenerator
{
    private readonly TaskGeneratorOptions _options;

    public TaskGenerator() : this(new TaskGeneratorOptions()) { }

    public TaskGenerator(TaskGeneratorOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    /// Generates a task breakdown from an intent.
    /// </summary>
    public TaskBreakdown GenerateTasks(Intent intent)
    {
        if (intent == null)
            throw new ArgumentNullException(nameof(intent));

        var validation = intent.Validate();
        if (!validation.IsValid)
        {
            throw new InvalidOperationException(
                $"Cannot generate tasks for invalid intent: {string.Join(", ", validation.Errors)}");
        }

        var breakdown = new TaskBreakdown
        {
            IntentId = intent.Id,
            Goal = intent.Goal
        };

        var taskId = 1;
        var tasks = new List<ImplementationTask>();

        // Task 1: Analysis
        if (_options.IncludeAnalysisTask)
        {
            tasks.Add(new ImplementationTask
            {
                Id = $"T{taskId++}",
                Title = "Analyze Current State",
                Type = TaskType.Analyze,
                Description = $"Review the current implementation of: {string.Join(", ", intent.Scope)}",
                AcceptanceCriteria = new List<string>
                {
                    "Understand existing code structure",
                    "Identify integration points",
                    "Document any concerns or blockers"
                },
                Complexity = 1
            });
        }

        // Tasks for each scope item
        var analysisTaskId = tasks.FirstOrDefault()?.Id;
        foreach (var scopeItem in intent.Scope)
        {
            var task = new ImplementationTask
            {
                Id = $"T{taskId++}",
                Title = $"Implement changes in {scopeItem}",
                Type = DetermineTaskType(scopeItem, intent),
                Target = scopeItem,
                Description = $"Modify {scopeItem} to achieve: {intent.Goal}",
                AcceptanceCriteria = GenerateAcceptanceCriteria(intent, scopeItem),
                Complexity = EstimateComplexity(scopeItem, intent)
            };

            if (analysisTaskId != null)
            {
                task.DependsOn.Add(analysisTaskId);
            }

            tasks.Add(task);
        }

        // Get implementation task IDs for dependencies
        var implementationTaskIds = tasks
            .Where(t => t.Type == TaskType.Implement || t.Type == TaskType.Create)
            .Select(t => t.Id)
            .ToList();

        // Test tasks from verification criteria
        foreach (var verification in intent.Verification)
        {
            var testTask = new ImplementationTask
            {
                Id = $"T{taskId++}",
                Title = FormatVerificationTitle(verification),
                Type = DetermineVerificationType(verification),
                Description = verification,
                AcceptanceCriteria = new List<string>
                {
                    $"Verify: {verification}",
                    "All tests pass",
                    "No regressions"
                },
                DependsOn = implementationTaskIds.ToList(),
                Complexity = 2
            };
            tasks.Add(testTask);
        }

        // Constraint verification tasks
        if (_options.IncludeConstraintVerification)
        {
            foreach (var constraint in intent.Constraints)
            {
                tasks.Add(new ImplementationTask
                {
                    Id = $"T{taskId++}",
                    Title = $"Verify: {TruncateString(constraint, 40)}",
                    Type = TaskType.Verify,
                    Description = $"Ensure constraint is met: {constraint}",
                    AcceptanceCriteria = new List<string>
                    {
                        $"Constraint satisfied: {constraint}",
                        "Evidence documented"
                    },
                    DependsOn = implementationTaskIds.ToList(),
                    Complexity = 1
                });
            }
        }

        // Final review task
        if (_options.IncludeFinalReviewTask)
        {
            var allTaskIds = tasks.Select(t => t.Id).ToList();
            tasks.Add(new ImplementationTask
            {
                Id = $"T{taskId++}",
                Title = "Final Review",
                Type = TaskType.Review,
                Description = "Review all changes, ensure code quality, and verify all criteria are met",
                AcceptanceCriteria = new List<string>
                {
                    "All implementation tasks complete",
                    "All tests pass",
                    "All constraints verified",
                    "Code review complete"
                },
                DependsOn = allTaskIds,
                Complexity = 1
            });
        }

        breakdown.Tasks = tasks;
        breakdown.UpdateProgress();

        return breakdown;
    }

    /// <summary>
    /// Generates task breakdown as YAML.
    /// </summary>
    public string GenerateTasksYaml(Intent intent)
    {
        var breakdown = GenerateTasks(intent);
        return FormatAsYaml(breakdown);
    }

    /// <summary>
    /// Generates task breakdown as Markdown.
    /// </summary>
    public string GenerateTasksMarkdown(Intent intent)
    {
        var breakdown = GenerateTasks(intent);
        return FormatAsMarkdown(breakdown);
    }

    /// <summary>
    /// Generates task breakdown as a checklist.
    /// </summary>
    public string GenerateTasksChecklist(Intent intent)
    {
        var breakdown = GenerateTasks(intent);
        return FormatAsChecklist(breakdown);
    }

    private TaskType DetermineTaskType(string scopeItem, Intent intent)
    {
        var lower = scopeItem.ToLowerInvariant();
        
        if (lower.Contains("(new)") || lower.Contains("new "))
            return TaskType.Create;
        if (lower.Contains("test"))
            return TaskType.Test;
        if (lower.Contains("config") || lower.Contains("setting"))
            return TaskType.Configure;
        if (lower.Contains("doc") || lower.Contains("readme"))
            return TaskType.Document;
            
        return TaskType.Implement;
    }

    private TaskType DetermineVerificationType(string verification)
    {
        var lower = verification.ToLowerInvariant();
        
        if (lower.Contains("test"))
            return TaskType.Test;
        if (lower.Contains("review"))
            return TaskType.Review;
        if (lower.Contains("document"))
            return TaskType.Document;
            
        return TaskType.Verify;
    }

    private List<string> GenerateAcceptanceCriteria(Intent intent, string scopeItem)
    {
        var criteria = new List<string>
        {
            $"Changes to {scopeItem} complete",
            "Code compiles without errors"
        };

        // Add relevant constraints
        foreach (var constraint in intent.Constraints.Take(2))
        {
            criteria.Add($"Respects: {constraint}");
        }

        return criteria;
    }

    private int EstimateComplexity(string scopeItem, Intent intent)
    {
        var complexity = 2; // Base complexity
        
        // Increase for more constraints
        complexity += Math.Min(intent.Constraints.Count / 2, 2);
        
        // New files are typically more complex
        if (scopeItem.Contains("(new)"))
            complexity += 1;
            
        return Math.Min(complexity, 5);
    }

    private string FormatVerificationTitle(string verification)
    {
        var title = verification;
        
        // Capitalize first letter
        if (!string.IsNullOrEmpty(title))
        {
            title = char.ToUpper(title[0]) + title.Substring(1);
        }
        
        return TruncateString(title, 50);
    }

    private string TruncateString(string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value))
            return value;
        
        return value.Length <= maxLength 
            ? value 
            : value.Substring(0, maxLength - 3) + "...";
    }

    private string FormatAsYaml(TaskBreakdown breakdown)
    {
        var sb = new System.Text.StringBuilder();
        
        sb.AppendLine($"# Task Breakdown");
        sb.AppendLine($"# Intent: {breakdown.IntentId}");
        sb.AppendLine($"# Goal: {breakdown.Goal}");
        sb.AppendLine();
        sb.AppendLine($"intent_id: {breakdown.IntentId}");
        sb.AppendLine($"goal: {breakdown.Goal}");
        sb.AppendLine();
        sb.AppendLine("tasks:");
        
        foreach (var task in breakdown.Tasks)
        {
            sb.AppendLine($"  - id: {task.Id}");
            sb.AppendLine($"    title: \"{task.Title}\"");
            sb.AppendLine($"    type: {task.Type.ToString().ToLower()}");
            sb.AppendLine($"    status: {task.Status.ToString().ToLower()}");
            if (!string.IsNullOrEmpty(task.Target))
                sb.AppendLine($"    target: {task.Target}");
            sb.AppendLine($"    description: |");
            sb.AppendLine($"      {task.Description}");
            if (task.AcceptanceCriteria.Any())
            {
                sb.AppendLine($"    acceptance_criteria:");
                foreach (var ac in task.AcceptanceCriteria)
                    sb.AppendLine($"      - {ac}");
            }
            if (task.DependsOn.Any())
            {
                sb.AppendLine($"    depends_on: [{string.Join(", ", task.DependsOn)}]");
            }
            sb.AppendLine($"    complexity: {task.Complexity}");
            sb.AppendLine();
        }

        sb.AppendLine("progress:");
        sb.AppendLine($"  completed: {breakdown.Progress.Completed}");
        sb.AppendLine($"  in_progress: {breakdown.Progress.InProgress}");
        sb.AppendLine($"  pending: {breakdown.Progress.Pending}");
        sb.AppendLine($"  total: {breakdown.Progress.Total}");
        sb.AppendLine($"  percentage: {breakdown.Progress.Percentage}%");

        return sb.ToString();
    }

    private string FormatAsMarkdown(TaskBreakdown breakdown)
    {
        var sb = new System.Text.StringBuilder();
        
        sb.AppendLine("# Task Breakdown");
        sb.AppendLine();
        sb.AppendLine($"**Goal:** {breakdown.Goal}");
        sb.AppendLine($"**Intent ID:** `{breakdown.IntentId}`");
        sb.AppendLine($"**Progress:** {breakdown.Progress.Completed}/{breakdown.Progress.Total} ({breakdown.Progress.Percentage}%)");
        sb.AppendLine();
        sb.AppendLine("## Tasks");
        sb.AppendLine();
        sb.AppendLine("| ID | Type | Title | Target | Status | Complexity |");
        sb.AppendLine("|----|------|-------|--------|--------|------------|");
        
        foreach (var task in breakdown.Tasks)
        {
            var typeIcon = GetTypeIcon(task.Type);
            var statusIcon = GetStatusIcon(task.Status);
            var target = task.Target ?? "-";
            sb.AppendLine($"| {task.Id} | {typeIcon} {task.Type} | {task.Title} | `{target}` | {statusIcon} | {"★".PadRight(task.Complexity, '★')} |");
        }

        sb.AppendLine();
        sb.AppendLine("## Task Details");
        sb.AppendLine();
        
        foreach (var task in breakdown.Tasks)
        {
            sb.AppendLine($"### {task.Id}: {task.Title}");
            sb.AppendLine();
            sb.AppendLine($"**Type:** {task.Type} | **Status:** {task.Status} | **Complexity:** {task.Complexity}/5");
            if (!string.IsNullOrEmpty(task.Target))
                sb.AppendLine($"**Target:** `{task.Target}`");
            sb.AppendLine();
            sb.AppendLine($"{task.Description}");
            sb.AppendLine();
            if (task.AcceptanceCriteria.Any())
            {
                sb.AppendLine("**Acceptance Criteria:**");
                foreach (var ac in task.AcceptanceCriteria)
                    sb.AppendLine($"- [ ] {ac}");
                sb.AppendLine();
            }
            if (task.DependsOn.Any())
            {
                sb.AppendLine($"**Depends on:** {string.Join(", ", task.DependsOn)}");
                sb.AppendLine();
            }
        }

        return sb.ToString();
    }

    private string FormatAsChecklist(TaskBreakdown breakdown)
    {
        var sb = new System.Text.StringBuilder();
        
        sb.AppendLine($"## Tasks: {breakdown.Goal}");
        sb.AppendLine();
        sb.AppendLine($"Progress: {breakdown.Progress.Completed}/{breakdown.Progress.Total}");
        sb.AppendLine();
        
        foreach (var task in breakdown.Tasks)
        {
            var checkbox = task.Status == Models.TaskStatus.Completed ? "[x]" : "[ ]";
            var target = task.Target != null ? $" ({task.Target})" : "";
            sb.AppendLine($"- {checkbox} **{task.Id}**: {task.Title}{target}");
        }

        return sb.ToString();
    }

    private string GetTypeIcon(TaskType type)
    {
        return type switch
        {
            TaskType.Analyze => "🔍",
            TaskType.Design => "📐",
            TaskType.Create => "➕",
            TaskType.Implement => "✏️",
            TaskType.Test => "🧪",
            TaskType.Review => "👀",
            TaskType.Document => "📝",
            TaskType.Configure => "⚙️",
            TaskType.Verify => "✓",
            _ => "▶️"
        };
    }

    private string GetStatusIcon(Models.TaskStatus status)
    {
        return status switch
        {
            Models.TaskStatus.Pending => "⏳",
            Models.TaskStatus.InProgress => "🔄",
            Models.TaskStatus.Completed => "✅",
            Models.TaskStatus.Blocked => "🚫",
            Models.TaskStatus.Skipped => "⏭️",
            _ => "⏳"
        };
    }
}

/// <summary>
/// Options for task generation.
/// </summary>
public class TaskGeneratorOptions
{
    /// <summary>
    /// Include an initial analysis task.
    /// </summary>
    public bool IncludeAnalysisTask { get; set; } = true;

    /// <summary>
    /// Include constraint verification tasks.
    /// </summary>
    public bool IncludeConstraintVerification { get; set; } = true;

    /// <summary>
    /// Include a final review task.
    /// </summary>
    public bool IncludeFinalReviewTask { get; set; } = true;
}
