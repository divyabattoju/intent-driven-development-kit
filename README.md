# Intent-Driven Development Kit (IntentDK)

[![NuGet](https://img.shields.io/nuget/v/IntentDK.svg)](https://www.nuget.org/packages/IntentDK)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

A structured, repeatable, human-readable way to describe development intents. Define goals, scope, constraints, and verification criteria in YAML files for AI agents to generate, execute, and verify code changes safely.

## What is Intent-Driven Development?

Instead of writing vague prompts, you define structured intents in YAML files:

```yaml
goal: Add logging to user login
scope:
  - AuthService
  - Logger
constraints:
  - Must not expose passwords
verification:
  - Unit test for login logs
```

Then use commands to plan, break down tasks, and implement:

```
/intent          → Create new intent file
/intent.plan     → Generate implementation plan
/intent.tasks    → Break down into detailed tasks
/intent.implement → Implement step by step
/intent.verify   → Verify against criteria
```

---

## Quick Start

### 1. Install the CLI Tool

```bash
# Install globally
dotnet tool install -g IntentDK.Cli

# Or from source
dotnet pack src/IntentDK.Cli/IntentDK.Cli.csproj -c Release
dotnet tool install -g --add-source ./src/IntentDK.Cli/nupkg IntentDK.Cli
```

### 2. Initialize in Your Project

```bash
# Interactive - asks which AI tool you use
intent init .

# Or specify directly
intent init . --tool cursor      # For Cursor
intent init . --tool copilot     # For GitHub Copilot
intent init . --tool cody        # For Sourcegraph Cody
intent init . --tool continue    # For Continue
intent init . --tool windsurf    # For Windsurf
intent init . --tool claude      # For Claude (AGENTS.md)
intent init . --all              # For all AI tools
```

This will:
- Create `.intent/` directory for your intent files
- Install the appropriate rules/instructions for your AI tool
- Set up everything you need to start using `/intent` commands

### 3. Create an Intent

In your AI chat (Cursor, Copilot, etc.), type:

```
/intent feature add user authentication
```

This creates `.intent/intent-YYYYMMDD-HHMMSS.intent.yaml` with a template.

### 4. Edit the Intent

Fill in the details:

```yaml
goal: Add JWT-based user authentication

scope:
  - AuthController
  - UserService
  - TokenGenerator

constraints:
  - Use JWT tokens
  - Tokens expire in 24 hours
  - Must hash passwords

verification:
  - Login test passes
  - Token validation test passes
  - Invalid credentials return 401
```

### 5. Plan & Implement

```
/intent.plan      # See the implementation plan
/intent.tasks     # Get detailed task breakdown
/intent.implement # Start implementing
/intent.verify    # Verify when done
```

---

## Commands Reference

| Command | Description |
|---------|-------------|
| `/intent` | Create a new intent YAML file with template |
| `/intent feature <hint>` | Create feature intent template |
| `/intent bugfix <hint>` | Create bug fix intent template |
| `/intent refactor <hint>` | Create refactoring intent template |
| `/intent security <hint>` | Create security enhancement template |
| `/intent.plan` | Generate implementation plan from intent |
| `/intent.tasks` | Break down intent into detailed tasks |
| `/intent.implement` | Implement tasks step by step |
| `/intent.verify` | Verify implementation against criteria |
| `/intent.status` | Show current intent status |

---

## Intent Format

```yaml
# Required fields
goal: <What you want to achieve>
scope:
  - <File, class, or module affected>

# Recommended fields  
constraints:
  - <Requirement that must be respected>
verification:
  - <How to verify success>

# Optional fields
context: <Background information>
priority: low | medium | high | critical
tags:
  - <categorization tag>
```

### Field Descriptions

| Field | Required | Description |
|-------|----------|-------------|
| `goal` | Yes | Clear description of what you want to achieve |
| `scope` | Yes | Files, classes, or modules to modify |
| `constraints` | No | Rules that must be followed |
| `verification` | No | Criteria to confirm success |
| `context` | No | Background information |
| `priority` | No | Urgency level |
| `tags` | No | Categories for organization |

---

## Workflow

### 1. Define Intent (`/intent`)

Creates a YAML file in `.intent/` directory:

```
project/
└── .intent/
    └── intent-20260213-143022.intent.yaml
```

### 2. Generate Plan (`/intent.plan`)

AI reads the intent and generates a structured plan:

```markdown
## Implementation Plan

**Goal:** Add JWT authentication

### Steps
| Step | Action | Target | Description |
|------|--------|--------|-------------|
| 1 | 👀 Review | AuthController | Analyze current auth |
| 2 | ✏️ Modify | UserService | Add JWT generation |
| 3 | ✏️ Modify | AuthController | Add login endpoint |
| 4 | 🧪 Test | AuthTests | Add authentication tests |

### Constraints to Respect
- ⚠️ Use JWT tokens
- ⚠️ Tokens expire in 24 hours
```

### 3. Break Down Tasks (`/intent.tasks`)

Get detailed tasks with acceptance criteria:

```markdown
## Task Breakdown

| ID | Type | Title | Status |
|----|------|-------|--------|
| T1 | 🔍 Analyze | Review current auth | ⏳ Pending |
| T2 | ✏️ Implement | Add JWT generation | ⏳ Pending |
| T3 | 🧪 Test | Add login tests | ⏳ Pending |

### T2: Add JWT generation
**Target:** `UserService`
**Complexity:** 3/5

**Acceptance Criteria:**
- [ ] JWT token generated on login
- [ ] Token includes user claims
- [ ] Token expires in 24 hours
```

### 4. Implement (`/intent.implement`)

AI implements tasks one by one, showing progress:

```markdown
## Implementing Task T2: Add JWT generation

**Status:** 🔄 In Progress

[Makes changes...]

### Task T2 Complete ✓
**Changes made:**
- Added JWT generation in UserService
- Configured token expiration
```

### 5. Verify (`/intent.verify`)

Verify against original criteria:

```markdown
## Verification Report

| Status | Criterion |
|--------|-----------|
| ✅ | Login test passes |
| ✅ | Token validation test passes |
| ✅ | Invalid credentials return 401 |

**Result:** 3/3 passed ✓
```

---

## Examples

### Feature: Add Pagination

```yaml
goal: Add pagination to user list API

scope:
  - UserController
  - UserRepository
  - UserService

constraints:
  - Default page size: 20
  - Max page size: 100
  - Must be backward compatible

verification:
  - Unit test for pagination logic
  - API returns correct page metadata
  - Existing tests still pass
```

### Bug Fix: Race Condition

```yaml
goal: Fix race condition in order processing

scope:
  - OrderProcessor
  - InventoryService

constraints:
  - Must not affect performance by more than 5%
  - Use optimistic locking

verification:
  - Concurrent order test passes
  - No deadlocks under load test

priority: critical
tags:
  - bugfix
```

### Refactoring

```yaml
goal: Extract email logic into dedicated service

scope:
  - UserService
  - NotificationService
  - EmailService (new)

constraints:
  - No changes to external API
  - All existing tests must pass

verification:
  - Unit tests for EmailService
  - Integration test for email flow
  - Code coverage >= 80%

tags:
  - refactor
  - tech-debt
```

---

## Installation

### CLI Tool (Recommended)

Install the `intent` CLI tool to automatically configure your AI coding assistant:

```bash
# Install globally
dotnet tool install -g IntentDK.Cli

# Initialize in your project
cd your-project
intent init .
```

The init command will:
1. Ask which AI tool(s) you use
2. Install the appropriate configuration files
3. Create the `.intent/` directory

**Supported AI Tools:**

| Tool | Config Location | Command |
|------|-----------------|---------|
| Cursor | `.cursor/rules/intent-command.mdc` | `intent init . -t cursor` |
| GitHub Copilot | `.github/copilot-instructions.md` | `intent init . -t copilot` |
| Sourcegraph Cody | `.sourcegraph/cody.json` | `intent init . -t cody` |
| Continue | `.continue/rules.md` | `intent init . -t continue` |
| Windsurf | `.windsurf/rules/intent-command.md` | `intent init . -t windsurf` |
| Aider | `.aider.conf.yml` | `intent init . -t aider` |
| Claude | `AGENTS.md` | `intent init . -t claude` |

### CLI Commands

```bash
intent init [path]              # Initialize IntentDK
intent init . --tool cursor     # Initialize for specific tool
intent init . --all             # Initialize for all tools
intent init . --force           # Overwrite existing files

intent new [type]               # Create new intent file
intent new feature              # Create feature intent
intent new bugfix               # Create bugfix intent
intent new --name my-feature    # Custom name

intent list                     # List all intent files
```

### For .NET Applications (NuGet)

```bash
dotnet add package IntentDK
```

---

## Library API

Use IntentDK programmatically in your .NET applications.

### Create Intent Files

```csharp
using IntentDK.Core;
using IntentDK.Core.Services;

var processor = new IntentProcessor();

// Create a new intent file
var filePath = processor.CreateIntentFileInProject(
    projectRoot: "/path/to/project",
    name: "add-auth",
    template: IntentTemplateType.Feature,
    hint: "user authentication"
);

// Read an intent file
var result = processor.ReadIntentFile(filePath);
if (result.IsSuccess)
{
    var intent = result.Value;
    Console.WriteLine($"Goal: {intent.Goal}");
}
```

### Generate Plans and Tasks

```csharp
using IntentDK.Core;
using IntentDK.Core.Models;

var processor = new IntentProcessor();

var intent = new Intent
{
    Goal = "Add caching",
    Scope = new List<string> { "Repository", "CacheService" },
    Constraints = new List<string> { "Max TTL: 1 hour" },
    Verification = new List<string> { "Cache hit test passes" }
};

// Generate plan
var planMarkdown = processor.CreatePlanMarkdown(intent);

// Generate task breakdown
var tasksMarkdown = processor.CreateTasksMarkdown(intent);

// Create task breakdown object
var taskBreakdown = processor.CreateTasks(intent);
foreach (var task in taskBreakdown.Tasks)
{
    Console.WriteLine($"{task.Id}: {task.Title} ({task.Status})");
}
```

### Fluent Workflow API

```csharp
using IntentDK.Core;

var workflow = IntentProcessor.Create("Add user authentication")
    .WithScope("AuthController", "UserService")
    .WithConstraints("Use JWT tokens", "Tokens expire in 24 hours")
    .WithVerification("Login test passes")
    .WithPriority(IntentPriority.High);

// Generate outputs
var plan = workflow.GetPlanMarkdown();
var tasks = workflow.GetTasksMarkdown();

// After implementation
workflow.CreateChecklist()
    .MarkPassed("Login test", "All scenarios pass")
    .Verify();

var report = workflow.GetVerificationReport();
```

### Get Templates

```csharp
using IntentDK.Core;
using IntentDK.Core.Services;

// Get template content
var featureTemplate = IntentProcessor.GetTemplate(
    IntentTemplateType.Feature, 
    "user authentication"
);

var bugfixTemplate = IntentProcessor.GetTemplate(
    IntentTemplateType.BugFix,
    "login not working"
);
```

---

## Project Structure

```
IntentDK/
├── src/
│   ├── IntentDK.Core/           # Core library (NuGet: IntentDK)
│   │   ├── Models/              # Intent, Plan, Task, VerificationResult
│   │   ├── Parsing/             # YAML parsing
│   │   ├── Planning/            # Plan & Task generation
│   │   ├── Services/            # File operations
│   │   ├── Templates/           # Intent templates
│   │   ├── Verification/        # Verification service
│   │   └── IntentProcessor.cs   # Main API
│   └── IntentDK.Cli/            # CLI tool (NuGet: IntentDK.Cli)
│       ├── AiTools/             # AI tool configurations
│       └── Program.cs           # CLI commands
├── tests/IntentDK.Core.Tests/
└── .cursor/rules/
    └── intent-command.mdc       # Cursor AI rule
```

---

## File Structure

When using IntentDK, files are organized in the `.intent/` directory:

```
project/
├── .intent/
│   ├── intent-20260213-143022.intent.yaml  # Intent definition
│   ├── intent-20260213-143022.plan.yaml    # Generated plan
│   └── intent-20260213-143022.tasks.yaml   # Task breakdown
└── .cursor/
    └── rules/
        └── intent-command.mdc
```

---

## Benefits

| Benefit | Description |
|---------|-------------|
| **Structured Communication** | Clear, consistent way to describe development tasks |
| **File-Based Workflow** | Intents are saved as YAML files for tracking |
| **Scope Control** | Define exactly what should be changed |
| **Safety Constraints** | Explicit requirements that must be respected |
| **Task Breakdown** | Complex intents broken into manageable tasks |
| **Verifiable Outcomes** | Built-in verification criteria |
| **AI-Friendly** | Designed for AI agents to parse and execute |
| **Reproducible** | Same format works across different AI tools |

---

## Building from Source

```bash
# Clone the repository
git clone https://github.com/your-org/Intent-driven-development-kit.git
cd Intent-driven-development-kit

# Build
dotnet build

# Run tests
dotnet test

# Create NuGet package
dotnet pack src/IntentDK.Core/IntentDK.Core.csproj -c Release
```

---

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the LICENSE file for details.
