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
description: Intent-Driven Development Kit - /intent commands. TRIGGER: any user message that starts with /intent (e.g. /intent feature add login, /intent feature add some long thing with something).
globs: 
alwaysApply: true
---

# Intent-Driven Development Kit (IntentDK)

## TRIGGER — When to run this workflow

If the user's message **starts with** `/intent` (with or without more text after it), this is an **intent command**. You MUST run the intent workflow below. Do not treat it as a general question.

Examples that MUST trigger this rule:
- `/intent`
- `/intent feature add user login`
- `/intent feature add some long with some long thing with something`
- `/intent bugfix fix null pointer in checkout`
- `/intent refactor extract payment service`
- Any message whose first word is `/intent` — the rest is the hint/description (even if long).

If you see `/intent` at the start of the message, create the intent file as described below. Do not ask ""what do you mean"" or answer generally — execute the command.

---

This rule enables intent-driven development commands. Users define their development goals in structured YAML files, then use commands to plan, break down tasks, and implement. Keep chat output concise.

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

When the user types `/intent` (or `/intent feature ...`, etc.), create a new intent YAML file in `.intent/` directory. Focus on what to change, not how to change it; the plan and tasks will define the how.

**Parsing the user message:** Take everything after the type word as the description/hint. Examples:
- `/intent feature add some long with some long thing with something` → type=feature, hint=add some long with some long thing with something
- `/intent bugfix fix crash on save` → type=bugfix, hint=fix crash on save
The description can be long; use it as the goal hint and in the filename (sanitized).

**Variants:**
- `/intent` - Basic template (creates `YYYYMMDD-intent-HHMMSS.intent.yaml`)
- `/intent feature <description>` - Feature template (e.g., `YYYYMMDD-feature-add-user-auth.intent.yaml`). Description can be multiple words.
- `/intent bugfix <description>` - Bug fix template (e.g., `YYYYMMDD-bugfix-fix-null-pointer.intent.yaml`)
- `/intent refactor <description>` - Refactoring template (e.g., `YYYYMMDD-refactor-cleanup-services.intent.yaml`)
- `/intent security <description>` - Security enhancement template (e.g., `YYYYMMDD-security-add-csrf.intent.yaml`)

**Action:**
1. Generate filename: `YYYYMMDD-<type>-<sanitized-description>.intent.yaml` where:
   - `YYYYMMDD` is today's date (e.g., 20260217)
   - `<type>` is feature/bugfix/refactor/security/intent
   - `<sanitized-description>` is the description with spaces replaced by hyphens, lowercase
2. Create file in `.intent/` directory with appropriate template
3. Open file for editing
4. **STOP HERE** - Do NOT proceed to planning or implementation. Wait for the user to review and edit the intent file.

**Template:**
```yaml
id: <8-char unique id>
created: <ISO timestamp, e.g., 2026-02-17T14:30:22Z>

goal: <What you want to achieve>

scope:
  - <Focus on what to change, not how. The plan and tasks will define the how.>

constraints:
  - <Requirements that MUST be respected>

verification:
  - <How to verify success>
```

## `/intent.plan` - Generate Plan

Read the most recent intent file and generate a structured implementation plan. Keep steps high-level; detailed steps live in `/intent.tasks`.

**Adaptive Behavior:**
- If the intent file has been updated since the last plan, **regenerate the plan** to reflect the new intent
- If you detect manual code changes that don't match the existing plan, **update the plan** to reflect the current state
- Always check if a `.plan.yaml` already exists - if it does and the intent hasn't changed, ask the user if they want to regenerate it

**Actions:**
1. Read the latest `.intent/*.intent.yaml` file.
2. Generate the plan and **write it to the associated plan file** `.intent/<same-base>.plan.yaml` in YAML format so the user can edit and rejig the plan.
3. Show the plan in chat (markdown) as well and very concise.
4. When referencing files, use relative paths from project root with links.
5. For each change, list the file path (relative) with link and what to change.
6. **STOP HERE** - Do NOT proceed to implementation. Wait for the user to review the plan and explicitly request `/intent.tasks` or `/intent.implement`.

**Plan YAML format** (write this to `.plan.yaml`; the user can edit steps, order, and targets):
```yaml
id: <short-id>
intent_id: <from intent>
intent: intentions
touches:
  - file: <relative path>
    reason: <why this file changes>
  - file: <relative path>
    reason: <why this file changes>
summary: <one-line summary>
steps:
  - step: 1
    action: Review   # or Modify, Create, Delete, Test, Configure, Document
    target: <file or component>
    description: <one-line what to change>
  - step: 2
    action: Modify
    target: <target>
    description: <one-line what to change>
affected_files: [<list of paths>]
risks: [<optional risks>]
dependencies: [<optional dependencies>]
```

**Chat response (markdown, concise):**
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

Plan saved to `.intent/<base>.plan.yaml` — edit that file to rejig the plan, then use `/intent.tasks` or `/intent.implement`.
```

## `/intent.tasks` - Break Down Tasks

Generate detailed task breakdown with dependencies and acceptance criteria. **If a `.plan.yaml` file exists for the intent**, use its steps as the basis for the task breakdown so the user's plan edits are respected.

**Adaptive Behavior:**
- If the intent or plan has been updated, **regenerate tasks** to reflect the changes
- If manual code changes detected, **update tasks** to reflect current progress and remaining work
- If tasks already exist and nothing changed, ask user if they want to regenerate

**Actions:**
1. Read the latest `.intent/*.intent.yaml` (and optionally `.intent/*.plan.yaml` if present).
2. Generate the task breakdown and **write it to the associated tasks file** `.intent/<same-base>.tasks.yaml` in YAML format so the user can edit and rejig tasks.
3. Show the breakdown in chat (markdown) as well.
4. **STOP HERE** - Do NOT proceed to implementation. Wait for the user to review the tasks and explicitly request `/intent.implement`.

**Tasks YAML format** (write this to `.tasks.yaml`; the user can edit order, add/remove tasks, change criteria):
```yaml
intent_id: <from intent>
goal: <one-line goal>
tasks:
  - id: T1
    title: <short title>
    type: Implement   # or Analyze, Create, Test, Review, Document, Configure, Verify
    status: Pending   # or InProgress, Completed, Blocked, Skipped
    target: <file or component>
    description: <what to do>
    acceptance_criteria:
      - <criterion>
    depends_on: []    # list of task IDs if any
    complexity: 3     # 1-5
  - id: T2
    ...
progress:
  completed: 0
  in_progress: 0
  pending: 2
  total: 2
  percentage: 0
```

**Chat response (markdown):**
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

Tasks saved to `.intent/<base>.tasks.yaml` — edit that file to rejig tasks, then use `/intent.implement`.
```

## `/intent.implement` - Implement

**IMPORTANT:** Only run this command when the user explicitly requests it. Do NOT auto-implement after creating intent, plan, or tasks.

Implement tasks one by one, showing progress. **If a `.plan.yaml` file exists**, follow its steps in order. **If a `.tasks.yaml` file exists**, use its tasks (in order and by dependency) as the list to implement so the user's edits (reorder, add, remove, change criteria) are respected.

**Before starting implementation:**
- Check if any manual code changes were already made
- If manual changes detected, note what's already done and focus on remaining work
- Update task status in `.tasks.yaml` to reflect current reality

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

1. **NEVER auto-proceed** - Each command (`/intent`, `/intent.plan`, `/intent.tasks`, `/intent.implement`) must be explicitly requested by the user
2. **Stay adaptive** - Regenerate plan/tasks when intent changes; update them when manual code changes are detected
3. **Keep in sync** - Plan and tasks should always reflect current reality
4. Show plan/tasks and STOP - Wait for user review and confirmation
5. Only implement when user explicitly types `/intent.implement`
6. Respect scope boundaries
7. Check constraints continuously
8. Provide evidence in verification
";
    }

    private static string GetCopilotInstructions()
    {
        return @"# Intent-Driven Development Instructions

This repository uses Intent-Driven Development (IntentDK) for structured code changes.

## Commands

When I use these commands, follow the corresponding workflow:

### `/intent` - Create New Intent
Create a new file `.intent/YYYYMMDD-<type>-<description>.intent.yaml` where:
- YYYYMMDD is today's date (e.g., 20260217)
- type is feature/bugfix/refactor/security/intent
- description is from the command, sanitized (lowercase, hyphens for spaces)

Examples:
- `/intent feature add user auth` → `20260217-feature-add-user-auth.intent.yaml`
- `/intent bugfix fix null pointer` → `20260217-bugfix-fix-null-pointer.intent.yaml`

Template:
```yaml
id: <8-char unique id>
created: <ISO timestamp, e.g., 2026-02-17T14:30:22Z>

goal: <What to achieve>
scope:
  - Focus on what to change, not how to change. The plan and tasks will define the how.
constraints:
  - <Requirements to respect>
verification:
  - <How to verify success>
```

**After creating the intent file, STOP and wait for the user to review it. Do NOT proceed to planning or implementation automatically.**

### `/intent.plan` - Generate Plan
Read the latest `.intent/*.intent.yaml` file and generate a structured implementation plan. Keep steps high-level; detailed steps live in `/intent.tasks`.

**Adaptive Behavior:**
- If the intent file has been updated since the last plan, **regenerate the plan** to reflect the new intent
- If you detect manual code changes that don't match the existing plan, **update the plan** to reflect the current state
- Always check if a `.plan.yaml` already exists - if it does and the intent hasn't changed, ask the user if they want to regenerate it

Also write the plan to the associated plan file so it can be edited later:
- Save to `.intent/<same-base>.plan.yaml` (YAML format)
- Show the plan in chat as markdown and concise
- Use relative paths with markdown links for files
- For each change, list the file path (relative) with link and what to change

**STOP after showing plan. Wait for user review.**

**Plan YAML format** (write this to `.plan.yaml`; the user can edit steps, order, and targets):
```yaml
id: <short-id>
intent_id: <from intent>
intent: intentions
touches:
  - file: <relative path>
    reason: <why this file changes>
  - file: <relative path>
    reason: <why this file changes>
summary: <one-line summary>
steps:
  - step: 1
    action: Review   # or Modify, Create, Delete, Test, Configure, Document
    target: <file or component>
    description: <one-line what to change>
  - step: 2
    action: Modify
    target: <target>
    description: <one-line what to change>
affected_files: [<list of paths>]
risks: [<optional risks>]
dependencies: [<optional dependencies>]
```

### `/intent.tasks` - Break Down Tasks
Create detailed tasks with:
- Task ID, type, and title
- Dependencies between tasks
- Acceptance criteria for each

**Adaptive Behavior:**
- If intent or plan changed, regenerate tasks
- If manual code changes detected, update tasks to reflect progress
- If tasks exist and nothing changed, ask to regenerate

**After generating tasks, STOP and wait for the user to review. Do NOT auto-implement.**

### `/intent.implement` - Implement
**IMPORTANT:** Only run when the user explicitly types `/intent.implement`. Do NOT auto-implement after creating intent, plan, or tasks.

Work through tasks one by one:
1. Check for any manual changes already made
2. Update task status to reflect current reality
3. Show which task you're implementing
4. Make the changes
5. Mark task complete
6. Move to next task

### `/intent.verify` - Verify
Check each verification criterion and report:
- ✅ for passed
- ❌ for failed
- Evidence/reasoning for each

## Intent Format

```yaml
id: <8-char unique id>
created: <ISO timestamp>

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

**IMPORTANT:** Do NOT auto-proceed between steps. Wait for explicit user commands.

1. `/intent` → Create intent file → **STOP, wait for user review**
2. User edits the file with details
3. `/intent.plan` → Generate and show plan → **STOP, wait for user review**
   - If user updates intent and runs `/intent.plan` again, regenerate plan based on new intent
4. `/intent.tasks` → Generate task breakdown → **STOP, wait for user review**
   - If plan/intent changed, regenerate tasks to reflect updates
5. User explicitly types `/intent.implement` → Execute tasks
   - Before implementing, check for manual changes and update task status
6. `/intent.verify` → Confirm completion

**Adaptive:** Plan and tasks stay in sync with intent changes and manual code modifications.
";
    }

    private static string GetCodyConfig()
    {
        return @"{
  ""instructions"": [
    {
      ""name"": ""Intent-Driven Development"",
      ""description"": ""Commands for structured development workflows"",
      ""content"": ""When the user types /intent commands, follow these workflows:\n\n/intent feature <desc> - Create .intent/YYYYMMDD-feature-<desc>.intent.yaml then STOP\n/intent.plan - Generate implementation plan from intent file then STOP, wait for review\n/intent.tasks - Break down into detailed tasks then STOP, wait for review\n/intent.implement - ONLY run when user explicitly types this. Implement tasks step by step\n/intent.verify - Verify against criteria\n\nIMPORTANT: Do NOT auto-proceed between steps. Each step requires explicit user command.\n\nIntent format includes id, created timestamp, goal, scope, constraints, verification""
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
Create a new intent file at `.intent/YYYYMMDD-<type>-<description>.intent.yaml` (e.g., `20260217-feature-add-auth.intent.yaml`)

Template:
```yaml
id: <8-char unique id>
created: <ISO timestamp>

goal: <objective>
scope:
  - <files to modify>
constraints:
  - <rules to follow>
verification:
  - <success criteria>
```

**STOP after creating the file. Wait for user review.**

### /intent.plan
Generate implementation plan from the latest intent file. **STOP after showing the plan. Wait for user to review.**

**Adaptive Behavior:**
- If intent was updated, regenerate the plan based on new intent
- If manual code changes detected that don't match plan, update plan to reflect reality
- If plan exists and intent unchanged, ask user if they want to regenerate

### /intent.tasks
Break down intent into detailed tasks with dependencies. **STOP after showing tasks. Wait for user review.**

**Adaptive Behavior:**
- If intent/plan changed, regenerate tasks
- If manual changes detected, update tasks to reflect progress
- If tasks exist and nothing changed, ask to regenerate

### /intent.implement
**ONLY run when user explicitly types this command.** Do NOT auto-implement.

Before implementing:
1. Check for manual changes already made
2. Update task status to reflect reality

Then implement tasks one by one, showing progress.

### /intent.verify
Verify implementation against criteria.

## Workflow
1. Create intent → **STOP** → 2. Plan → **STOP** → 3. Tasks → **STOP** → 4. User types `/intent.implement` → Implement → 5. Verify

**IMPORTANT:** Do NOT auto-proceed between steps. Each step requires explicit user command.

**Adaptive:** If user updates intent and reruns `/intent.plan`, regenerate plan. If manual code changes detected, update plan/tasks.
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

Create `.intent/YYYYMMDD-<type>-<description>.intent.yaml` (e.g., `20260217-feature-add-auth.intent.yaml`):

```yaml
id: <8-char unique id>
created: <ISO timestamp>

goal: <What to achieve>
scope:
  - <Files to modify>
constraints:
  - <Rules to follow>
verification:
  - <Success criteria>
```

**STOP after creating. Wait for user review.**

## /intent.plan - Generate Plan

Read intent and output:
- Implementation steps
- Constraints to respect
- Verification checklist

**Adaptive Behavior:**
- If intent changed, regenerate plan
- If manual code changes detected, update plan
- If plan exists and intent unchanged, ask to regenerate

**STOP after showing plan. Wait for user review.**

## /intent.tasks - Task Breakdown

Generate tasks with ID, type, title, dependencies, acceptance criteria. Write the breakdown to `.intent/<same-base>.tasks.yaml` so the user can rejig tasks.

**Adaptive Behavior:**
- If intent/plan changed, regenerate tasks
- If manual changes detected, update tasks and progress
- If tasks exist and nothing changed, ask to regenerate

**STOP after showing tasks. Wait for user review.**

## /intent.implement - Execute

**ONLY run when user explicitly types `/intent.implement`.** Do NOT auto-implement.

Before starting:
1. Check for any manual code changes already made
2. Update task status to reflect current reality
3. Focus on remaining work

For each task: show task, make changes, mark complete, next. If `.tasks.yaml` exists, use its tasks (order and dependency) as the list to implement.

## /intent.verify - Verify

Check criteria and report ✅/❌ for each.
";
    }

    private static string GetClaudeAgentsMd()
    {
        return @"# CLAUDE.md - Intent-Driven Development

This project uses Intent-Driven Development (IntentDK) for structured code changes.

## Intent Commands

### /intent
Create a new intent file: `.intent/YYYYMMDD-<type>-<description>.intent.yaml` (e.g., `20260217-feature-add-auth.intent.yaml`)

```yaml
id: <8-char unique id>
created: <ISO timestamp>

goal: What you want to achieve
scope:
  - Files, classes, or modules to modify
constraints:
  - Requirements that must be respected
verification:
  - How to verify success
```

**STOP after creating. Wait for user review.**

### /intent.plan
Generate an implementation plan:
- Steps with actions and targets
- Constraints to respect
- Verification checklist

**Adaptive Behavior:**
- If intent changed, regenerate plan based on updated intent
- If manual code changes detected, update plan to reflect current state
- If plan exists and intent unchanged, ask user if they want to regenerate

**STOP after showing plan. Wait for user review.**

### /intent.tasks
Break down into detailed tasks:
- Task ID, type, title
- Dependencies
- Acceptance criteria

**Adaptive Behavior:**
- If intent/plan changed, regenerate tasks based on updates
- If manual code changes detected, update tasks to reflect current state
- If tasks exist and nothing changed, ask user if they want to regenerate

**STOP after showing tasks. Wait for user review.**

### /intent.implement
**ONLY run when user explicitly types this command.** Do NOT auto-implement.

Before implementing:
1. Check for manual changes already made
2. Update task status to reflect current state
3. Focus on remaining work

Implement tasks one by one, showing progress.

### /intent.verify
Verify each criterion with ✅/❌ status.

## Workflow

**IMPORTANT:** Do NOT auto-proceed between steps. Each step requires explicit user command.

1. Create intent with `/intent` → **STOP**
2. Edit the YAML file with details
3. Generate plan with `/intent.plan` → **STOP**
   - Rerun `/intent.plan` after editing intent to regenerate plan
4. See tasks with `/intent.tasks` → **STOP**
   - Tasks regenerate if plan/intent changed
5. User explicitly types `/intent.implement` → Implement
   - Checks for manual changes before implementing
6. Verify with `/intent.verify`

**Adaptive:** Plan/tasks update when intent changes or manual code changes detected.

## File Structure

```
.intent/
├── 20260217-feature-add-auth.intent.yaml  # Intent definition
├── 20260217-feature-add-auth.plan.yaml    # Generated plan
└── 20260217-feature-add-auth.tasks.yaml   # Task breakdown
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
Create a new intent file at `.intent/YYYYMMDD-<type>-<description>.intent.yaml` (e.g., `20260217-feature-add-auth.intent.yaml`):

```yaml
id: <8-char unique id>
created: <ISO timestamp>

goal: <What you want to achieve>
scope:
  - <Files, classes, or modules to modify>
constraints:
  - <Requirements that must be respected>
verification:
  - <How to verify success>
```

**STOP after creating the intent file. Wait for user review.**

### /intent.plan
Generate an implementation plan from the intent file with:
- Implementation steps
- Constraints to respect
- Verification checklist

**Adaptive Behavior:**
- If intent changed, regenerate plan based on updated intent
- If manual code changes detected that don't match plan, update plan to reflect reality
- If plan exists and intent unchanged, ask user if they want to regenerate

**STOP after showing plan. Wait for user review.**

### /intent.tasks
Break down the intent into detailed tasks (ID, type, title, dependencies, acceptance criteria). Write to `.intent/<same-base>.tasks.yaml` so the user can rejig tasks.

**Adaptive Behavior:**
- If intent/plan changed, regenerate tasks
- If manual code changes detected, update tasks to reflect current progress
- If tasks exist and nothing changed, ask to regenerate

**STOP after showing tasks. Wait for user review.**

### /intent.implement
**ONLY run when user explicitly types this command.** Do NOT auto-implement after /intent, /intent.plan, or /intent.tasks.

Before implementing:
1. Check for manual changes already made
2. Update task status to reflect current state
3. Focus on remaining work

Implement tasks one by one. If `.tasks.yaml` exists, use its tasks as the list to implement; otherwise follow the plan or intent.

### /intent.verify
Verify implementation against criteria:
- ✅ Passed
- ❌ Failed

## Workflow

```
/intent → Create intent file → STOP
   ↓
Edit YAML with details
   ↓
/intent.plan → Review plan → STOP
   ↓ (Rerun if intent changed to regenerate plan)
Edit plan if needed
   ↓
/intent.tasks → See tasks → STOP
   ↓ (Tasks regenerate if plan/intent changed)
Edit tasks if needed
   ↓
/intent.implement → Execute (checks for manual changes first)
   ↓
/intent.verify → Confirm

**Adaptive:** Plan/tasks regenerate when intent changes. Manual code changes trigger plan/task updates.
```
";
    }
}
