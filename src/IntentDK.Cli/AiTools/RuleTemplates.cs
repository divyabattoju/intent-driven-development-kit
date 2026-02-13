namespace IntentDK.Cli.AiTools;

/// <summary>
/// Templates for AI tool rules/instructions.
/// </summary>
public static class RuleTemplates
{
    /// <summary>
    /// Gets the rule content for a specific AI tool.
    /// </summary>
    public static string GetRuleContent(AiToolConfig tool)
    {
        return tool.Id switch
        {
            "cursor" => GetCursorRules(),
            "copilot" => GetCopilotInstructions(),
            "cody" => GetCodyConfig(),
            "continue" => GetContinueRules(),
            "aider" => GetAiderConfig(),
            "windsurf" => GetWindsurfRules(),
            "claude" => GetClaudeAgentsMd(),
            "generic" => GetGenericInstructions(),
            _ => GetGenericInstructions()
        };
    }

    private static string GetCursorRules()
    {
        return @"---
description: Intent-Driven Development Kit - /intent commands for structured development workflows
globs: 
alwaysApply: true
---

# Intent-Driven Development Kit (IntentDK)

This rule enables intent-driven development commands. Users define their development goals in structured YAML files, then use commands to plan, break down tasks, and implement.

## Commands Overview

| Command | Description |
|---------|-------------|
| `/intent` | Create a new intent YAML file with template |
| `/intent.plan` | Generate implementation plan from intent file |
| `/intent.tasks` | Break down intent into detailed tasks |
| `/intent.implement` | Implement the tasks step by step |
| `/intent.verify` | Verify implementation against criteria |
| `/intent.status` | Show current intent status and progress |

## `/intent` - Create New Intent

When the user types `/intent`, create a new intent YAML file in `.intent/` directory.

**Variants:**
- `/intent` - Basic template
- `/intent feature <hint>` - Feature template
- `/intent bugfix <hint>` - Bug fix template
- `/intent refactor <hint>` - Refactoring template
- `/intent security <hint>` - Security enhancement template

**Action:**
1. Create `.intent/intent-YYYYMMDD-HHMMSS.intent.yaml`
2. Populate with appropriate template
3. Open file for editing

**Template:**
```yaml
goal: <What you want to achieve>

scope:
  - <File, class, or module to modify>

constraints:
  - <Requirements that MUST be respected>

verification:
  - <How to verify success>
```

## `/intent.plan` - Generate Plan

Read the most recent intent file and generate a structured implementation plan.

**Response Format:**
```markdown
## Implementation Plan

**Goal:** [goal]

### Steps
| Step | Action | Target | Description |
|------|--------|--------|-------------|
| 1 | 👀 Review | [target] | Analyze current state |
| 2 | ✏️ Modify | [file] | [changes] |
| 3 | 🧪 Test | [test] | Add tests |

### Constraints
- ⚠️ [constraint]

### Verification Checklist
- [ ] [criterion]
```

## `/intent.tasks` - Break Down Tasks

Generate detailed task breakdown with dependencies and acceptance criteria.

**Response Format:**
```markdown
## Task Breakdown

| ID | Type | Title | Status |
|----|------|-------|--------|
| T1 | 🔍 Analyze | [title] | ⏳ Pending |
| T2 | ✏️ Implement | [title] | ⏳ Pending |

### Task Details
#### T1: [Title]
**Acceptance Criteria:**
- [ ] [criterion]
```

## `/intent.implement` - Implement

Implement tasks one by one, showing progress:

```markdown
## Implementing Task T1: [Title]
**Status:** 🔄 In Progress

[Make changes]

### Task T1 Complete ✓
```

## `/intent.verify` - Verify

Check implementation against criteria:

```markdown
## Verification Report

| Status | Criterion |
|--------|-----------|
| ✅ | [passed] |
| ❌ | [failed] |

**Result:** X/Y passed
```

## Best Practices

1. Show plan before implementing
2. Wait for confirmation before changes
3. Respect scope boundaries
4. Check constraints continuously
5. Provide evidence in verification
";
    }

    private static string GetCopilotInstructions()
    {
        return @"# Intent-Driven Development Instructions

This repository uses Intent-Driven Development (IntentDK) for structured code changes.

## Commands

When I use these commands, follow the corresponding workflow:

### `/intent` - Create New Intent
Create a new file `.intent/intent-YYYYMMDD-HHMMSS.intent.yaml` with this template:

```yaml
goal: <What to achieve>
scope:
  - <Files/classes to modify>
constraints:
  - <Requirements to respect>
verification:
  - <How to verify success>
```

### `/intent.plan` - Generate Plan
Read the latest `.intent/*.intent.yaml` file and generate:
1. Implementation steps with actions and targets
2. Constraints to respect
3. Verification checklist

### `/intent.tasks` - Break Down Tasks
Create detailed tasks with:
- Task ID, type, and title
- Dependencies between tasks
- Acceptance criteria for each

### `/intent.implement` - Implement
Work through tasks one by one:
1. Show which task you're implementing
2. Make the changes
3. Mark task complete
4. Move to next task

### `/intent.verify` - Verify
Check each verification criterion and report:
- ✅ for passed
- ❌ for failed
- Evidence/reasoning for each

## Intent Format

```yaml
goal: Clear description of what to achieve
scope:
  - List of files, classes, or modules to modify
constraints:
  - Rules that must be followed
verification:
  - Criteria to confirm success
context: Optional background information
priority: low | medium | high | critical
```

## Workflow

1. `/intent` → Create intent file
2. Edit the file with details
3. `/intent.plan` → Review plan
4. `/intent.tasks` → See task breakdown
5. `/intent.implement` → Execute tasks
6. `/intent.verify` → Confirm completion
";
    }

    private static string GetCodyConfig()
    {
        return @"{
  ""instructions"": [
    {
      ""name"": ""Intent-Driven Development"",
      ""description"": ""Commands for structured development workflows"",
      ""content"": ""When the user types /intent commands, follow these workflows:\n\n/intent - Create .intent/intent-YYYYMMDD-HHMMSS.intent.yaml with template\n/intent.plan - Generate implementation plan from intent file\n/intent.tasks - Break down into detailed tasks\n/intent.implement - Implement tasks step by step\n/intent.verify - Verify against criteria\n\nIntent format:\ngoal: What to achieve\nscope: Files to modify\nconstraints: Rules to follow\nverification: How to verify success""
    }
  ],
  ""context"": {
    ""include"": ["".intent/**""],
    ""codebase"": true
  }
}";
    }

    private static string GetContinueRules()
    {
        return @"# Intent-Driven Development Rules

## Commands

### /intent
Create a new intent file at `.intent/intent-YYYYMMDD-HHMMSS.intent.yaml`

Template:
```yaml
goal: <objective>
scope:
  - <files to modify>
constraints:
  - <rules to follow>
verification:
  - <success criteria>
```

### /intent.plan
Generate implementation plan from the latest intent file.

### /intent.tasks
Break down intent into detailed tasks with dependencies.

### /intent.implement
Implement tasks one by one, showing progress.

### /intent.verify
Verify implementation against criteria.

## Workflow
1. Create intent → 2. Plan → 3. Tasks → 4. Implement → 5. Verify
";
    }

    private static string GetAiderConfig()
    {
        return @"# Aider configuration for Intent-Driven Development

# Read intent files for context
read:
  - .intent/*.intent.yaml
  - .intent/*.plan.yaml
  - .intent/*.tasks.yaml

# Intent-driven conventions
conventions: |
  When working on changes, look for intent files in .intent/ directory.
  These files define:
  - goal: What to achieve
  - scope: Files to modify
  - constraints: Rules to follow
  - verification: Success criteria
  
  Respect the scope and constraints defined in intent files.
  Verify against criteria when implementation is complete.
";
    }

    private static string GetWindsurfRules()
    {
        return @"# Intent-Driven Development Kit

## Commands

| Command | Description |
|---------|-------------|
| `/intent` | Create new intent YAML file |
| `/intent.plan` | Generate implementation plan |
| `/intent.tasks` | Break down into tasks |
| `/intent.implement` | Implement step by step |
| `/intent.verify` | Verify against criteria |

## /intent - Create Intent

Create `.intent/intent-YYYYMMDD-HHMMSS.intent.yaml`:

```yaml
goal: <What to achieve>
scope:
  - <Files to modify>
constraints:
  - <Rules to follow>
verification:
  - <Success criteria>
```

## /intent.plan - Generate Plan

Read intent and output:
- Implementation steps
- Constraints to respect
- Verification checklist

## /intent.tasks - Task Breakdown

Generate tasks with:
- ID, type, title
- Dependencies
- Acceptance criteria

## /intent.implement - Execute

For each task:
1. Show task being implemented
2. Make changes
3. Mark complete
4. Proceed to next

## /intent.verify - Verify

Check criteria and report ✅/❌ for each.
";
    }

    private static string GetClaudeAgentsMd()
    {
        return @"# AGENTS.md - Intent-Driven Development

This project uses Intent-Driven Development (IntentDK) for structured code changes.

## Intent Commands

### /intent
Create a new intent file: `.intent/intent-YYYYMMDD-HHMMSS.intent.yaml`

```yaml
goal: What you want to achieve
scope:
  - Files, classes, or modules to modify
constraints:
  - Requirements that must be respected
verification:
  - How to verify success
```

### /intent.plan
Generate an implementation plan:
- Steps with actions and targets
- Constraints to respect
- Verification checklist

### /intent.tasks
Break down into detailed tasks:
- Task ID, type, title
- Dependencies
- Acceptance criteria

### /intent.implement
Implement tasks one by one, showing progress.

### /intent.verify
Verify each criterion with ✅/❌ status.

## Workflow

1. Create intent with `/intent`
2. Edit the YAML file with details
3. Generate plan with `/intent.plan`
4. See tasks with `/intent.tasks`
5. Implement with `/intent.implement`
6. Verify with `/intent.verify`

## File Structure

```
.intent/
├── intent-YYYYMMDD-HHMMSS.intent.yaml  # Intent definition
├── intent-YYYYMMDD-HHMMSS.plan.yaml    # Generated plan
└── intent-YYYYMMDD-HHMMSS.tasks.yaml   # Task breakdown
```
";
    }

    private static string GetGenericInstructions()
    {
        return @"# AI Assistant Instructions - Intent-Driven Development

## Overview

This project uses Intent-Driven Development (IntentDK) for structured code changes.
Copy these instructions to your AI assistant.

## Commands

### /intent
Create a new intent file at `.intent/intent-YYYYMMDD-HHMMSS.intent.yaml`:

```yaml
goal: <What you want to achieve>
scope:
  - <Files, classes, or modules to modify>
constraints:
  - <Requirements that must be respected>
verification:
  - <How to verify success>
```

### /intent.plan
Generate an implementation plan from the intent file with:
- Implementation steps
- Constraints to respect
- Verification checklist

### /intent.tasks
Break down the intent into detailed tasks with:
- Task ID, type, and title
- Dependencies between tasks
- Acceptance criteria

### /intent.implement
Implement tasks one by one:
1. Show current task
2. Make changes
3. Mark complete
4. Move to next

### /intent.verify
Verify implementation against criteria:
- ✅ Passed
- ❌ Failed

## Workflow

```
/intent → Create intent file
   ↓
Edit YAML with details
   ↓
/intent.plan → Review plan
   ↓
/intent.tasks → See tasks
   ↓
/intent.implement → Execute
   ↓
/intent.verify → Confirm
```
";
    }
}
