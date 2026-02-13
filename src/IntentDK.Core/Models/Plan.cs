using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace IntentDK.Core.Models;

/// <summary>
/// Represents an implementation plan generated from an intent.
/// </summary>
public class Plan
{
    /// <summary>
    /// Unique identifier for this plan.
    /// </summary>
    [YamlMember(Alias = "id")]
    public string Id { get; set; } = Guid.NewGuid().ToString("N")[..8];

    /// <summary>
    /// Reference to the source intent ID.
    /// </summary>
    [YamlMember(Alias = "intent_id")]
    public string IntentId { get; set; } = string.Empty;

    /// <summary>
    /// Summary of what this plan will accomplish.
    /// </summary>
    [YamlMember(Alias = "summary")]
    public string Summary { get; set; } = string.Empty;

    /// <summary>
    /// Ordered list of steps to implement the intent.
    /// </summary>
    [YamlMember(Alias = "steps")]
    public List<PlanStep> Steps { get; set; } = new();

    /// <summary>
    /// Files that will be created or modified.
    /// </summary>
    [YamlMember(Alias = "affected_files")]
    public List<string> AffectedFiles { get; set; } = new();

    /// <summary>
    /// Potential risks or considerations.
    /// </summary>
    [YamlMember(Alias = "risks")]
    public List<string> Risks { get; set; } = new();

    /// <summary>
    /// Dependencies required for implementation.
    /// </summary>
    [YamlMember(Alias = "dependencies")]
    public List<string> Dependencies { get; set; } = new();

    /// <summary>
    /// Timestamp when the plan was created.
    /// </summary>
    [YamlIgnore]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Current status of the plan.
    /// </summary>
    [YamlIgnore]
    public PlanStatus Status { get; set; } = PlanStatus.Draft;

    /// <summary>
    /// Gets the next step to execute.
    /// </summary>
    public PlanStep? GetNextStep()
    {
        return Steps.FirstOrDefault(s => s.Status == StepStatus.Pending);
    }

    /// <summary>
    /// Gets the current progress as a percentage.
    /// </summary>
    public double GetProgressPercentage()
    {
        if (Steps.Count == 0) return 0;
        var completed = Steps.Count(s => s.Status == StepStatus.Completed);
        return (double)completed / Steps.Count * 100;
    }
}

/// <summary>
/// Status of a plan.
/// </summary>
public enum PlanStatus
{
    /// <summary>Plan is being drafted.</summary>
    Draft,
    
    /// <summary>Plan is ready for execution.</summary>
    Ready,
    
    /// <summary>Plan is being executed.</summary>
    Executing,
    
    /// <summary>Plan execution is paused.</summary>
    Paused,
    
    /// <summary>Plan has been completed.</summary>
    Completed,
    
    /// <summary>Plan execution failed.</summary>
    Failed
}

/// <summary>
/// Represents a single step in the implementation plan.
/// </summary>
public class PlanStep
{
    /// <summary>
    /// Step number (1-based).
    /// </summary>
    [YamlMember(Alias = "step")]
    public int StepNumber { get; set; }

    /// <summary>
    /// Description of what this step accomplishes.
    /// </summary>
    [YamlMember(Alias = "description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Type of action for this step.
    /// </summary>
    [YamlMember(Alias = "action")]
    public StepAction Action { get; set; } = StepAction.Modify;

    /// <summary>
    /// Target file or resource for this step.
    /// </summary>
    [YamlMember(Alias = "target")]
    public string? Target { get; set; }

    /// <summary>
    /// Detailed instructions for implementing this step.
    /// </summary>
    [YamlMember(Alias = "details")]
    public string? Details { get; set; }

    /// <summary>
    /// Expected outcome of this step.
    /// </summary>
    [YamlMember(Alias = "expected_outcome")]
    public string? ExpectedOutcome { get; set; }

    /// <summary>
    /// Current status of this step.
    /// </summary>
    [YamlIgnore]
    public StepStatus Status { get; set; } = StepStatus.Pending;

    /// <summary>
    /// Error message if the step failed.
    /// </summary>
    [YamlIgnore]
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Types of actions a step can perform.
/// </summary>
public enum StepAction
{
    Create,
    Modify,
    Delete,
    Test,
    Review,
    Configure,
    Document
}

/// <summary>
/// Status of a plan step.
/// </summary>
public enum StepStatus
{
    Pending,
    InProgress,
    Completed,
    Skipped,
    Failed
}
