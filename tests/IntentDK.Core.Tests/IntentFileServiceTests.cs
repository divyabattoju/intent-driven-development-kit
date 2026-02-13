using IntentDK.Core.Services;
using IntentDK.Core.Templates;

namespace IntentDK.Core.Tests;

public class IntentFileServiceTests : IDisposable
{
    private readonly IntentFileService _service = new();
    private readonly string _testDirectory;

    public IntentFileServiceTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"IntentDK_Test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_testDirectory);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, true);
        }
    }

    [Fact]
    public void CreateIntentFile_CreatesFileWithTemplate()
    {
        // Act
        var filePath = _service.CreateIntentFile(_testDirectory);

        // Assert
        Assert.True(File.Exists(filePath));
        var content = File.ReadAllText(filePath);
        Assert.Contains("goal:", content);
        Assert.Contains("scope:", content);
    }

    [Fact]
    public void CreateIntentFile_WithName_UsesNameInFileName()
    {
        // Act
        var filePath = _service.CreateIntentFile(_testDirectory, "my-feature");

        // Assert
        Assert.Contains("my-feature", Path.GetFileName(filePath));
    }

    [Fact]
    public void CreateIntentFile_FeatureTemplate_ContainsFeatureContent()
    {
        // Act
        var filePath = _service.CreateIntentFile(
            _testDirectory, 
            "feature", 
            IntentTemplateType.Feature);

        // Assert
        var content = File.ReadAllText(filePath);
        Assert.Contains("feature", content.ToLower());
        Assert.Contains("backward compatible", content.ToLower());
    }

    [Fact]
    public void CreateIntentFile_BugFixTemplate_ContainsBugFixContent()
    {
        // Act
        var filePath = _service.CreateIntentFile(
            _testDirectory, 
            "bugfix", 
            IntentTemplateType.BugFix);

        // Assert
        var content = File.ReadAllText(filePath);
        Assert.Contains("fix", content.ToLower());
        Assert.Contains("regression", content.ToLower());
    }

    [Fact]
    public void CreateIntentFileInProject_CreatesInIntentDirectory()
    {
        // Act
        var filePath = _service.CreateIntentFileInProject(_testDirectory);

        // Assert
        var expectedDir = Path.Combine(_testDirectory, IntentFileService.DefaultIntentDirectory);
        Assert.True(Directory.Exists(expectedDir));
        Assert.StartsWith(expectedDir, filePath);
    }

    [Fact]
    public void ReadIntent_ValidFile_ReturnsIntent()
    {
        // Arrange
        var content = @"
goal: Test goal
scope:
  - TestService
";
        var filePath = Path.Combine(_testDirectory, "test.intent.yaml");
        File.WriteAllText(filePath, content);

        // Act
        var result = _service.ReadIntent(filePath);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Test goal", result.Value!.Goal);
    }

    [Fact]
    public void ReadIntent_NonExistentFile_ReturnsFailure()
    {
        // Act
        var result = _service.ReadIntent(Path.Combine(_testDirectory, "nonexistent.yaml"));

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Contains("not found"));
    }

    [Fact]
    public void FindLatestIntentFile_ReturnsLatest()
    {
        // Arrange
        var file1 = Path.Combine(_testDirectory, "old.intent.yaml");
        var file2 = Path.Combine(_testDirectory, "new.intent.yaml");
        
        File.WriteAllText(file1, "goal: old");
        Thread.Sleep(100); // Ensure different timestamps
        File.WriteAllText(file2, "goal: new");

        // Act
        var latest = _service.FindLatestIntentFile(_testDirectory);

        // Assert
        Assert.NotNull(latest);
        Assert.Equal(file2, latest);
    }

    [Fact]
    public void FindIntentFile_FindsInIntentDirectory()
    {
        // Arrange
        _service.CreateIntentFileInProject(_testDirectory, "my-intent");

        // Act
        var found = _service.FindIntentFile(_testDirectory);

        // Assert
        Assert.NotNull(found);
        Assert.Contains("my-intent", found);
    }

    [Fact]
    public void GetAllIntents_ReturnsAllIntentFiles()
    {
        // Arrange
        _service.CreateIntentFileInProject(_testDirectory, "intent1");
        _service.CreateIntentFileInProject(_testDirectory, "intent2");

        // Act
        var intents = _service.GetAllIntents(_testDirectory).ToList();

        // Assert
        Assert.Equal(2, intents.Count);
    }

    [Fact]
    public void CreatePlanFile_CreatesAssociatedFile()
    {
        // Arrange
        var intentPath = _service.CreateIntentFile(_testDirectory, "test");
        var planContent = "# Plan\n\nTest plan content";

        // Act
        var planPath = _service.CreatePlanFile(intentPath, planContent);

        // Assert
        Assert.True(File.Exists(planPath));
        Assert.Contains(".plan.yaml", planPath);
        var content = File.ReadAllText(planPath);
        Assert.Contains("Test plan content", content);
    }

    [Fact]
    public void CreateTasksFile_CreatesAssociatedFile()
    {
        // Arrange
        var intentPath = _service.CreateIntentFile(_testDirectory, "test");
        var tasksContent = "# Tasks\n\nTest tasks content";

        // Act
        var tasksPath = _service.CreateTasksFile(intentPath, tasksContent);

        // Assert
        Assert.True(File.Exists(tasksPath));
        Assert.Contains(".tasks.yaml", tasksPath);
    }

    [Fact]
    public void GetAssociatedFilePath_GeneratesCorrectPath()
    {
        // Arrange
        var intentPath = Path.Combine(_testDirectory, "my-feature.intent.yaml");

        // Act
        var planPath = _service.GetAssociatedFilePath(intentPath, ".plan.yaml");
        var tasksPath = _service.GetAssociatedFilePath(intentPath, ".tasks.yaml");

        // Assert
        Assert.EndsWith("my-feature.plan.yaml", planPath);
        Assert.EndsWith("my-feature.tasks.yaml", tasksPath);
    }

    [Fact]
    public void IntentFileInfo_TracksHasPlanAndHasTasks()
    {
        // Arrange
        var intentPath = _service.CreateIntentFile(_testDirectory, "test");
        _service.CreatePlanFile(intentPath, "plan content");

        // Act
        var intents = _service.GetAllIntents(_testDirectory).ToList();

        // Assert
        var intentInfo = intents.First();
        Assert.True(intentInfo.HasPlan);
        Assert.False(intentInfo.HasTasks);
    }
}
