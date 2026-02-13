using IntentDK.Core.Models;

namespace IntentDK.Core.Planning;

/// <summary>
/// Generates implementation plans from intents.
/// This provides a structured template that AI agents can use to implement intents.
/// </summary>
public class PlanGenerator
{
    private readonly PlanGeneratorOptions _options;

    public PlanGenerator() : this(new PlanGeneratorOptions()) { }

    public PlanGenerator(PlanGeneratorOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    /// Generates a plan template from an intent.
    /// </summary>
    /// <param name="intent">The intent to plan for.</param>
    /// <returns>A structured plan with steps.</returns>
    public Plan GeneratePlan(Intent intent)
    {
        if (intent == null)
        {
            throw new ArgumentNullException(nameof(intent));
        }

        var validation = intent.Validate();
        if (!validation.IsValid)
        {
            throw new InvalidOperationException(
                $"Cannot generate plan for invalid intent: {string.Join(", ", validation.Errors)}");
        }

        var plan = new Plan
        {
            IntentId = intent.Id,
            Summary = GenerateSummary(intent),
            AffectedFiles = GenerateAffectedFilesList(intent),
            Risks = GenerateRisks(intent),
            Dependencies = new List<string>()
        };

        // Generate steps based on the intent
        var steps = new List<PlanStep>();
        var stepNumber = 1;

        // Step 1: Analysis
        if (_options.IncludeAnalysisStep)
        {
            steps.Add(new PlanStep
            {
                StepNumber = stepNumber++,
                Action = StepAction.Review,
                Description = "Analyze current implementation",
                Details = $"Review the current state of: {string.Join(", ", intent.Scope)}",
                ExpectedOutcome = "Understanding of existing code structure and patterns"
            });
        }

        // Steps for each scope item
        foreach (var scopeItem in intent.Scope)
        {
            steps.Add(new PlanStep
            {
                StepNumber = stepNumber++,
                Action = StepAction.Modify,
                Target = scopeItem,
                Description = $"Implement changes in {scopeItem}",
                Details = GenerateStepDetails(intent, scopeItem),
                ExpectedOutcome = $"{scopeItem} updated to achieve: {intent.Goal}"
            });
        }

        // Add verification steps
        foreach (var verification in intent.Verification)
        {
            var verificationStep = CreateVerificationStep(verification, stepNumber++);
            steps.Add(verificationStep);
        }

        // Final review step
        if (_options.IncludeReviewStep)
        {
            steps.Add(new PlanStep
            {
                StepNumber = stepNumber++,
                Action = StepAction.Review,
                Description = "Final review and cleanup",
                Details = "Review all changes, ensure code quality, and verify constraints are met",
                ExpectedOutcome = "Clean, tested implementation ready for commit"
            });
        }

        plan.Steps = steps;
        plan.Status = PlanStatus.Ready;

        return plan;
    }

    /// <summary>
    /// Generates a plan as a formatted string suitable for AI agent consumption.
    /// </summary>
    public string GeneratePlanText(Intent intent)
    {
        var plan = GeneratePlan(intent);
        return FormatPlanAsText(plan, intent);
    }

    /// <summary>
    /// Generates a plan as markdown.
    /// </summary>
    public string GeneratePlanMarkdown(Intent intent)
    {
        var plan = GeneratePlan(intent);
        return FormatPlanAsMarkdown(plan, intent);
    }

    private string GenerateSummary(Intent intent)
    {
        return $"Implementation plan for: {intent.Goal}";
    }

    private List<string> GenerateAffectedFilesList(Intent intent)
    {
        // This would be enhanced by actual code analysis
        // For now, we return scope items as potential files
        return intent.Scope.Select(s => $"{s}.*").ToList();
    }

    private List<string> GenerateRisks(Intent intent)
    {
        var risks = new List<string>();

        // Generate risks based on constraints
        foreach (var constraint in intent.Constraints)
        {
            risks.Add($"Must ensure: {constraint}");
        }

        // Add scope-based risks
        if (intent.Scope.Count > 3)
        {
            risks.Add("Large scope - consider breaking into smaller intents");
        }

        return risks;
    }

    private string GenerateStepDetails(Intent intent, string scopeItem)
    {
        var details = $"Modify {scopeItem} to achieve: {intent.Goal}\n\n";
        
        if (intent.Constraints.Any())
        {
            details += "Constraints to respect:\n";
            foreach (var constraint in intent.Constraints)
            {
                details += $"  - {constraint}\n";
            }
        }

        return details;
    }

    private PlanStep CreateVerificationStep(string verification, int stepNumber)
    {
        var action = StepAction.Test;
        var description = verification;

        // Determine verification type from content
        if (verification.Contains("test", StringComparison.OrdinalIgnoreCase))
        {
            action = StepAction.Test;
            description = $"Create/run: {verification}";
        }
        else if (verification.Contains("review", StringComparison.OrdinalIgnoreCase))
        {
            action = StepAction.Review;
        }
        else if (verification.Contains("document", StringComparison.OrdinalIgnoreCase))
        {
            action = StepAction.Document;
        }

        return new PlanStep
        {
            StepNumber = stepNumber,
            Action = action,
            Description = description,
            ExpectedOutcome = $"Verified: {verification}"
        };
    }

    private string FormatPlanAsText(Plan plan, Intent intent)
    {
        var text = new System.Text.StringBuilder();
        
        text.AppendLine("=" .PadRight(60, '='));
        text.AppendLine($"IMPLEMENTATION PLAN");
        text.AppendLine("=".PadRight(60, '='));
        text.AppendLine();
        text.AppendLine($"Goal: {intent.Goal}");
        text.AppendLine($"Plan ID: {plan.Id}");
        text.AppendLine();
        
        text.AppendLine("SCOPE:");
        foreach (var scope in intent.Scope)
        {
            text.AppendLine($"  - {scope}");
        }
        text.AppendLine();

        if (intent.Constraints.Any())
        {
            text.AppendLine("CONSTRAINTS:");
            foreach (var constraint in intent.Constraints)
            {
                text.AppendLine($"  - {constraint}");
            }
            text.AppendLine();
        }

        text.AppendLine("STEPS:");
        text.AppendLine("-".PadRight(60, '-'));
        
        foreach (var step in plan.Steps)
        {
            text.AppendLine($"\n[Step {step.StepNumber}] {step.Action}: {step.Description}");
            if (!string.IsNullOrEmpty(step.Target))
            {
                text.AppendLine($"  Target: {step.Target}");
            }
            if (!string.IsNullOrEmpty(step.Details))
            {
                text.AppendLine($"  Details: {step.Details.Trim()}");
            }
            if (!string.IsNullOrEmpty(step.ExpectedOutcome))
            {
                text.AppendLine($"  Expected: {step.ExpectedOutcome}");
            }
        }

        text.AppendLine();
        text.AppendLine("=".PadRight(60, '='));
        text.AppendLine("VERIFICATION CRITERIA:");
        foreach (var v in intent.Verification)
        {
            text.AppendLine($"  [ ] {v}");
        }

        return text.ToString();
    }

    private string FormatPlanAsMarkdown(Plan plan, Intent intent)
    {
        var md = new System.Text.StringBuilder();
        
        md.AppendLine($"# Implementation Plan");
        md.AppendLine();
        md.AppendLine($"**Goal:** {intent.Goal}");
        md.AppendLine($"**Plan ID:** `{plan.Id}`");
        md.AppendLine($"**Intent ID:** `{intent.Id}`");
        md.AppendLine();
        
        md.AppendLine("## Scope");
        foreach (var scope in intent.Scope)
        {
            md.AppendLine($"- `{scope}`");
        }
        md.AppendLine();

        if (intent.Constraints.Any())
        {
            md.AppendLine("## Constraints");
            foreach (var constraint in intent.Constraints)
            {
                md.AppendLine($"- ⚠️ {constraint}");
            }
            md.AppendLine();
        }

        if (plan.Risks.Any())
        {
            md.AppendLine("## Risks");
            foreach (var risk in plan.Risks)
            {
                md.AppendLine($"- {risk}");
            }
            md.AppendLine();
        }

        md.AppendLine("## Implementation Steps");
        md.AppendLine();
        
        foreach (var step in plan.Steps)
        {
            var icon = step.Action switch
            {
                StepAction.Create => "➕",
                StepAction.Modify => "✏️",
                StepAction.Delete => "🗑️",
                StepAction.Test => "🧪",
                StepAction.Review => "👀",
                StepAction.Configure => "⚙️",
                StepAction.Document => "📝",
                _ => "▶️"
            };
            
            md.AppendLine($"### Step {step.StepNumber}: {step.Description}");
            md.AppendLine();
            md.AppendLine($"- **Action:** {icon} {step.Action}");
            if (!string.IsNullOrEmpty(step.Target))
            {
                md.AppendLine($"- **Target:** `{step.Target}`");
            }
            if (!string.IsNullOrEmpty(step.Details))
            {
                md.AppendLine($"- **Details:** {step.Details.Trim()}");
            }
            if (!string.IsNullOrEmpty(step.ExpectedOutcome))
            {
                md.AppendLine($"- **Expected Outcome:** {step.ExpectedOutcome}");
            }
            md.AppendLine();
        }

        md.AppendLine("## Verification Checklist");
        foreach (var v in intent.Verification)
        {
            md.AppendLine($"- [ ] {v}");
        }

        return md.ToString();
    }
}

/// <summary>
/// Options for plan generation.
/// </summary>
public class PlanGeneratorOptions
{
    /// <summary>
    /// Include an initial analysis/review step.
    /// </summary>
    public bool IncludeAnalysisStep { get; set; } = true;

    /// <summary>
    /// Include a final review step.
    /// </summary>
    public bool IncludeReviewStep { get; set; } = true;

    /// <summary>
    /// Maximum number of steps to generate.
    /// </summary>
    public int MaxSteps { get; set; } = 20;
}
