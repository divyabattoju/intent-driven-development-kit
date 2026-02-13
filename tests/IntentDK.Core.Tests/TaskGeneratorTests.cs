using IntentDK.Core.Models;
using IntentDK.Core.Planning;

namespace IntentDK.Core.Tests;

public class TaskGeneratorTests
{
    private readonly TaskGenerator _generator = new();

    [Fact]
    public void GenerateTasks_ValidIntent_ReturnsTaskBreakdown()
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
        var breakdown = _generator.GenerateTasks(intent);

        // Assert
        Assert.NotNull(breakdown);
        Assert.Equal(intent.Id, breakdown.IntentId);
        Assert.Equal(intent.Goal, breakdown.Goal);
        Assert.NotEmpty(breakdown.Tasks);
    }

    [Fact]
    public void GenerateTasks_IncludesAnalysisTask_WhenEnabled()
    {
        // Arrange
        var options = new TaskGeneratorOptions { IncludeAnalysisTask = true };
        var generator = new TaskGenerator(options);
        var intent = new Intent
        {
            Goal = "Test",
            Scope = new List<string> { "Service" }
        };

        // Act
        var breakdown = generator.GenerateTasks(intent);

        // Assert
        var analysisTask = breakdown.Tasks.FirstOrDefault(t => t.Type == TaskType.Analyze);
        Assert.NotNull(analysisTask);
    }

    [Fact]
    public void GenerateTasks_ExcludesAnalysisTask_WhenDisabled()
    {
        // Arrange
        var options = new TaskGeneratorOptions 
        { 
            IncludeAnalysisTask = false,
            IncludeFinalReviewTask = false,
            IncludeConstraintVerification = false
        };
        var generator = new TaskGenerator(options);
        var intent = new Intent
        {
            Goal = "Test",
            Scope = new List<string> { "Service" }
        };

        // Act
        var breakdown = generator.GenerateTasks(intent);

        // Assert
        var analysisTask = breakdown.Tasks.FirstOrDefault(t => t.Type == TaskType.Analyze);
        Assert.Null(analysisTask);
    }

    [Fact]
    public void GenerateTasks_CreatesTaskForEachScopeItem()
    {
        // Arrange
        var intent = new Intent
        {
            Goal = "Refactor services",
            Scope = new List<string> { "ServiceA", "ServiceB", "ServiceC" }
        };

        // Act
        var breakdown = _generator.GenerateTasks(intent);

        // Assert
        var implementTasks = breakdown.Tasks.Where(t => 
            t.Type == TaskType.Implement && 
            t.Target != null).ToList();
        
        Assert.Contains(implementTasks, t => t.Target == "ServiceA");
        Assert.Contains(implementTasks, t => t.Target == "ServiceB");
        Assert.Contains(implementTasks, t => t.Target == "ServiceC");
    }

    [Fact]
    public void GenerateTasks_CreatesVerificationTasks()
    {
        // Arrange
        var intent = new Intent
        {
            Goal = "Add feature",
            Scope = new List<string> { "Service" },
            Verification = new List<string> 
            { 
                "Unit test passes",
                "Integration test passes"
            }
        };

        // Act
        var breakdown = _generator.GenerateTasks(intent);

        // Assert
        var testTasks = breakdown.Tasks.Where(t => 
            t.Type == TaskType.Test || t.Type == TaskType.Verify).ToList();
        Assert.True(testTasks.Count >= 2);
    }

    [Fact]
    public void GenerateTasks_SetsDependencies()
    {
        // Arrange
        var intent = new Intent
        {
            Goal = "Add feature",
            Scope = new List<string> { "Service" },
            Verification = new List<string> { "Test passes" }
        };

        // Act
        var breakdown = _generator.GenerateTasks(intent);

        // Assert
        var testTask = breakdown.Tasks.FirstOrDefault(t => t.Type == TaskType.Test);
        Assert.NotNull(testTask);
        Assert.NotEmpty(testTask.DependsOn);
    }

    [Fact]
    public void GenerateTasks_CalculatesProgress()
    {
        // Arrange
        var intent = new Intent
        {
            Goal = "Test",
            Scope = new List<string> { "Service" }
        };

        // Act
        var breakdown = _generator.GenerateTasks(intent);

        // Assert
        Assert.Equal(breakdown.Tasks.Count, breakdown.Progress.Total);
        Assert.Equal(0, breakdown.Progress.Completed);
        Assert.Equal(0, breakdown.Progress.Percentage);
    }

    [Fact]
    public void TaskBreakdown_UpdateProgress_CalculatesCorrectly()
    {
        // Arrange
        var breakdown = new TaskBreakdown
        {
            Tasks = new List<ImplementationTask>
            {
                new() { Id = "T1", Status = Models.TaskStatus.Completed },
                new() { Id = "T2", Status = Models.TaskStatus.Completed },
                new() { Id = "T3", Status = Models.TaskStatus.InProgress },
                new() { Id = "T4", Status = Models.TaskStatus.Pending }
            }
        };

        // Act
        breakdown.UpdateProgress();

        // Assert
        Assert.Equal(4, breakdown.Progress.Total);
        Assert.Equal(2, breakdown.Progress.Completed);
        Assert.Equal(1, breakdown.Progress.InProgress);
        Assert.Equal(1, breakdown.Progress.Pending);
        Assert.Equal(50, breakdown.Progress.Percentage);
    }

    [Fact]
    public void TaskBreakdown_GetNextTask_ReturnsPendingTask()
    {
        // Arrange
        var breakdown = new TaskBreakdown
        {
            Tasks = new List<ImplementationTask>
            {
                new() { Id = "T1", Status = Models.TaskStatus.Completed },
                new() { Id = "T2", Status = Models.TaskStatus.Pending },
                new() { Id = "T3", Status = Models.TaskStatus.Pending }
            }
        };

        // Act
        var nextTask = breakdown.GetNextTask();

        // Assert
        Assert.NotNull(nextTask);
        Assert.Equal("T2", nextTask.Id);
    }

    [Fact]
    public void GenerateTasksMarkdown_ReturnsFormattedMarkdown()
    {
        // Arrange
        var intent = new Intent
        {
            Goal = "Add caching",
            Scope = new List<string> { "Repository" },
            Verification = new List<string> { "Cache hit test" }
        };

        // Act
        var markdown = _generator.GenerateTasksMarkdown(intent);

        // Assert
        Assert.Contains("# Task Breakdown", markdown);
        Assert.Contains("Add caching", markdown);
        Assert.Contains("Repository", markdown);
    }

    [Fact]
    public void GenerateTasksYaml_ReturnsValidYaml()
    {
        // Arrange
        var intent = new Intent
        {
            Goal = "Add logging",
            Scope = new List<string> { "Service" }
        };

        // Act
        var yaml = _generator.GenerateTasksYaml(intent);

        // Assert
        Assert.Contains("intent_id:", yaml);
        Assert.Contains("goal:", yaml);
        Assert.Contains("tasks:", yaml);
        Assert.Contains("progress:", yaml);
    }

    [Fact]
    public void GenerateTasksChecklist_ReturnsChecklist()
    {
        // Arrange
        var intent = new Intent
        {
            Goal = "Add feature",
            Scope = new List<string> { "Service" }
        };

        // Act
        var checklist = _generator.GenerateTasksChecklist(intent);

        // Assert
        Assert.Contains("[ ]", checklist);
        Assert.Contains("Add feature", checklist);
    }

    [Fact]
    public void GenerateTasks_InvalidIntent_ThrowsException()
    {
        // Arrange
        var intent = new Intent { Goal = "" };

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => _generator.GenerateTasks(intent));
    }

    [Fact]
    public void GenerateTasks_DetectsNewFileScope()
    {
        // Arrange
        var intent = new Intent
        {
            Goal = "Add service",
            Scope = new List<string> { "NewService (new)", "ExistingService" }
        };

        // Act
        var breakdown = _generator.GenerateTasks(intent);

        // Assert
        var createTask = breakdown.Tasks.FirstOrDefault(t => 
            t.Type == TaskType.Create && t.Target == "NewService (new)");
        Assert.NotNull(createTask);
    }
}
