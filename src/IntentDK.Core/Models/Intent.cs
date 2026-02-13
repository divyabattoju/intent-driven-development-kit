using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace IntentDK.Core.Models;

/// <summary>
/// Represents a development intent - a structured, human-readable description
/// of what a developer wants to accomplish.
/// </summary>
public class Intent
{
    /// <summary>
    /// Unique identifier for this intent.
    /// </summary>
    [YamlMember(Alias = "id")]
    public string Id { get; set; } = Guid.NewGuid().ToString("N")[..8];

    /// <summary>
    /// The primary goal or objective to achieve.
    /// Example: "Add logging to user login"
    /// </summary>
    [YamlMember(Alias = "goal")]
    public string Goal { get; set; } = string.Empty;

    /// <summary>
    /// Files, classes, or modules that should be affected.
    /// Example: ["AuthService", "Logger"]
    /// </summary>
    [YamlMember(Alias = "scope")]
    public List<string> Scope { get; set; } = new();

    /// <summary>
    /// Constraints or requirements that must be respected.
    /// Example: ["Must not expose passwords"]
    /// </summary>
    [YamlMember(Alias = "constraints")]
    public List<string> Constraints { get; set; } = new();

    /// <summary>
    /// Verification criteria to confirm the intent was achieved.
    /// Example: ["Unit test for login logs"]
    /// </summary>
    [YamlMember(Alias = "verification")]
    public List<string> Verification { get; set; } = new();

    /// <summary>
    /// Optional context or background information.
    /// </summary>
    [YamlMember(Alias = "context")]
    public string? Context { get; set; }

    /// <summary>
    /// Optional priority level (low, medium, high, critical).
    /// </summary>
    [YamlMember(Alias = "priority")]
    public IntentPriority Priority { get; set; } = IntentPriority.Medium;

    /// <summary>
    /// Optional tags for categorization.
    /// </summary>
    [YamlMember(Alias = "tags")]
    public List<string> Tags { get; set; } = new();

    /// <summary>
    /// The current status of this intent.
    /// </summary>
    [YamlIgnore]
    public IntentStatus Status { get; set; } = IntentStatus.Pending;

    /// <summary>
    /// Timestamp when the intent was created.
    /// </summary>
    [YamlIgnore]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Validates that the intent has all required fields.
    /// </summary>
    public ValidationResult Validate()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(Goal))
        {
            errors.Add("Goal is required and cannot be empty.");
        }

        if (Scope.Count == 0)
        {
            errors.Add("At least one scope item is required.");
        }

        return new ValidationResult
        {
            IsValid = errors.Count == 0,
            Errors = errors
        };
    }
}

/// <summary>
/// Priority levels for intents.
/// </summary>
public enum IntentPriority
{
    Low,
    Medium,
    High,
    Critical
}

/// <summary>
/// Status of an intent through its lifecycle.
/// </summary>
public enum IntentStatus
{
    /// <summary>Intent has been defined but not started.</summary>
    Pending,
    
    /// <summary>A plan has been created for this intent.</summary>
    Planned,
    
    /// <summary>Implementation is in progress.</summary>
    InProgress,
    
    /// <summary>Implementation is complete, awaiting verification.</summary>
    Implemented,
    
    /// <summary>Verification is in progress.</summary>
    Verifying,
    
    /// <summary>Intent has been successfully completed and verified.</summary>
    Completed,
    
    /// <summary>Intent verification failed.</summary>
    Failed,
    
    /// <summary>Intent was cancelled.</summary>
    Cancelled
}

/// <summary>
/// Result of validation operations.
/// </summary>
public class ValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    
    public static ValidationResult Success() => new() { IsValid = true };
    
    public static ValidationResult Failure(params string[] errors) => new()
    {
        IsValid = false,
        Errors = errors.ToList()
    };
}
