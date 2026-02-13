using IntentDK.Core.Models;
using IntentDK.Core.Planning;

namespace IntentDK.Core.Tests;

public class PlanGeneratorTests
{
    private readonly PlanGenerator _generator = new();

    [Fact]
    public void GeneratePlan_ValidIntent_ReturnsPlan()
    {
        // Arrange
        var intent = new Intent
        {
            Goal = "Add logging to authentication",
            Scope = new List<string> { "AuthService", "Logger" },
            Constraints = new List<string> { "Must not log passwords" },
            Verification = new List<string> { "Unit test for login logs" }
        };

        // Act
        var plan = _generator.GeneratePlan(intent);

        // Assert
        Assert.NotNull(plan);
        Assert.Equal(intent.Id, plan.IntentId);
        Assert.NotEmpty(plan.Steps);
        Assert.Equal(PlanStatus.Ready, plan.Status);
    }

    [Fact]
    public void GeneratePlan_IncludesStepsForEachScopeItem()
    {
        // Arrange
        var intent = new Intent
        {
            Goal = "Refactor services",
            Scope = new List<string> { "ServiceA", "ServiceB", "ServiceC" }
        };

        // Act
        var plan = _generator.GeneratePlan(intent);

        // Assert
        var modifySteps = plan.Steps.Where(s => s.Action == StepAction.Modify).ToList();
        Assert.Equal(3, modifySteps.Count);
        Assert.Contains(modifySteps, s => s.Target == "ServiceA");
        Assert.Contains(modifySteps, s => s.Target == "ServiceB");
        Assert.Contains(modifySteps, s => s.Target == "ServiceC");
    }

    [Fact]
    public void GeneratePlan_IncludesVerificationSteps()
    {
        // Arrange
        var intent = new Intent
        {
            Goal = "Add feature",
            Scope = new List<string> { "Service" },
            Verification = new List<string> 
            { 
                "Unit test coverage",
                "Integration test passes"
            }
        };

        // Act
        var plan = _generator.GeneratePlan(intent);

        // Assert
        var testSteps = plan.Steps.Where(s => s.Action == StepAction.Test).ToList();
        Assert.Equal(2, testSteps.Count);
    }

    [Fact]
    public void GeneratePlan_IncludesAnalysisStep_WhenEnabled()
    {
        // Arrange
        var options = new PlanGeneratorOptions { IncludeAnalysisStep = true };
        var generator = new PlanGenerator(options);
        var intent = new Intent
        {
            Goal = "Add feature",
            Scope = new List<string> { "Service" }
        };

        // Act
        var plan = generator.GeneratePlan(intent);

        // Assert
        var firstStep = plan.Steps.First();
        Assert.Equal(StepAction.Review, firstStep.Action);
        Assert.Contains("Analyze", firstStep.Description);
    }

    [Fact]
    public void GeneratePlan_ExcludesAnalysisStep_WhenDisabled()
    {
        // Arrange
        var options = new PlanGeneratorOptions 
        { 
            IncludeAnalysisStep = false,
            IncludeReviewStep = false
        };
        var generator = new PlanGenerator(options);
        var intent = new Intent
        {
            Goal = "Add feature",
            Scope = new List<string> { "Service" }
        };

        // Act
        var plan = generator.GeneratePlan(intent);

        // Assert
        var reviewSteps = plan.Steps.Where(s => 
            s.Action == StepAction.Review && 
            s.Description.Contains("Analyze")).ToList();
        Assert.Empty(reviewSteps);
    }

    [Fact]
    public void GeneratePlanMarkdown_ReturnsFormattedMarkdown()
    {
        // Arrange
        var intent = new Intent
        {
            Goal = "Add caching",
            Scope = new List<string> { "Repository" },
            Constraints = new List<string> { "Max 1 hour TTL" },
            Verification = new List<string> { "Cache hit test" }
        };

        // Act
        var markdown = _generator.GeneratePlanMarkdown(intent);

        // Assert
        Assert.Contains("# Implementation Plan", markdown);
        Assert.Contains("Add caching", markdown);
        Assert.Contains("Repository", markdown);
        Assert.Contains("Max 1 hour TTL", markdown);
        Assert.Contains("[ ] Cache hit test", markdown);
    }

    [Fact]
    public void GeneratePlanText_ReturnsFormattedText()
    {
        // Arrange
        var intent = new Intent
        {
            Goal = "Add logging",
            Scope = new List<string> { "Service" },
            Verification = new List<string> { "Log output verified" }
        };

        // Act
        var text = _generator.GeneratePlanText(intent);

        // Assert
        Assert.Contains("IMPLEMENTATION PLAN", text);
        Assert.Contains("Add logging", text);
        Assert.Contains("VERIFICATION CRITERIA", text);
    }

    [Fact]
    public void GeneratePlan_InvalidIntent_ThrowsException()
    {
        // Arrange
        var intent = new Intent { Goal = "" }; // Invalid - empty goal

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => _generator.GeneratePlan(intent));
    }

    [Fact]
    public void Plan_GetNextStep_ReturnsFirstPendingStep()
    {
        // Arrange
        var intent = new Intent
        {
            Goal = "Test",
            Scope = new List<string> { "A", "B" }
        };
        var plan = _generator.GeneratePlan(intent);
        
        // Mark first step as completed
        plan.Steps[0].Status = StepStatus.Completed;

        // Act
        var nextStep = plan.GetNextStep();

        // Assert
        Assert.NotNull(nextStep);
        Assert.Equal(2, nextStep.StepNumber);
    }

    [Fact]
    public void Plan_GetProgressPercentage_CalculatesCorrectly()
    {
        // Arrange
        var intent = new Intent
        {
            Goal = "Test",
            Scope = new List<string> { "A" }
        };
        var plan = _generator.GeneratePlan(intent);
        var totalSteps = plan.Steps.Count;
        
        // Mark half as completed
        var halfCount = totalSteps / 2;
        for (var i = 0; i < halfCount; i++)
        {
            plan.Steps[i].Status = StepStatus.Completed;
        }

        // Act
        var progress = plan.GetProgressPercentage();

        // Assert
        var expectedProgress = (double)halfCount / totalSteps * 100;
        Assert.Equal(expectedProgress, progress, 1);
    }
}
