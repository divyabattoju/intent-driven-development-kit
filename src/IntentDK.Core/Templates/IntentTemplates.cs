namespace IntentDK.Core.Templates;

/// <summary>
/// Provides templates for intent YAML files.
/// </summary>
public static class IntentTemplates
{
    /// <summary>
    /// Default intent file name.
    /// </summary>
    public const string DefaultFileName = "intent.yaml";

    /// <summary>
    /// Gets the basic intent template with example values.
    /// </summary>
    public static string GetBasicTemplate(string? goalHint = null)
    {
        var goal = goalHint ?? "Describe what you want to achieve";
        
        return $@"# Intent-Driven Development
# Edit this file to define your development intent, then run:
#   /intent.plan     - Generate implementation plan
#   /intent.tasks    - Break down into tasks
#   /intent.implement - Implement the tasks

goal: {goal}

scope:
  - # File, class, or module to modify
  - # Add more scope items as needed

constraints:
  - # Requirements that MUST be respected
  - # Security, performance, or compatibility constraints

verification:
  - # How to verify success (tests, checks)
  - # Add verification criteria

# Optional fields (uncomment to use):
# context: Background information or why this is needed
# priority: medium  # low | medium | high | critical
# tags:
#   - feature
";
    }

    /// <summary>
    /// Gets a feature intent template.
    /// </summary>
    public static string GetFeatureTemplate(string? featureName = null)
    {
        var name = featureName ?? "new feature";
        
        return $@"# Intent: Add New Feature
# Run /intent.plan to generate implementation plan

goal: Implement {name}

scope:
  - # Controller or API endpoint
  - # Service layer
  - # Repository or data access
  - # Tests

constraints:
  - Must be backward compatible
  - Follow existing code patterns
  - Include error handling

verification:
  - Unit tests pass
  - Integration test passes
  - API documentation updated

context: |
  Describe the feature requirements and acceptance criteria here.
  Include any relevant business logic or user stories.

priority: medium

tags:
  - feature
";
    }

    /// <summary>
    /// Gets a bug fix intent template.
    /// </summary>
    public static string GetBugFixTemplate(string? bugDescription = null)
    {
        var description = bugDescription ?? "the reported issue";
        
        return $@"# Intent: Bug Fix
# Run /intent.plan to generate implementation plan

goal: Fix {description}

scope:
  - # File(s) where the bug exists
  - # Related test files

constraints:
  - Must not introduce regressions
  - Preserve existing behavior for other cases
  - Add test to prevent recurrence

verification:
  - Bug no longer reproducible
  - Regression test added
  - Existing tests still pass

context: |
  Describe the bug:
  - Steps to reproduce:
  - Expected behavior:
  - Actual behavior:
  - Root cause (if known):

priority: high

tags:
  - bugfix
";
    }

    /// <summary>
    /// Gets a refactoring intent template.
    /// </summary>
    public static string GetRefactorTemplate(string? refactorGoal = null)
    {
        var goal = refactorGoal ?? "improve code quality";
        
        return $@"# Intent: Refactoring
# Run /intent.plan to generate implementation plan

goal: Refactor to {goal}

scope:
  - # Files to refactor
  - # New files to create (if any)
  - # Test files to update

constraints:
  - No changes to external API/interface
  - All existing tests must pass
  - No behavior changes

verification:
  - All tests pass
  - Code coverage maintained or improved
  - No new linter warnings

context: |
  Describe the refactoring goals:
  - Current issues:
  - Desired improvements:
  - Design patterns to apply:

priority: low

tags:
  - refactor
  - tech-debt
";
    }

    /// <summary>
    /// Gets a security enhancement intent template.
    /// </summary>
    public static string GetSecurityTemplate(string? securityGoal = null)
    {
        var goal = securityGoal ?? "enhance security";
        
        return $@"# Intent: Security Enhancement
# Run /intent.plan to generate implementation plan

goal: {goal}

scope:
  - # Security-sensitive files
  - # Configuration files
  - # Test files

constraints:
  - Must not break existing authentication/authorization
  - Follow OWASP guidelines
  - Log security events appropriately
  - No sensitive data in logs

verification:
  - Security tests pass
  - Penetration test scenarios covered
  - No security warnings from static analysis

context: |
  Security requirements:
  - Threat model:
  - Attack vectors to address:
  - Compliance requirements:

priority: critical

tags:
  - security
";
    }

    /// <summary>
    /// Gets a plan template for documenting implementation plans.
    /// </summary>
    public static string GetPlanTemplate(string intentId, string goal)
    {
        return $@"# Implementation Plan
# Generated from intent: {intentId}
# Goal: {goal}

## Overview

summary: |
  Describe the overall approach here.

## Steps

steps:
  - step: 1
    action: review
    target: # file or component
    description: Analyze current implementation
    
  - step: 2
    action: modify
    target: # file or component
    description: Implement changes
    details: |
      Specific changes to make...
    
  - step: 3
    action: test
    target: # test file
    description: Add/update tests

## Risks

risks:
  - # Potential risk or consideration

## Dependencies

dependencies:
  - # External dependencies or prerequisites
";
    }

    /// <summary>
    /// Gets a tasks template for tracking implementation tasks.
    /// </summary>
    public static string GetTasksTemplate(string intentId, string goal)
    {
        return $@"# Implementation Tasks
# Generated from intent: {intentId}
# Goal: {goal}

## Tasks

tasks:
  - id: 1
    title: # Task title
    status: pending  # pending | in_progress | completed | blocked
    description: |
      Task details...
    acceptance_criteria:
      - # Criterion 1
      - # Criterion 2
    
  - id: 2
    title: # Next task
    status: pending
    depends_on: [1]  # Task dependencies
    description: |
      Task details...

## Progress

completed: 0
total: 2
percentage: 0%
";
    }
}
