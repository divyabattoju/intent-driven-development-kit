using IntentDK.Core.Models;
using IntentDK.Core.Parsing;

namespace IntentDK.Core.Tests;

public class IntentParserTests
{
    private readonly IntentParser _parser = new();

    [Fact]
    public void Parse_ValidYaml_ReturnsIntent()
    {
        // Arrange
        var yaml = @"
goal: Add logging to user login
scope:
  - AuthService
  - Logger
constraints:
  - Must not expose passwords
verification:
  - Unit test for login logs
";

        // Act
        var intent = _parser.Parse(yaml);

        // Assert
        Assert.Equal("Add logging to user login", intent.Goal);
        Assert.Equal(2, intent.Scope.Count);
        Assert.Contains("AuthService", intent.Scope);
        Assert.Contains("Logger", intent.Scope);
        Assert.Single(intent.Constraints);
        Assert.Equal("Must not expose passwords", intent.Constraints[0]);
        Assert.Single(intent.Verification);
        Assert.Equal("Unit test for login logs", intent.Verification[0]);
    }

    [Fact]
    public void ParseAndValidate_ValidYaml_ReturnsSuccess()
    {
        // Arrange
        var yaml = @"
goal: Add feature
scope:
  - MyService
";

        // Act
        var result = _parser.ParseAndValidate(yaml);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("Add feature", result.Value!.Goal);
    }

    [Fact]
    public void ParseAndValidate_MissingGoal_ReturnsFailure()
    {
        // Arrange
        var yaml = @"
scope:
  - MyService
";

        // Act
        var result = _parser.ParseAndValidate(yaml);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Contains("Goal"));
    }

    [Fact]
    public void ParseAndValidate_MissingScope_ReturnsFailure()
    {
        // Arrange
        var yaml = @"
goal: Do something
";

        // Act
        var result = _parser.ParseAndValidate(yaml);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Contains("scope"));
    }

    [Fact]
    public void ExtractFromText_WithCodeFence_ParsesCorrectly()
    {
        // Arrange
        var text = @"
Here is my intent:

```yaml
goal: Implement caching
scope:
  - CacheService
  - Repository
```
";

        // Act
        var result = _parser.ExtractFromText(text);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Implement caching", result.Value!.Goal);
    }

    [Fact]
    public void ExtractFromText_WithIntentCommand_ParsesCorrectly()
    {
        // Arrange
        var text = @"/intent
goal: Add validation
scope:
  - UserController
verification:
  - Test invalid input handling
";

        // Act
        var result = _parser.ExtractFromText(text);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Add validation", result.Value!.Goal);
    }

    [Fact]
    public void Parse_EmptyYaml_ThrowsException()
    {
        // Act & Assert
        Assert.Throws<IntentParseException>(() => _parser.Parse(""));
        Assert.Throws<IntentParseException>(() => _parser.Parse("   "));
    }

    [Fact]
    public void ToYaml_SerializesIntentCorrectly()
    {
        // Arrange
        var intent = new Intent
        {
            Goal = "Test goal",
            Scope = new List<string> { "Service1", "Service2" },
            Constraints = new List<string> { "No breaking changes" },
            Verification = new List<string> { "Unit tests pass" }
        };

        // Act
        var yaml = _parser.ToYaml(intent);

        // Assert
        Assert.Contains("goal: Test goal", yaml);
        Assert.Contains("Service1", yaml);
        Assert.Contains("Service2", yaml);
        Assert.Contains("No breaking changes", yaml);
    }

    [Fact]
    public void Parse_WithPriority_ParsesCorrectly()
    {
        // Arrange
        var yaml = @"
goal: Critical fix
scope:
  - ErrorHandler
priority: high
";

        // Act
        var intent = _parser.Parse(yaml);

        // Assert
        Assert.Equal(IntentPriority.High, intent.Priority);
    }

    [Fact]
    public void Parse_WithTags_ParsesCorrectly()
    {
        // Arrange
        var yaml = @"
goal: Add feature
scope:
  - Service
tags:
  - feature
  - v2
";

        // Act
        var intent = _parser.Parse(yaml);

        // Assert
        Assert.Equal(2, intent.Tags.Count);
        Assert.Contains("feature", intent.Tags);
        Assert.Contains("v2", intent.Tags);
    }
}
