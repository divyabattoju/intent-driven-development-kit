using YamlDotNet.Serialization;

namespace IntentDK.Core.Models;

/// <summary>
/// Represents a breakdown of tasks to implement an intent.
/// </summary>
public class TaskBreakdown
{
    /// <summary>
    /// Reference to the intent ID.
    /// </summary>
    [YamlMember(Alias = "intent_id")]
    public string IntentId { get; set; } = string.Empty;

    /// <summary>
    /// The goal from the intent.
    /// </summary>
    [YamlMember(Alias = "goal")]
    public string Goal { get; set; } = string.Empty;

    /// <summary>
    /// List of tasks to complete.
    /// </summary>
    [YamlMember(Alias = "tasks")]
    public List<ImplementationTask> Tasks { get; set; } = new();

    /// <summary>
    /// Current progress summary.
    /// </summary>
    [YamlMember(Alias = "progress")]
    public TaskProgress Progress { get; set; } = new();

    /// <summary>
    /// Timestamp when the breakdown was created.
    /// </summary>
    [YamlIgnore]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets the next pending task.
    /// </summary>
    public ImplementationTask? GetNextTask()
    {
        return Tasks.FirstOrDefault(t => t.Status == TaskStatus.Pending && !t.IsBlocked);
    }

    /// <summary>
    /// Gets all tasks that are ready to work on.
    /// </summary>
    public IEnumerable<ImplementationTask> GetReadyTasks()
    {
        var completedIds = Tasks
            .Where(t => t.Status == TaskStatus.Completed)
            .Select(t => t.Id)
            .ToHashSet();

        return Tasks.Where(t => 
            t.Status == TaskStatus.Pending && 
            t.DependsOn.All(d => completedIds.Contains(d)));
    }

    /// <summary>
    /// Updates progress based on task statuses.
    /// </summary>
    public void UpdateProgress()
    {
        Progress.Total = Tasks.Count;
        Progress.Completed = Tasks.Count(t => t.Status == TaskStatus.Completed);
        Progress.InProgress = Tasks.Count(t => t.Status == TaskStatus.InProgress);
        Progress.Blocked = Tasks.Count(t => t.Status == TaskStatus.Blocked);
        Progress.Pending = Tasks.Count(t => t.Status == TaskStatus.Pending);
        Progress.Percentage = Progress.Total > 0 
            ? (int)((double)Progress.Completed / Progress.Total * 100) 
            : 0;
    }
}

/// <summary>
/// Progress tracking for task breakdown.
/// </summary>
public class TaskProgress
{
    [YamlMember(Alias = "completed")]
    public int Completed { get; set; }

    [YamlMember(Alias = "in_progress")]
    public int InProgress { get; set; }

    [YamlMember(Alias = "pending")]
    public int Pending { get; set; }

    [YamlMember(Alias = "blocked")]
    public int Blocked { get; set; }

    [YamlMember(Alias = "total")]
    public int Total { get; set; }

    [YamlMember(Alias = "percentage")]
    public int Percentage { get; set; }
}

/// <summary>
/// Represents a single implementation task.
/// </summary>
public class ImplementationTask
{
    /// <summary>
    /// Unique identifier for the task.
    /// </summary>
    [YamlMember(Alias = "id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Short title for the task.
    /// </summary>
    [YamlMember(Alias = "title")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Detailed description of what to do.
    /// </summary>
    [YamlMember(Alias = "description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Type of task.
    /// </summary>
    [YamlMember(Alias = "type")]
    public TaskType Type { get; set; } = TaskType.Implement;

    /// <summary>
    /// Current status.
    /// </summary>
    [YamlMember(Alias = "status")]
    public TaskStatus Status { get; set; } = TaskStatus.Pending;

    /// <summary>
    /// Target file or component.
    /// </summary>
    [YamlMember(Alias = "target")]
    public string? Target { get; set; }

    /// <summary>
    /// Acceptance criteria for this task.
    /// </summary>
    [YamlMember(Alias = "acceptance_criteria")]
    public List<string> AcceptanceCriteria { get; set; } = new();

    /// <summary>
    /// IDs of tasks this depends on.
    /// </summary>
    [YamlMember(Alias = "depends_on")]
    public List<string> DependsOn { get; set; } = new();

    /// <summary>
    /// Estimated complexity (1-5).
    /// </summary>
    [YamlMember(Alias = "complexity")]
    public int Complexity { get; set; } = 1;

    /// <summary>
    /// Notes or comments.
    /// </summary>
    [YamlMember(Alias = "notes")]
    public string? Notes { get; set; }

    /// <summary>
    /// Whether this task is blocked by dependencies.
    /// </summary>
    [YamlIgnore]
    public bool IsBlocked => Status == TaskStatus.Blocked;
}

/// <summary>
/// Types of implementation tasks.
/// </summary>
public enum TaskType
{
    /// <summary>Analyze existing code.</summary>
    Analyze,
    
    /// <summary>Design or plan.</summary>
    Design,
    
    /// <summary>Create new file/component.</summary>
    Create,
    
    /// <summary>Modify existing code.</summary>
    Implement,
    
    /// <summary>Write tests.</summary>
    Test,
    
    /// <summary>Review changes.</summary>
    Review,
    
    /// <summary>Update documentation.</summary>
    Document,
    
    /// <summary>Configure settings.</summary>
    Configure,
    
    /// <summary>Verify/validate.</summary>
    Verify
}

/// <summary>
/// Status of an implementation task.
/// </summary>
public enum TaskStatus
{
    /// <summary>Not started.</summary>
    Pending,
    
    /// <summary>Currently being worked on.</summary>
    InProgress,
    
    /// <summary>Completed successfully.</summary>
    Completed,
    
    /// <summary>Blocked by dependencies or issues.</summary>
    Blocked,
    
    /// <summary>Skipped/not needed.</summary>
    Skipped
}
