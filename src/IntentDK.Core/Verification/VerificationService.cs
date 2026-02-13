using IntentDK.Core.Models;

namespace IntentDK.Core.Verification;

/// <summary>
/// Service for verifying that intent implementations meet their criteria.
/// </summary>
public class VerificationService
{
    private readonly List<IVerificationStrategy> _strategies;

    public VerificationService()
    {
        _strategies = new List<IVerificationStrategy>
        {
            new TestVerificationStrategy(),
            new ConstraintVerificationStrategy(),
            new ManualVerificationStrategy()
        };
    }

    /// <summary>
    /// Creates a verification checklist from an intent.
    /// </summary>
    public VerificationChecklist CreateChecklist(Intent intent)
    {
        if (intent == null)
        {
            throw new ArgumentNullException(nameof(intent));
        }

        var checklist = new VerificationChecklist
        {
            IntentId = intent.Id,
            Goal = intent.Goal
        };

        // Add verification items from intent
        foreach (var criterion in intent.Verification)
        {
            var item = new ChecklistItem
            {
                Id = Guid.NewGuid().ToString("N")[..8],
                Criterion = criterion,
                Type = DetermineVerificationType(criterion),
                Status = ChecklistItemStatus.Pending
            };
            checklist.Items.Add(item);
        }

        // Add implicit constraint checks
        foreach (var constraint in intent.Constraints)
        {
            var item = new ChecklistItem
            {
                Id = Guid.NewGuid().ToString("N")[..8],
                Criterion = $"Constraint: {constraint}",
                Type = VerificationType.CodeReview,
                Status = ChecklistItemStatus.Pending,
                IsConstraint = true
            };
            checklist.Items.Add(item);
        }

        return checklist;
    }

    /// <summary>
    /// Evaluates verification results and generates a summary.
    /// </summary>
    public VerificationResult Evaluate(VerificationChecklist checklist)
    {
        var checks = checklist.Items.Select(item => new VerificationCheck
        {
            Name = item.Criterion,
            Criterion = item.Criterion,
            Passed = item.Status == ChecklistItemStatus.Passed,
            Message = item.Notes,
            Type = item.Type
        }).ToList();

        var passedCount = checks.Count(c => c.Passed);
        var totalCount = checks.Count;
        var allPassed = passedCount == totalCount;

        var result = new VerificationResult
        {
            IntentId = checklist.IntentId,
            Status = allPassed ? VerificationStatus.Passed : VerificationStatus.Failed,
            Checks = checks,
            Summary = allPassed 
                ? $"All {totalCount} verification criteria passed."
                : $"{passedCount}/{totalCount} verification criteria passed."
        };

        // Add suggestions for failed checks
        foreach (var failed in checks.Where(c => !c.Passed))
        {
            result.Suggestions.Add($"Address: {failed.Criterion}");
        }

        return result;
    }

    /// <summary>
    /// Generates verification report in text format.
    /// </summary>
    public string GenerateReport(VerificationResult result, ReportFormat format = ReportFormat.Text)
    {
        return format switch
        {
            ReportFormat.Markdown => GenerateMarkdownReport(result),
            ReportFormat.Json => GenerateJsonReport(result),
            _ => GenerateTextReport(result)
        };
    }

    private VerificationType DetermineVerificationType(string criterion)
    {
        var lower = criterion.ToLowerInvariant();
        
        if (lower.Contains("unit test"))
            return VerificationType.UnitTest;
        if (lower.Contains("integration test"))
            return VerificationType.IntegrationTest;
        if (lower.Contains("test"))
            return VerificationType.UnitTest;
        if (lower.Contains("review") || lower.Contains("check"))
            return VerificationType.CodeReview;
        if (lower.Contains("lint"))
            return VerificationType.Linter;
        if (lower.Contains("build") || lower.Contains("compile"))
            return VerificationType.Build;
        if (lower.Contains("security") || lower.Contains("vulnerability"))
            return VerificationType.Security;
            
        return VerificationType.Manual;
    }

    private string GenerateTextReport(VerificationResult result)
    {
        var sb = new System.Text.StringBuilder();
        
        sb.AppendLine("=".PadRight(60, '='));
        sb.AppendLine("VERIFICATION REPORT");
        sb.AppendLine("=".PadRight(60, '='));
        sb.AppendLine();
        sb.AppendLine($"Intent ID: {result.IntentId}");
        sb.AppendLine($"Status: {result.Status}");
        sb.AppendLine($"Verified At: {result.VerifiedAt:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine();
        sb.AppendLine($"Summary: {result.Summary}");
        sb.AppendLine();
        sb.AppendLine("-".PadRight(60, '-'));
        sb.AppendLine("CHECKS:");
        sb.AppendLine("-".PadRight(60, '-'));
        
        foreach (var check in result.Checks)
        {
            var status = check.Passed ? "[PASS]" : "[FAIL]";
            sb.AppendLine($"{status} {check.Criterion}");
            if (!string.IsNullOrEmpty(check.Message))
            {
                sb.AppendLine($"       {check.Message}");
            }
        }

        if (result.Suggestions.Any())
        {
            sb.AppendLine();
            sb.AppendLine("-".PadRight(60, '-'));
            sb.AppendLine("SUGGESTIONS:");
            sb.AppendLine("-".PadRight(60, '-'));
            foreach (var suggestion in result.Suggestions)
            {
                sb.AppendLine($"  - {suggestion}");
            }
        }

        sb.AppendLine();
        sb.AppendLine("=".PadRight(60, '='));
        sb.AppendLine($"Result: {result.PassedCount}/{result.Checks.Count} passed");
        
        return sb.ToString();
    }

    private string GenerateMarkdownReport(VerificationResult result)
    {
        var sb = new System.Text.StringBuilder();
        
        sb.AppendLine("# Verification Report");
        sb.AppendLine();
        sb.AppendLine($"**Intent ID:** `{result.IntentId}`");
        sb.AppendLine($"**Status:** {GetStatusEmoji(result.Status)} {result.Status}");
        sb.AppendLine($"**Verified At:** {result.VerifiedAt:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine();
        sb.AppendLine("## Summary");
        sb.AppendLine(result.Summary);
        sb.AppendLine();
        sb.AppendLine("## Checks");
        sb.AppendLine();
        sb.AppendLine("| Status | Criterion | Type |");
        sb.AppendLine("|--------|-----------|------|");
        
        foreach (var check in result.Checks)
        {
            var status = check.Passed ? "✅" : "❌";
            sb.AppendLine($"| {status} | {check.Criterion} | {check.Type} |");
        }

        if (result.Suggestions.Any())
        {
            sb.AppendLine();
            sb.AppendLine("## Suggestions");
            foreach (var suggestion in result.Suggestions)
            {
                sb.AppendLine($"- {suggestion}");
            }
        }

        sb.AppendLine();
        sb.AppendLine("---");
        sb.AppendLine($"**Result:** {result.PassedCount}/{result.Checks.Count} passed");
        
        return sb.ToString();
    }

    private string GenerateJsonReport(VerificationResult result)
    {
        return System.Text.Json.JsonSerializer.Serialize(result, new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
        });
    }

    private string GetStatusEmoji(VerificationStatus status)
    {
        return status switch
        {
            VerificationStatus.Passed => "✅",
            VerificationStatus.Failed => "❌",
            VerificationStatus.InProgress => "🔄",
            VerificationStatus.Skipped => "⏭️",
            _ => "⏳"
        };
    }
}

/// <summary>
/// A checklist for tracking verification progress.
/// </summary>
public class VerificationChecklist
{
    public string IntentId { get; set; } = string.Empty;
    public string Goal { get; set; } = string.Empty;
    public List<ChecklistItem> Items { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsComplete => Items.All(i => 
        i.Status == ChecklistItemStatus.Passed || 
        i.Status == ChecklistItemStatus.Skipped);

    public double CompletionPercentage
    {
        get
        {
            if (Items.Count == 0) return 0;
            var completed = Items.Count(i => i.Status != ChecklistItemStatus.Pending);
            return (double)completed / Items.Count * 100;
        }
    }
}

/// <summary>
/// Individual item in a verification checklist.
/// </summary>
public class ChecklistItem
{
    public string Id { get; set; } = string.Empty;
    public string Criterion { get; set; } = string.Empty;
    public VerificationType Type { get; set; }
    public ChecklistItemStatus Status { get; set; }
    public string? Notes { get; set; }
    public bool IsConstraint { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public enum ChecklistItemStatus
{
    Pending,
    Passed,
    Failed,
    Skipped
}

public enum ReportFormat
{
    Text,
    Markdown,
    Json
}

/// <summary>
/// Strategy interface for different verification methods.
/// </summary>
public interface IVerificationStrategy
{
    bool CanHandle(VerificationType type);
    Task<VerificationCheck> VerifyAsync(ChecklistItem item);
}

/// <summary>
/// Strategy for test-based verification.
/// </summary>
internal class TestVerificationStrategy : IVerificationStrategy
{
    public bool CanHandle(VerificationType type) =>
        type == VerificationType.UnitTest || type == VerificationType.IntegrationTest;

    public Task<VerificationCheck> VerifyAsync(ChecklistItem item)
    {
        // This would integrate with test runners
        return Task.FromResult(new VerificationCheck
        {
            Name = item.Criterion,
            Criterion = item.Criterion,
            Type = item.Type,
            Passed = false,
            Message = "Requires manual test execution"
        });
    }
}

/// <summary>
/// Strategy for constraint verification.
/// </summary>
internal class ConstraintVerificationStrategy : IVerificationStrategy
{
    public bool CanHandle(VerificationType type) =>
        type == VerificationType.CodeReview;

    public Task<VerificationCheck> VerifyAsync(ChecklistItem item)
    {
        return Task.FromResult(new VerificationCheck
        {
            Name = item.Criterion,
            Criterion = item.Criterion,
            Type = item.Type,
            Passed = false,
            Message = "Requires code review"
        });
    }
}

/// <summary>
/// Strategy for manual verification.
/// </summary>
internal class ManualVerificationStrategy : IVerificationStrategy
{
    public bool CanHandle(VerificationType type) =>
        type == VerificationType.Manual;

    public Task<VerificationCheck> VerifyAsync(ChecklistItem item)
    {
        return Task.FromResult(new VerificationCheck
        {
            Name = item.Criterion,
            Criterion = item.Criterion,
            Type = item.Type,
            Passed = false,
            Message = "Requires manual verification"
        });
    }
}
