using IntentDK.Core.Models;
using IntentDK.Core.Parsing;
using IntentDK.Core.Planning;
using IntentDK.Core.Services;
using IntentDK.Core.Templates;
using IntentDK.Core.Verification;

namespace IntentDK.Core;

/// <summary>
/// Main processor for intent-driven development workflow.
/// Provides the core API for parsing, planning, implementing, and verifying intents.
/// </summary>
public class IntentProcessor
{
    private readonly IntentParser _parser;
    private readonly PlanGenerator _planGenerator;
    private readonly TaskGenerator _taskGenerator;
    private readonly VerificationService _verificationService;
    private readonly IntentFileService _fileService;

    public IntentProcessor()
    {
        _parser = new IntentParser();
        _planGenerator = new PlanGenerator();
        _taskGenerator = new TaskGenerator();
        _verificationService = new VerificationService();
        _fileService = new IntentFileService();
    }

    public IntentProcessor(PlanGeneratorOptions planOptions)
    {
        _parser = new IntentParser();
        _planGenerator = new PlanGenerator(planOptions);
        _taskGenerator = new TaskGenerator();
        _verificationService = new VerificationService();
        _fileService = new IntentFileService();
    }

    /// <summary>
    /// Parses an intent from YAML text.
    /// </summary>
    public ParseResult<Intent> Parse(string yaml)
    {
        return _parser.ParseAndValidate(yaml);
    }

    /// <summary>
    /// Extracts and parses an intent from text (supports /intent command format).
    /// </summary>
    public ParseResult<Intent> ExtractIntent(string text)
    {
        return _parser.ExtractFromText(text);
    }

    /// <summary>
    /// Generates an implementation plan from an intent.
    /// </summary>
    public Plan CreatePlan(Intent intent)
    {
        return _planGenerator.GeneratePlan(intent);
    }

    /// <summary>
    /// Generates a plan as formatted markdown.
    /// </summary>
    public string CreatePlanMarkdown(Intent intent)
    {
        return _planGenerator.GeneratePlanMarkdown(intent);
    }

    /// <summary>
    /// Generates a plan as formatted text.
    /// </summary>
    public string CreatePlanText(Intent intent)
    {
        return _planGenerator.GeneratePlanText(intent);
    }

    /// <summary>
    /// Creates a verification checklist from an intent.
    /// </summary>
    public VerificationChecklist CreateVerificationChecklist(Intent intent)
    {
        return _verificationService.CreateChecklist(intent);
    }

    /// <summary>
    /// Evaluates a verification checklist and generates results.
    /// </summary>
    public VerificationResult Verify(VerificationChecklist checklist)
    {
        return _verificationService.Evaluate(checklist);
    }

    /// <summary>
    /// Generates a verification report.
    /// </summary>
    public string GenerateVerificationReport(VerificationResult result, ReportFormat format = ReportFormat.Markdown)
    {
        return _verificationService.GenerateReport(result, format);
    }

    /// <summary>
    /// Serializes an intent to YAML.
    /// </summary>
    public string ToYaml(Intent intent)
    {
        return _parser.ToYaml(intent);
    }

    /// <summary>
    /// Serializes a plan to YAML.
    /// </summary>
    public string ToYaml(Plan plan)
    {
        return _parser.ToYaml(plan);
    }

    /// <summary>
    /// Serializes a task breakdown to YAML.
    /// </summary>
    public string ToYaml(TaskBreakdown breakdown)
    {
        return _parser.ToYaml(breakdown);
    }

    #region Task Generation

    /// <summary>
    /// Generates a task breakdown from an intent.
    /// </summary>
    public TaskBreakdown CreateTasks(Intent intent)
    {
        return _taskGenerator.GenerateTasks(intent);
    }

    /// <summary>
    /// Generates tasks as YAML.
    /// </summary>
    public string CreateTasksYaml(Intent intent)
    {
        return _taskGenerator.GenerateTasksYaml(intent);
    }

    /// <summary>
    /// Generates tasks as Markdown.
    /// </summary>
    public string CreateTasksMarkdown(Intent intent)
    {
        return _taskGenerator.GenerateTasksMarkdown(intent);
    }

    /// <summary>
    /// Generates tasks as a checklist.
    /// </summary>
    public string CreateTasksChecklist(Intent intent)
    {
        return _taskGenerator.GenerateTasksChecklist(intent);
    }

    #endregion

    #region File Operations

    /// <summary>
    /// Creates a new intent file with a template.
    /// </summary>
    /// <param name="directory">Directory to create the file in.</param>
    /// <param name="name">Optional name for the intent.</param>
    /// <param name="template">Template type to use.</param>
    /// <param name="hint">Optional hint for the goal.</param>
    /// <returns>Path to the created file.</returns>
    public string CreateIntentFile(
        string directory,
        string? name = null,
        IntentTemplateType template = IntentTemplateType.Basic,
        string? hint = null)
    {
        return _fileService.CreateIntentFile(directory, name, template, hint);
    }

    /// <summary>
    /// Creates a new intent file in the .intent directory of a project.
    /// </summary>
    public string CreateIntentFileInProject(
        string projectRoot,
        string? name = null,
        IntentTemplateType template = IntentTemplateType.Basic,
        string? hint = null)
    {
        return _fileService.CreateIntentFileInProject(projectRoot, name, template, hint);
    }

    /// <summary>
    /// Reads an intent from a file.
    /// </summary>
    public ParseResult<Intent> ReadIntentFile(string filePath)
    {
        return _fileService.ReadIntent(filePath);
    }

    /// <summary>
    /// Finds the most recent intent file in a project.
    /// </summary>
    public string? FindIntentFile(string projectRoot, string? name = null)
    {
        return _fileService.FindIntentFile(projectRoot, name);
    }

    /// <summary>
    /// Gets all intent files in a project.
    /// </summary>
    public IEnumerable<IntentFileInfo> GetAllIntents(string projectRoot)
    {
        return _fileService.GetAllIntents(projectRoot);
    }

    /// <summary>
    /// Creates a plan file for an intent (YAML format so the user can edit and re-run).
    /// </summary>
    public string CreatePlanFile(string intentFilePath, Intent intent)
    {
        var plan = CreatePlan(intent);
        var planYaml = ToYaml(plan);
        return _fileService.CreatePlanFile(intentFilePath, planYaml);
    }

    /// <summary>
    /// Reads the plan file associated with an intent file (e.g. .intent/intent-xxx.plan.yaml).
    /// Use this so tasks/implement can use the user's edited plan when present.
    /// </summary>
    public ParseResult<Plan> ReadPlanFile(string intentFilePath)
    {
        return _fileService.ReadPlan(intentFilePath);
    }

    /// <summary>
    /// Creates a tasks file for an intent (YAML format so the user can edit and re-run).
    /// </summary>
    public string CreateTasksFile(string intentFilePath, Intent intent)
    {
        var breakdown = CreateTasks(intent);
        var tasksYaml = ToYaml(breakdown);
        return _fileService.CreateTasksFile(intentFilePath, tasksYaml);
    }

    /// <summary>
    /// Reads the tasks file associated with an intent file (e.g. .intent/intent-xxx.tasks.yaml).
    /// Use this so implement can use the user's edited tasks when present.
    /// </summary>
    public ParseResult<TaskBreakdown> ReadTasksFile(string intentFilePath)
    {
        return _fileService.ReadTasks(intentFilePath);
    }

    /// <summary>
    /// Gets an intent template.
    /// </summary>
    public static string GetTemplate(IntentTemplateType template = IntentTemplateType.Basic, string? hint = null)
    {
        return template switch
        {
            IntentTemplateType.Feature => IntentTemplates.GetFeatureTemplate(hint),
            IntentTemplateType.BugFix => IntentTemplates.GetBugFixTemplate(hint),
            IntentTemplateType.Refactor => IntentTemplates.GetRefactorTemplate(hint),
            IntentTemplateType.Security => IntentTemplates.GetSecurityTemplate(hint),
            _ => IntentTemplates.GetBasicTemplate(hint)
        };
    }

    #endregion

    /// <summary>
    /// Creates a new intent workflow with fluent API.
    /// </summary>
    public static IntentWorkflow Create(string goal)
    {
        return new IntentWorkflow(goal);
    }

    /// <summary>
    /// Creates a new intent workflow from YAML.
    /// </summary>
    public static IntentWorkflow FromYaml(string yaml)
    {
        var processor = new IntentProcessor();
        var result = processor.Parse(yaml);
        
        if (!result.IsSuccess)
        {
            throw new IntentParseException(string.Join("; ", result.Errors));
        }

        return new IntentWorkflow(result.Value!);
    }
}

/// <summary>
/// Fluent workflow builder for intents.
/// </summary>
public class IntentWorkflow
{
    private readonly Intent _intent;
    private Plan? _plan;
    private TaskBreakdown? _taskBreakdown;
    private VerificationChecklist? _checklist;
    private VerificationResult? _verificationResult;
    private readonly IntentProcessor _processor;

    public IntentWorkflow(string goal)
    {
        _intent = new Intent { Goal = goal };
        _processor = new IntentProcessor();
    }

    internal IntentWorkflow(Intent intent)
    {
        _intent = intent;
        _processor = new IntentProcessor();
    }

    /// <summary>
    /// Gets the underlying intent.
    /// </summary>
    public Intent Intent => _intent;

    /// <summary>
    /// Gets the generated plan (if created).
    /// </summary>
    public Plan? Plan => _plan;

    /// <summary>
    /// Gets the task breakdown (if created).
    /// </summary>
    public TaskBreakdown? TaskBreakdown => _taskBreakdown;

    /// <summary>
    /// Gets the verification checklist (if created).
    /// </summary>
    public VerificationChecklist? Checklist => _checklist;

    /// <summary>
    /// Gets the verification result (if verified).
    /// </summary>
    public VerificationResult? VerificationResult => _verificationResult;

    /// <summary>
    /// Adds scope items to the intent.
    /// </summary>
    public IntentWorkflow WithScope(params string[] scope)
    {
        _intent.Scope.AddRange(scope);
        return this;
    }

    /// <summary>
    /// Adds constraints to the intent.
    /// </summary>
    public IntentWorkflow WithConstraints(params string[] constraints)
    {
        _intent.Constraints.AddRange(constraints);
        return this;
    }

    /// <summary>
    /// Adds verification criteria to the intent.
    /// </summary>
    public IntentWorkflow WithVerification(params string[] verification)
    {
        _intent.Verification.AddRange(verification);
        return this;
    }

    /// <summary>
    /// Sets the priority.
    /// </summary>
    public IntentWorkflow WithPriority(IntentPriority priority)
    {
        _intent.Priority = priority;
        return this;
    }

    /// <summary>
    /// Adds context information.
    /// </summary>
    public IntentWorkflow WithContext(string context)
    {
        _intent.Context = context;
        return this;
    }

    /// <summary>
    /// Adds tags for categorization.
    /// </summary>
    public IntentWorkflow WithTags(params string[] tags)
    {
        _intent.Tags.AddRange(tags);
        return this;
    }

    /// <summary>
    /// Validates the intent.
    /// </summary>
    public IntentWorkflow Validate()
    {
        var result = _intent.Validate();
        if (!result.IsValid)
        {
            throw new InvalidOperationException(
                $"Intent validation failed: {string.Join(", ", result.Errors)}");
        }
        return this;
    }

    /// <summary>
    /// Creates an implementation plan.
    /// </summary>
    public IntentWorkflow CreatePlan()
    {
        Validate();
        _plan = _processor.CreatePlan(_intent);
        _intent.Status = IntentStatus.Planned;
        return this;
    }

    /// <summary>
    /// Gets the plan as markdown.
    /// </summary>
    public string GetPlanMarkdown()
    {
        if (_plan == null)
        {
            CreatePlan();
        }
        return _processor.CreatePlanMarkdown(_intent);
    }

    /// <summary>
    /// Gets the plan as text.
    /// </summary>
    public string GetPlanText()
    {
        if (_plan == null)
        {
            CreatePlan();
        }
        return _processor.CreatePlanText(_intent);
    }

    /// <summary>
    /// Creates a task breakdown.
    /// </summary>
    public IntentWorkflow CreateTasks()
    {
        Validate();
        _taskBreakdown = _processor.CreateTasks(_intent);
        return this;
    }

    /// <summary>
    /// Gets the tasks as markdown.
    /// </summary>
    public string GetTasksMarkdown()
    {
        if (_taskBreakdown == null)
        {
            CreateTasks();
        }
        return _processor.CreateTasksMarkdown(_intent);
    }

    /// <summary>
    /// Gets the tasks as YAML.
    /// </summary>
    public string GetTasksYaml()
    {
        if (_taskBreakdown == null)
        {
            CreateTasks();
        }
        return _processor.CreateTasksYaml(_intent);
    }

    /// <summary>
    /// Gets the tasks as a checklist.
    /// </summary>
    public string GetTasksChecklist()
    {
        if (_taskBreakdown == null)
        {
            CreateTasks();
        }
        return _processor.CreateTasksChecklist(_intent);
    }

    /// <summary>
    /// Creates a verification checklist.
    /// </summary>
    public IntentWorkflow CreateChecklist()
    {
        _checklist = _processor.CreateVerificationChecklist(_intent);
        return this;
    }

    /// <summary>
    /// Marks a verification item as passed.
    /// </summary>
    public IntentWorkflow MarkPassed(string criterionContains, string? notes = null)
    {
        if (_checklist == null)
        {
            CreateChecklist();
        }

        var item = _checklist!.Items.FirstOrDefault(i => 
            i.Criterion.Contains(criterionContains, StringComparison.OrdinalIgnoreCase));
        
        if (item != null)
        {
            item.Status = ChecklistItemStatus.Passed;
            item.Notes = notes;
            item.CompletedAt = DateTime.UtcNow;
        }

        return this;
    }

    /// <summary>
    /// Marks a verification item as failed.
    /// </summary>
    public IntentWorkflow MarkFailed(string criterionContains, string? notes = null)
    {
        if (_checklist == null)
        {
            CreateChecklist();
        }

        var item = _checklist!.Items.FirstOrDefault(i => 
            i.Criterion.Contains(criterionContains, StringComparison.OrdinalIgnoreCase));
        
        if (item != null)
        {
            item.Status = ChecklistItemStatus.Failed;
            item.Notes = notes;
            item.CompletedAt = DateTime.UtcNow;
        }

        return this;
    }

    /// <summary>
    /// Verifies the intent implementation.
    /// </summary>
    public IntentWorkflow Verify()
    {
        if (_checklist == null)
        {
            CreateChecklist();
        }

        _verificationResult = _processor.Verify(_checklist!);
        _intent.Status = _verificationResult.Status == VerificationStatus.Passed 
            ? IntentStatus.Completed 
            : IntentStatus.Failed;

        return this;
    }

    /// <summary>
    /// Gets the verification report.
    /// </summary>
    public string GetVerificationReport(ReportFormat format = ReportFormat.Markdown)
    {
        if (_verificationResult == null)
        {
            Verify();
        }
        return _processor.GenerateVerificationReport(_verificationResult!, format);
    }

    /// <summary>
    /// Exports the intent to YAML.
    /// </summary>
    public string ToYaml()
    {
        return _processor.ToYaml(_intent);
    }
}
