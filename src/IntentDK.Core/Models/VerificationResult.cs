using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace IntentDK.Core.Models;

/// <summary>
/// Result of verifying an intent implementation.
/// </summary>
public class VerificationResult
{
    /// <summary>
    /// Reference to the intent ID being verified.
    /// </summary>
    [YamlMember(Alias = "intent_id")]
    public string IntentId { get; set; } = string.Empty;

    /// <summary>
    /// Reference to the plan ID being verified.
    /// </summary>
    [YamlMember(Alias = "plan_id")]
    public string? PlanId { get; set; }

    /// <summary>
    /// Overall verification status.
    /// </summary>
    [YamlMember(Alias = "status")]
    public VerificationStatus Status { get; set; } = VerificationStatus.Pending;

    /// <summary>
    /// Individual verification check results.
    /// </summary>
    [YamlMember(Alias = "checks")]
    public List<VerificationCheck> Checks { get; set; } = new();

    /// <summary>
    /// Summary of the verification outcome.
    /// </summary>
    [YamlMember(Alias = "summary")]
    public string Summary { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when verification was performed.
    /// </summary>
    [YamlMember(Alias = "verified_at")]
    public DateTime VerifiedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Suggestions for improvements or fixes.
    /// </summary>
    [YamlMember(Alias = "suggestions")]
    public List<string> Suggestions { get; set; } = new();

    /// <summary>
    /// Returns true if all checks passed.
    /// </summary>
    public bool AllChecksPassed => Checks.All(c => c.Passed);

    /// <summary>
    /// Gets the number of passed checks.
    /// </summary>
    public int PassedCount => Checks.Count(c => c.Passed);

    /// <summary>
    /// Gets the number of failed checks.
    /// </summary>
    public int FailedCount => Checks.Count(c => !c.Passed);

    /// <summary>
    /// Creates a successful verification result.
    /// </summary>
    public static VerificationResult Success(string intentId, List<VerificationCheck> checks)
    {
        return new VerificationResult
        {
            IntentId = intentId,
            Status = VerificationStatus.Passed,
            Checks = checks,
            Summary = "All verification criteria have been met."
        };
    }

    /// <summary>
    /// Creates a failed verification result.
    /// </summary>
    public static VerificationResult Failure(string intentId, List<VerificationCheck> checks, string summary)
    {
        return new VerificationResult
        {
            IntentId = intentId,
            Status = VerificationStatus.Failed,
            Checks = checks,
            Summary = summary
        };
    }
}

/// <summary>
/// Overall verification status.
/// </summary>
public enum VerificationStatus
{
    /// <summary>Verification has not been performed.</summary>
    Pending,
    
    /// <summary>Verification is in progress.</summary>
    InProgress,
    
    /// <summary>All verification criteria passed.</summary>
    Passed,
    
    /// <summary>Some verification criteria failed.</summary>
    Failed,
    
    /// <summary>Verification was skipped.</summary>
    Skipped
}

/// <summary>
/// Individual verification check.
/// </summary>
public class VerificationCheck
{
    /// <summary>
    /// Name or description of the check.
    /// </summary>
    [YamlMember(Alias = "name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The original verification criterion from the intent.
    /// </summary>
    [YamlMember(Alias = "criterion")]
    public string Criterion { get; set; } = string.Empty;

    /// <summary>
    /// Whether this check passed.
    /// </summary>
    [YamlMember(Alias = "passed")]
    public bool Passed { get; set; }

    /// <summary>
    /// Detailed message about the check result.
    /// </summary>
    [YamlMember(Alias = "message")]
    public string? Message { get; set; }

    /// <summary>
    /// Evidence supporting the check result.
    /// </summary>
    [YamlMember(Alias = "evidence")]
    public string? Evidence { get; set; }

    /// <summary>
    /// Type of verification performed.
    /// </summary>
    [YamlMember(Alias = "type")]
    public VerificationType Type { get; set; } = VerificationType.Manual;
}

/// <summary>
/// Types of verification checks.
/// </summary>
public enum VerificationType
{
    /// <summary>Manual review by developer.</summary>
    Manual,
    
    /// <summary>Unit test execution.</summary>
    UnitTest,
    
    /// <summary>Integration test execution.</summary>
    IntegrationTest,
    
    /// <summary>Code review or static analysis.</summary>
    CodeReview,
    
    /// <summary>Linter or code quality check.</summary>
    Linter,
    
    /// <summary>Build verification.</summary>
    Build,
    
    /// <summary>Security scan.</summary>
    Security
}
