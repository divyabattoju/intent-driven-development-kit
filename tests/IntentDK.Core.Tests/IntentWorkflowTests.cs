using IntentDK.Core.Models;
using IntentDK.Core.Verification;

namespace IntentDK.Core.Tests;

public class IntentWorkflowTests
{
    [Fact]
    public void Create_WithFluentApi_BuildsIntent()
    {
        // Act
        var workflow = IntentProcessor.Create("Add user authentication")
            .WithScope("AuthController", "UserService", "TokenGenerator")
            .WithConstraints("Use JWT tokens", "Tokens expire in 24 hours")
            .WithVerification("Login test passes", "Token validation test")
            .WithPriority(IntentPriority.High)
            .WithTags("security", "auth");

        // Assert
        var intent = workflow.Intent;
        Assert.Equal("Add user authentication", intent.Goal);
        Assert.Equal(3, intent.Scope.Count);
        Assert.Equal(2, intent.Constraints.Count);
        Assert.Equal(2, intent.Verification.Count);
        Assert.Equal(IntentPriority.High, intent.Priority);
        Assert.Equal(2, intent.Tags.Count);
    }

    [Fact]
    public void Validate_ValidIntent_DoesNotThrow()
    {
        // Arrange
        var workflow = IntentProcessor.Create("Add feature")
            .WithScope("Service");

        // Act & Assert - should not throw
        var result = workflow.Validate();
        Assert.Same(workflow, result);
    }

    [Fact]
    public void Validate_InvalidIntent_Throws()
    {
        // Arrange
        var workflow = IntentProcessor.Create(""); // Invalid - empty goal

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => workflow.Validate());
    }

    [Fact]
    public void CreatePlan_GeneratesPlan()
    {
        // Arrange
        var workflow = IntentProcessor.Create("Add caching")
            .WithScope("Repository")
            .WithVerification("Cache hit test");

        // Act
        workflow.CreatePlan();

        // Assert
        Assert.NotNull(workflow.Plan);
        Assert.Equal(IntentStatus.Planned, workflow.Intent.Status);
    }

    [Fact]
    public void GetPlanMarkdown_ReturnsMarkdown()
    {
        // Arrange
        var workflow = IntentProcessor.Create("Add logging")
            .WithScope("Service")
            .WithConstraints("No PII in logs")
            .WithVerification("Log output test");

        // Act
        var markdown = workflow.GetPlanMarkdown();

        // Assert
        Assert.Contains("# Implementation Plan", markdown);
        Assert.Contains("Add logging", markdown);
    }

    [Fact]
    public void CreateChecklist_GeneratesChecklist()
    {
        // Arrange
        var workflow = IntentProcessor.Create("Add feature")
            .WithScope("Service")
            .WithConstraints("Must be backward compatible")
            .WithVerification("Unit tests pass", "Integration test passes");

        // Act
        workflow.CreateChecklist();

        // Assert
        Assert.NotNull(workflow.Checklist);
        // 2 verification items + 1 constraint = 3 total
        Assert.Equal(3, workflow.Checklist.Items.Count);
    }

    [Fact]
    public void MarkPassed_UpdatesChecklistItem()
    {
        // Arrange
        var workflow = IntentProcessor.Create("Add feature")
            .WithScope("Service")
            .WithVerification("Unit tests pass")
            .CreateChecklist();

        // Act
        workflow.MarkPassed("Unit tests", "All 15 tests passed");

        // Assert
        var item = workflow.Checklist!.Items.First(i => i.Criterion.Contains("Unit tests"));
        Assert.Equal(ChecklistItemStatus.Passed, item.Status);
        Assert.Equal("All 15 tests passed", item.Notes);
    }

    [Fact]
    public void MarkFailed_UpdatesChecklistItem()
    {
        // Arrange
        var workflow = IntentProcessor.Create("Add feature")
            .WithScope("Service")
            .WithVerification("Performance test passes")
            .CreateChecklist();

        // Act
        workflow.MarkFailed("Performance", "Response time > 200ms");

        // Assert
        var item = workflow.Checklist!.Items.First(i => i.Criterion.Contains("Performance"));
        Assert.Equal(ChecklistItemStatus.Failed, item.Status);
    }

    [Fact]
    public void Verify_AllPassed_SetsCompletedStatus()
    {
        // Arrange
        var workflow = IntentProcessor.Create("Add feature")
            .WithScope("Service")
            .WithVerification("Test passes")
            .CreateChecklist()
            .MarkPassed("Test");

        // Act
        workflow.Verify();

        // Assert
        Assert.Equal(VerificationStatus.Passed, workflow.VerificationResult!.Status);
        Assert.Equal(IntentStatus.Completed, workflow.Intent.Status);
    }

    [Fact]
    public void Verify_SomeFailed_SetsFailedStatus()
    {
        // Arrange
        var workflow = IntentProcessor.Create("Add feature")
            .WithScope("Service")
            .WithVerification("Test 1", "Test 2")
            .CreateChecklist()
            .MarkPassed("Test 1")
            .MarkFailed("Test 2");

        // Act
        workflow.Verify();

        // Assert
        Assert.Equal(VerificationStatus.Failed, workflow.VerificationResult!.Status);
        Assert.Equal(IntentStatus.Failed, workflow.Intent.Status);
    }

    [Fact]
    public void ToYaml_SerializesIntent()
    {
        // Arrange
        var workflow = IntentProcessor.Create("Add logging")
            .WithScope("AuthService", "Logger")
            .WithConstraints("No passwords")
            .WithVerification("Log test");

        // Act
        var yaml = workflow.ToYaml();

        // Assert
        Assert.Contains("goal: Add logging", yaml);
        Assert.Contains("AuthService", yaml);
        Assert.Contains("No passwords", yaml);
    }

    [Fact]
    public void FromYaml_CreatesWorkflow()
    {
        // Arrange
        var yaml = @"
goal: Add caching
scope:
  - Repository
  - CacheService
constraints:
  - Max 1 hour TTL
verification:
  - Cache hit test
";

        // Act
        var workflow = IntentProcessor.FromYaml(yaml);

        // Assert
        Assert.Equal("Add caching", workflow.Intent.Goal);
        Assert.Equal(2, workflow.Intent.Scope.Count);
    }

    [Fact]
    public void GetVerificationReport_ReturnsReport()
    {
        // Arrange
        var workflow = IntentProcessor.Create("Add feature")
            .WithScope("Service")
            .WithVerification("Test passes")
            .CreateChecklist()
            .MarkPassed("Test");

        // Act
        var report = workflow.GetVerificationReport(ReportFormat.Markdown);

        // Assert
        Assert.Contains("# Verification Report", report);
        Assert.Contains("✅", report);
    }

    [Fact]
    public void CompleteWorkflow_EndToEnd()
    {
        // This test demonstrates the complete workflow
        
        // 1. Define intent
        var workflow = IntentProcessor.Create("Add input validation")
            .WithScope("UserController", "ValidationService")
            .WithConstraints("Return 400 for invalid input", "Log validation failures")
            .WithVerification("Empty name returns error", "Invalid email returns error")
            .WithContext("Users can currently submit invalid data")
            .WithPriority(IntentPriority.High);

        // 2. Validate
        workflow.Validate();
        Assert.True(workflow.Intent.Validate().IsValid);

        // 3. Create plan
        workflow.CreatePlan();
        Assert.NotNull(workflow.Plan);
        Assert.True(workflow.Plan.Steps.Count > 0);

        // 4. Get plan markdown (for review)
        var planMarkdown = workflow.GetPlanMarkdown();
        Assert.Contains("UserController", planMarkdown);

        // 5. Create verification checklist
        workflow.CreateChecklist();
        Assert.NotNull(workflow.Checklist);

        // 6. After implementation, mark verifications
        workflow.MarkPassed("Empty name", "Tested with empty string");
        workflow.MarkPassed("Invalid email", "Tested with 'notanemail'");
        workflow.MarkPassed("400 for invalid", "Verified response code");
        workflow.MarkPassed("Log validation", "Checked log output");

        // 7. Verify
        workflow.Verify();
        Assert.Equal(VerificationStatus.Passed, workflow.VerificationResult!.Status);

        // 8. Get report
        var report = workflow.GetVerificationReport();
        Assert.Contains("Passed", report);
    }
}
